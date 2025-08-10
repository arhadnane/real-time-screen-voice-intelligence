using Vision;
using OpenCvSharp;
using Serilog;

namespace RealTimeIntelligence.Web.Services;

public class ScreenCaptureService : IScreenCaptureService
{
	private readonly OpenCvScreenAnalyzer _analyzer;
	private readonly Serilog.ILogger _logger;
	private readonly OcrEngine? _ocr;
	private readonly IConfiguration _config;

	public ScreenCaptureService(IConfiguration config, Serilog.ILogger? logger = null, OcrEngine? ocr = null)
	{
		_config = config;
		_logger = logger ?? Log.ForContext<ScreenCaptureService>();
		_analyzer = new OpenCvScreenAnalyzer();
		_ocr = ocr; // optional, may be null; coordinator can combine with IOcrService

		// Apply configuration (mask style etc.)
		try
		{
			var section = _config.GetSection("Vision");
			var styleStr = section["MaskStyle"] ?? "Black";
			var blurFactor = int.TryParse(section["BlurDownscaleFactor"], out var bf) ? bf : 8;
			if (!Enum.TryParse<MaskStyle>(styleStr, true, out var style)) style = MaskStyle.Black;
			_analyzer.ConfigureMask(style, blurFactor);
			_logger.Information("ScreenCaptureService configured: MaskStyle={MaskStyle} BlurFactor={BlurFactor}", style, blurFactor);
		}
		catch (Exception ex)
		{
			_logger.Warning(ex, "Failed to apply Vision configuration");
		}
	}

	public async Task<ScreenCaptureResult> CaptureAsync(CancellationToken ct = default)
	{
		try
		{
			using var mat = _analyzer.CaptureScreen();
			if (mat.Empty())
				return new ScreenCaptureResult(null, null, DateTime.UtcNow, false, "Empty frame");

			// Encode to PNG
			var data = mat.ImEncode(".png");
			string? text = null;
			if (_ocr != null)
			{
				text = await Task.Run(() => _ocr.RunTesseract(mat), ct);
			}
			return new ScreenCaptureResult(data, text, DateTime.UtcNow, true);
		}
		catch (Exception ex)
		{
			_logger.Error(ex, "Screen capture failed");
			return new ScreenCaptureResult(null, null, DateTime.UtcNow, false, ex.Message);
		}
	}

	public ValueTask DisposeAsync()
	{
		_analyzer.Dispose();
		return ValueTask.CompletedTask;
	}

	public void ConfigureMask(MaskStyle style, int blurDownscaleFactor = 8)
	{
		_analyzer.ConfigureMask(style, blurDownscaleFactor);
		_logger.Information("Mask updated: {Style} (factor {Factor})", style, blurDownscaleFactor);
	}
}
