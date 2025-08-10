using Microsoft.AspNetCore.SignalR;
using RealTimeIntelligence.Web.Hubs;
using Serilog;

namespace RealTimeIntelligence.Web.Services;

public class RealTimeCoordinator : IRealTimeCoordinator, IAsyncDisposable
{
	private readonly IScreenCaptureService _screen;
	private readonly IOcrService _ocr;
	private readonly IMicrophoneService _mic;
	private readonly IAIAnalysisService _ai;
	private readonly ActivityLogService _log;
	private readonly IHubContext<ActivityHub>? _hub;
	private readonly Serilog.ILogger _logger;
	private CancellationTokenSource? _cts;
	private Task? _loopTask;
	public bool IsRunning => _cts != null && !_cts.IsCancellationRequested;

	public RealTimeCoordinator(IScreenCaptureService screen, IOcrService ocr, IMicrophoneService mic, IAIAnalysisService ai, ActivityLogService log, IHubContext<ActivityHub>? hubContext = null, Serilog.ILogger? logger = null)
	{
		_screen = screen;
		_ocr = ocr;
		_mic = mic;
		_ai = ai;
		_log = log;
		_hub = hubContext;
		_logger = logger ?? Log.ForContext<RealTimeCoordinator>();
	}

	public async Task StartAsync(CancellationToken ct = default)
	{
		if (IsRunning) return;
		_cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
		await _mic.StartAsync(_cts.Token);
		_mic.Transcription += OnTranscription;
		await _log.LogActivityAsync("🚀 Real-time coordinator started", ActivityType.Success);
		_loopTask = Task.Run(() => LoopAsync(_cts.Token));
	}

	private async Task LoopAsync(CancellationToken ct)
	{
		while (!ct.IsCancellationRequested)
		{
			try
			{
				var capture = await _screen.CaptureAsync(ct);
				if (capture.Success && capture.ImageBytes != null)
				{
					string text = capture.Text ?? await _ocr.ExtractTextAsync(capture.ImageBytes, ct);
					if (!string.IsNullOrWhiteSpace(text))
					{
						await _log.LogOcrActivityAsync(text);
						var aiContext = $"Screen text: {text[..Math.Min(text.Length, 500)]}";
						var aiResult = await _ai.AnalyzeAsync(aiContext, ct);
						await _log.LogAiAnalysisAsync(aiResult);
						if (_hub != null)
						{
							await _hub.Clients.All.SendAsync("ScreenAnalyzed", new { text, ai = aiResult, ts = DateTime.UtcNow });
						}
					}
				}
				await Task.Delay(3000, ct); // basic cadence
			}
			catch (OperationCanceledException) { }
			catch (Exception ex)
			{
				_logger.Error(ex, "Coordinator loop error");
				await _log.LogErrorAsync(ex.Message);
				await Task.Delay(2000, ct);
			}
		}
	}

	private async void OnTranscription(object? sender, string text)
	{
		try
		{
			await _log.LogVoiceActivityAsync(text);
			var ai = await _ai.AnalyzeAsync($"User said: {text}");
			await _log.LogAiAnalysisAsync(ai);
			if (_hub != null)
			{
				await _hub.Clients.All.SendAsync("VoiceAnalyzed", new { text, ai, ts = DateTime.UtcNow });
			}
		}
		catch (Exception ex)
		{
			_logger.Error(ex, "Voice transcription handling failed");
		}
	}

	public async Task StopAsync()
	{
		if (!IsRunning) return;
		_cts?.Cancel();
		if (_loopTask != null)
		{
			try { await _loopTask; } catch { }
		}
		await _mic.StopAsync();
		await _log.LogSystemStopAsync();
		_cts?.Dispose();
		_cts = null;
	}

	public async ValueTask DisposeAsync()
	{
		await StopAsync();
		await _screen.DisposeAsync();
		await _mic.DisposeAsync();
	}
}
