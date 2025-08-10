using Vision;

namespace RealTimeIntelligence.Web.Services;

public interface IScreenCaptureService : IAsyncDisposable
{
	Task<ScreenCaptureResult> CaptureAsync(CancellationToken ct = default);
	void ConfigureMask(MaskStyle style, int blurDownscaleFactor = 8);
}

public record ScreenCaptureResult(byte[]? ImageBytes, string? Text, DateTime Timestamp, bool Success, string? Error = null);

public interface IOcrService
{
	Task<string> ExtractTextAsync(byte[] imageBytes, CancellationToken ct = default);
}

public interface IMicrophoneService : IAsyncDisposable
{
	Task StartAsync(CancellationToken ct = default);
	Task StopAsync();
	event EventHandler<string>? Transcription;
}

public interface IAIAnalysisService
{
	Task<string> AnalyzeAsync(string context, CancellationToken ct = default);
}

public interface IRealTimeCoordinator
{
	Task StartAsync(CancellationToken ct = default);
	Task StopAsync();
	bool IsRunning { get; }
}
