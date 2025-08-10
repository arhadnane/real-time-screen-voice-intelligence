using Vision;
using OpenCvSharp;
using Serilog;

namespace RealTimeIntelligence.Web.Services;

public class OcrService : IOcrService
{
	private readonly OcrEngine _engine;
	private readonly Serilog.ILogger _logger;

	public OcrService(string tessDataPath = "./tessdata", string languages = "eng+fra", Serilog.ILogger? logger = null)
	{
		_logger = logger ?? Log.ForContext<OcrService>();
		if (!Directory.Exists(tessDataPath))
		{
			_logger.Warning("tessdata path {Path} not found. OCR will produce empty results.", tessDataPath);
		}
		_engine = new OcrEngine(tessDataPath, languages, _logger);
	}

	public async Task<string> ExtractTextAsync(byte[] imageBytes, CancellationToken ct = default)
	{
		try
		{
			using var mat = Cv2.ImDecode(imageBytes, ImreadModes.Color);
			return await Task.Run(() => _engine.RunTesseract(mat), ct);
		}
		catch (Exception ex)
		{
			_logger.Error(ex, "OCR extraction failed");
			return string.Empty;
		}
	}
}
