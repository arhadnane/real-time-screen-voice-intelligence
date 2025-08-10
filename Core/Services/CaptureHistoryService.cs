using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Core.Models;
using Serilog;

namespace Core.Services
{
    public class CaptureHistoryService
    {
        private readonly string _historyFilePath;
        private readonly ILogger _logger;
        private CaptureHistory _history;

        public CaptureHistoryService(ILogger logger, string? customPath = null)
        {
            _logger = logger;
            _historyFilePath = customPath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                "RealTimeIntelligence", "capture_history.json");
            _history = new CaptureHistory();
            
            // Créer le dossier si nécessaire
            var directory = Path.GetDirectoryName(_historyFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public async Task LoadHistoryAsync()
        {
            try
            {
                if (File.Exists(_historyFilePath))
                {
                    var json = await File.ReadAllTextAsync(_historyFilePath);
                    _history = JsonSerializer.Deserialize<CaptureHistory>(json) ?? new CaptureHistory();
                    _logger.Information($"Historique chargé: {_history.Entries.Count} entrées");
                }
                else
                {
                    _logger.Information("Aucun fichier d'historique trouvé, création d'un nouvel historique");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Erreur lors du chargement de l'historique");
                _history = new CaptureHistory();
            }
        }

        public async Task SaveHistoryAsync()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                
                var json = JsonSerializer.Serialize(_history, options);
                await File.WriteAllTextAsync(_historyFilePath, json);
                _logger.Debug($"Historique sauvegardé: {_history.Entries.Count} entrées");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Erreur lors de la sauvegarde de l'historique");
            }
        }

        public async Task AddCaptureAsync(string? ocrText, string? aiAnalysis, string? windowTitle = null, 
            int width = 0, int height = 0, string? imagePath = null)
        {
            try
            {
                var entry = new CaptureEntry
                {
                    Timestamp = DateTime.Now,
                    OcrText = ocrText,
                    AiAnalysis = aiAnalysis,
                    WindowTitle = windowTitle,
                    Width = width,
                    Height = height,
                    ImagePath = imagePath
                };

                _history.AddEntry(entry);
                await SaveHistoryAsync();
                
                _logger.Information($"Nouvelle capture ajoutée à l'historique: {ocrText?.Length ?? 0} caractères OCR, " +
                                  $"Analyse: {!string.IsNullOrEmpty(aiAnalysis)}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Erreur lors de l'ajout d'une capture à l'historique");
            }
        }

        public List<CaptureEntry> GetHistory()
        {
            return _history.Entries;
        }

        public async Task ClearHistoryAsync()
        {
            _history.Clear();
            await SaveHistoryAsync();
            _logger.Information("Historique effacé");
        }

        public List<CaptureEntry> SearchByText(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return _history.Entries;

            return _history.Entries.Where(e => 
                (!string.IsNullOrEmpty(e.OcrText) && e.OcrText.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(e.AiAnalysis) && e.AiAnalysis.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(e.WindowTitle) && e.WindowTitle.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            ).ToList();
        }

        public List<CaptureEntry> GetEntriesByDateRange(DateTime from, DateTime to)
        {
            return _history.Entries.Where(e => e.Timestamp >= from && e.Timestamp <= to).ToList();
        }
    }
}
