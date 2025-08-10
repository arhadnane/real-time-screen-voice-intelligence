using Microsoft.AspNetCore.SignalR;
using RealTimeIntelligence.Web.Hubs;
using System.Collections.Concurrent;

namespace RealTimeIntelligence.Web.Services;

public class ActivityLogService
{
    private readonly IHubContext<ActivityHub>? _hubContext;
    private readonly ConcurrentQueue<ActivityLogEntry> _activities = new();
    private const int MaxLogEntries = 200;

    public ActivityLogService(IHubContext<ActivityHub>? hubContext = null)
    {
        _hubContext = hubContext;
    }

    // Méthode utilisée par diverses pages / composants pour ajouter une entrée complète
    public async Task AddLogAsync(ActivityLogEntry entry)
    {
        Enqueue(entry);
        await BroadcastAsync(entry);
    }

    public async Task LogActivityAsync(string message, ActivityType type = ActivityType.Info, TimeSpan? duration = null)
    {
        var entry = new ActivityLogEntry
        {
            Timestamp = DateTime.Now,
            Message = message,
            Type = type,
            Duration = duration
        };
        Enqueue(entry);
        await BroadcastAsync(entry);
    }

    private void Enqueue(ActivityLogEntry entry)
    {
        _activities.Enqueue(entry);
        while (_activities.Count > MaxLogEntries)
        {
            _activities.TryDequeue(out _);
        }
    }

    private async Task BroadcastAsync(ActivityLogEntry entry)
    {
        if (_hubContext != null)
        {
            try { await _hubContext.Clients.All.SendAsync("ActivityLogged", entry); } catch { /* ignore */ }
        }
    }

    public Task<List<ActivityLogEntry>> GetLogsAsync() => Task.FromResult(_activities.Reverse().ToList());

    public Task ClearLogsAsync()
    {
        while (_activities.TryDequeue(out _)) { }
        return Task.CompletedTask;
    }

    public async Task LogSystemStartAsync()
    {
        await LogActivityAsync("Système d'intelligence en temps réel démarré", ActivityType.Système);
        await LogActivityAsync("Module de capture d'écran initialisé", ActivityType.Vision);
        await LogActivityAsync("Module de reconnaissance vocale prêt", ActivityType.Audio);
        await LogActivityAsync("Routeur IA connecté", ActivityType.IA);
    }

    public async Task LogSystemStopAsync()
    {
        await LogActivityAsync("Système d'intelligence arrêté", ActivityType.Système);
    }

    public Task LogOcrActivityAsync(string text)
        => LogActivityAsync($"Texte OCR détecté: {text.Substring(0, Math.Min(50, text.Length))}...", ActivityType.OCR);

    public Task LogVoiceActivityAsync(string transcript)
        => LogActivityAsync($"Parole détectée: {transcript}", ActivityType.Audio);

    public Task LogAiAnalysisAsync(string analysis)
        => LogActivityAsync($"Analyse IA: {analysis.Substring(0, Math.Min(60, analysis.Length))}...", ActivityType.IA);

    public Task LogErrorAsync(string error)
        => LogActivityAsync($"Erreur: {error}", ActivityType.Error);
}

public class ActivityLogEntry
{
    public DateTime Timestamp { get; set; }
    public string Message { get; set; } = string.Empty;
    public ActivityType Type { get; set; }
    public TimeSpan? Duration { get; set; }
    public string Icon => Type switch
    {
        ActivityType.Système => "fas fa-power-off text-primary",
        ActivityType.Vision => "fas fa-desktop text-warning",
        ActivityType.Audio => "fas fa-microphone text-info",
        ActivityType.IA => "fas fa-brain text-success",
        ActivityType.OCR => "fas fa-text-width text-secondary",
        ActivityType.Interface => "fas fa-window-maximize text-dark",
        ActivityType.Success => "fas fa-check-circle text-success",
        ActivityType.Warning => "fas fa-exclamation-triangle text-warning",
        ActivityType.Error => "fas fa-exclamation-circle text-danger",
        ActivityType.Info => "fas fa-info-circle text-info",
        _ => "fas fa-circle text-secondary"
    };
}

public enum ActivityType
{
    // Types attendus par Home.razor
    Système,
    Vision,
    Audio,
    IA,
    OCR,
    Interface,
    // Anciens / génériques
    Info,
    Success,
    Warning,
    Error
}
