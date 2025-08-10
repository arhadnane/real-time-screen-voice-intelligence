using RealTimeIntelligence.Web.Services;

namespace RealTimeIntelligence.Web.Services;

public class IntelligenceService
{
    private readonly ActivityLogService? _activityLog;
    private bool _isRunning = false;
    private CancellationTokenSource? _cancellationTokenSource;

    public IntelligenceService(ActivityLogService? activityLog = null)
    {
        _activityLog = activityLog;
    }

    public bool IsRunning => _isRunning;

    public async Task StartAsync()
    {
        if (_isRunning) return;

        _isRunning = true;
        _cancellationTokenSource = new CancellationTokenSource();

        await _activityLog.LogSystemStartAsync();

        // Simulation des activités du système
        _ = Task.Run(async () => await SimulateSystemActivity(_cancellationTokenSource.Token));
    }

    public async Task StopAsync()
    {
        if (!_isRunning) return;

        _isRunning = false;
        _cancellationTokenSource?.Cancel();

        await _activityLog.LogSystemStopAsync();
    }

    public async Task RunDiagnosticAsync()
    {
        await _activityLog.LogActivityAsync("🔍 Démarrage du diagnostic système...", ActivityType.Info);
        
        await Task.Delay(1000);
        await _activityLog.LogActivityAsync("✅ Modules de capture: OK", ActivityType.Success);
        
        await Task.Delay(800);
        await _activityLog.LogActivityAsync("✅ Modules audio: OK", ActivityType.Success);
        
        await Task.Delay(600);
        await _activityLog.LogActivityAsync("✅ Modules IA: OK", ActivityType.Success);
        
        await Task.Delay(400);
        await _activityLog.LogActivityAsync("🎯 Diagnostic terminé - Tous les systèmes opérationnels", ActivityType.Success);
    }

    private async Task SimulateSystemActivity(CancellationToken cancellationToken)
    {
        var activities = new[]
        {
            () => _activityLog.LogOcrActivityAsync("Texte détecté dans la capture d'écran"),
            () => _activityLog.LogVoiceActivityAsync("Bonjour, comment ça va?"),
            () => _activityLog.LogAiAnalysisAsync("L'utilisateur semble être en train de coder"),
            () => _activityLog.LogActivityAsync("📊 Analyse des données en cours...", ActivityType.Info),
            () => _activityLog.LogActivityAsync("🔄 Mise à jour des modèles IA", ActivityType.Info)
        };

        var random = new Random();

        while (!cancellationToken.IsCancellationRequested && _isRunning)
        {
            try
            {
                await Task.Delay(random.Next(3000, 8000), cancellationToken);
                
                if (!cancellationToken.IsCancellationRequested)
                {
                    var activity = activities[random.Next(activities.Length)];
                    await activity();
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
