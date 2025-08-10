using Audio;
using Serilog;

namespace RealTimeIntelligence.Web.Services;

public class MicrophoneService : IMicrophoneService
{
	private readonly Serilog.ILogger _logger;
	private readonly string _modelPath;
	private VoskSpeechRecognizer? _recognizer;
	public event EventHandler<string>? Transcription;

	public MicrophoneService(string modelPath = "./vosk-models/vosk-model-small-fr-0.22", Serilog.ILogger? logger = null)
	{
		_modelPath = modelPath;
		_logger = logger ?? Log.ForContext<MicrophoneService>();
	}

	public Task StartAsync(CancellationToken ct = default)
	{
		if (_recognizer != null) return Task.CompletedTask;
		try
		{
			_recognizer = new VoskSpeechRecognizer(_modelPath, 16000, _logger);
			_recognizer.TranscriptionReceived += (_, text) => Transcription?.Invoke(this, text);
			_logger.Information("Microphone service started");
		}
		catch (Exception ex)
		{
			_logger.Error(ex, "Failed to start microphone service");
		}
		return Task.CompletedTask;
	}

	public Task StopAsync()
	{
		_recognizer?.Dispose();
		_recognizer = null;
		return Task.CompletedTask;
	}

	public ValueTask DisposeAsync()
	{
		_recognizer?.Dispose();
		return ValueTask.CompletedTask;
	}
}
