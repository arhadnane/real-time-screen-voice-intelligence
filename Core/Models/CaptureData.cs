using System;
using System.Collections.Generic;

namespace Core.Models
{
    public class CaptureEntry
    {
        public DateTime Timestamp { get; set; }
        public string? OcrText { get; set; }
        public string? AiAnalysis { get; set; }
        public string? ImagePath { get; set; }
        public string? WindowTitle { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class CaptureHistory
    {
        public List<CaptureEntry> Entries { get; set; } = new();
        public int MaxEntries { get; set; } = 100; // Limiter à 100 entrées pour éviter un fichier trop gros
        
        public void AddEntry(CaptureEntry entry)
        {
            // Ajouter en tête de liste (nouveaux en haut)
            Entries.Insert(0, entry);
            
            // Limiter le nombre d'entrées
            if (Entries.Count > MaxEntries)
            {
                Entries.RemoveRange(MaxEntries, Entries.Count - MaxEntries);
            }
        }
        
        public void Clear()
        {
            Entries.Clear();
        }
    }
}
