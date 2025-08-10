using AI;
using Serilog;

namespace RealTimeIntelligence.Web.Services;

public class AIAnalysisService : IAIAnalysisService, IDisposable
{
	private readonly AIRouter _router;
	private readonly Serilog.ILogger _logger;

	public AIAnalysisService(string ollamaEndpoint = "http://localhost:11434", string ollamaModel = "phi3:mini", string hfToken = "", Serilog.ILogger? logger = null)
	{
		_logger = logger ?? Log.ForContext<AIAnalysisService>();
		var ollama = new OllamaProvider(ollamaEndpoint, ollamaModel);
		var hf = new HuggingFaceProvider(hfToken);
		_router = new AIRouter(ollama, hf);
	}

	public async Task<string> AnalyzeAsync(string context, CancellationToken ct = default)
	{
		try
		{
			return await _router.GetResponse(context);
		}
		catch (Exception ex)
		{
			_logger.Error(ex, "AI analysis failed");
			return $"[AI Error] {ex.Message}";
		}
	}

	public void Dispose()
	{
		_router.Dispose();
	}
}
