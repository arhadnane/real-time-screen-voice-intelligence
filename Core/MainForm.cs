using System.Drawing;
using System.Windows.Forms;
using Vision;
using AI;
using Serilog;
using System.Runtime.InteropServices;
using System.Text;
using Core.Services;
using Core.Models;

namespace Core;

public class MainForm : Form
{
    private readonly OpenCvScreenAnalyzer _screenAnalyzer = new();
    private readonly OcrEngine? _ocrEngine;
    private readonly AIRouter _aiRouter;
    private readonly CaptureHistoryService _historyService;
    private readonly Serilog.ILogger _logger = Log.ForContext<MainForm>();

    private readonly PictureBox _picture = new() { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.Black };
    private readonly TextBox _ocrText = new() { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical };
    private readonly TextBox _aiText = new() { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical };
    private readonly ListBox _historyList = new() { Dock = DockStyle.Fill };
    private readonly TextBox _historyDetails = new() { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical, ReadOnly = true };
    private readonly Button _toggleBtn = new() { Text = "Démarrer", Dock = DockStyle.Top, Height = 40 };
    private readonly Button _clearHistoryBtn = new() { Text = "Effacer Historique", Dock = DockStyle.Bottom, Height = 30 };
    private readonly Label _status = new() { Text = "Inactif", Dock = DockStyle.Top, Height = 20 };
    private readonly System.Windows.Forms.Timer _captureTimer = new() { Interval = 3000 }; // 3s
    private bool _running;
    private DateTime _lastAiAnalysis = DateTime.MinValue;

    public MainForm(Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        Text = "Real-Time Intelligence Desktop";
        Width = 1400; Height = 800; // Agrandir pour l'historique

        // Initialiser le service d'historique
        _historyService = new CaptureHistoryService(_logger);
        _ = Task.Run(_historyService.LoadHistoryAsync);

        var visionConfig = configuration.GetSection("Vision");
        var tessPath = visionConfig["TessDataPath"] ?? "tessdata";
        var ocrLangs = visionConfig["OcrLanguages"] ?? "eng";
        if (Directory.Exists(tessPath))
        {
            _ocrEngine = new OcrEngine(tessPath, ocrLangs, _logger);
        }

        // Capture interval & mask config
        if (int.TryParse(visionConfig["CaptureIntervalMs"], out var interval) && interval > 200)
            _captureTimer.Interval = interval;
        Enum.TryParse<MaskStyle>(visionConfig["MaskStyle"], true, out var maskStyle);
        if (int.TryParse(visionConfig["BlurDownscaleFactor"], out var blurFactor))
            _screenAnalyzer.ConfigureMask(maskStyle, blurFactor);
        else
            _screenAnalyzer.ConfigureMask(maskStyle);

        // Excluded window titles
        var excludedTitlesRaw = visionConfig["ExcludedWindowTitles"] ?? string.Empty;
        var titleParts = excludedTitlesRaw.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (titleParts.Length > 0)
        {
            foreach (var handle in FindWindowsByTitleParts(titleParts))
                _screenAnalyzer.AddExcludedWindow(handle);
        }

        var aiConfig = configuration.GetSection("AI");
        var ollamaEndpoint = aiConfig["OllamaEndpoint"] ?? "http://localhost:11434";
        var ollamaModel = aiConfig["OllamaModel"] ?? "phi3:mini";
        var ollama = new OllamaProvider(ollamaEndpoint);
        var hf = new HuggingFaceProvider(aiConfig["HF_Token"] ?? "");
        _aiRouter = new AIRouter(ollama, hf);

        _toggleBtn.Click += (_, _) => Toggle();
        _clearHistoryBtn.Click += async (_, _) => await ClearHistoryAsync();
        _historyList.SelectedIndexChanged += (_, _) => ShowHistoryDetails();
        _captureTimer.Tick += async (_, _) => await CaptureCycleAsync();

        SetupLayout();

        _ocrText.PlaceholderText = "Texte OCR actuel";
        _aiText.PlaceholderText = "Analyse IA actuelle";
        _historyDetails.PlaceholderText = "Sélectionnez un élément dans l'historique pour voir les détails";
    }

    private void SetupLayout()
    {
        var topPanel = new Panel { Dock = DockStyle.Top, Height = 60 };
        topPanel.Controls.Add(_toggleBtn);
        topPanel.Controls.Add(_status);

        // Panneau principal avec capture d'écran et données actuelles
        var splitMain = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 900 };
        
        // Partie gauche: capture d'écran et données actuelles
        var splitLeft = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 450 };
        splitLeft.Panel1.Controls.Add(_picture);
        
        var splitCurrent = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 150 };
        splitCurrent.Panel1.Controls.Add(_ocrText);
        splitCurrent.Panel2.Controls.Add(_aiText);
        splitLeft.Panel2.Controls.Add(splitCurrent);
        
        splitMain.Panel1.Controls.Add(splitLeft);

        // Partie droite: historique
        var historyPanel = new Panel { Dock = DockStyle.Fill };
        var historyLabel = new Label { Text = "Historique des Captures (nouveaux en haut)", Dock = DockStyle.Top, Height = 25, Font = new Font(Font, FontStyle.Bold) };
        
        var splitHistory = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 200 };
        splitHistory.Panel1.Controls.Add(_historyList);
        splitHistory.Panel2.Controls.Add(_historyDetails);
        
        historyPanel.Controls.Add(splitHistory);
        historyPanel.Controls.Add(_clearHistoryBtn);
        historyPanel.Controls.Add(historyLabel);
        
        splitMain.Panel2.Controls.Add(historyPanel);

        Controls.Add(splitMain);
        Controls.Add(topPanel);
    }

    private async Task ClearHistoryAsync()
    {
        var result = MessageBox.Show("Êtes-vous sûr de vouloir effacer tout l'historique ?", 
            "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        
        if (result == DialogResult.Yes)
        {
            await _historyService.ClearHistoryAsync();
            RefreshHistoryList();
            _historyDetails.Clear();
        }
    }

    private void ShowHistoryDetails()
    {
        if (_historyList.SelectedItem is CaptureEntry entry)
        {
            var details = new StringBuilder();
            details.AppendLine($"📅 Date: {entry.Timestamp:dd/MM/yyyy HH:mm:ss}");
            details.AppendLine($"🪟 Fenêtre: {entry.WindowTitle ?? "Inconnue"}");
            details.AppendLine($"📏 Dimensions: {entry.Width}x{entry.Height}");
            details.AppendLine();
            details.AppendLine("📝 TEXTE OCR:");
            details.AppendLine(new string('=', 50));
            details.AppendLine(entry.OcrText ?? "Aucun texte détecté");
            details.AppendLine();
            details.AppendLine("🤖 ANALYSE IA:");
            details.AppendLine(new string('=', 50));
            details.AppendLine(entry.AiAnalysis ?? "Aucune analyse disponible");
            
            _historyDetails.Text = details.ToString();
        }
    }

    private void RefreshHistoryList()
    {
        _historyList.Items.Clear();
        var history = _historyService.GetHistory();
        
        foreach (var entry in history)
        {
            _historyList.Items.Add(entry);
        }
        
        // Configurer l'affichage personnalisé
        _historyList.DrawMode = DrawMode.OwnerDrawFixed;
        _historyList.DrawItem -= HistoryList_DrawItem; // Éviter les doublons
        _historyList.DrawItem += HistoryList_DrawItem;
    }

    private void HistoryList_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0 || e.Index >= _historyList.Items.Count) return;
        
        var entry = (CaptureEntry)_historyList.Items[e.Index];
        var preview = entry.OcrText?.Length > 50 
            ? entry.OcrText[..50] + "..." 
            : entry.OcrText ?? "Aucun texte";
        
        var displayText = $"{entry.Timestamp:HH:mm:ss} - {preview}";
        
        e.DrawBackground();
        using var brush = new SolidBrush(e.ForeColor);
        e.Graphics.DrawString(displayText, e.Font!, brush, e.Bounds);
        e.DrawFocusRectangle();
    }

    private string GetDisplayText(CaptureEntry entry)
    {
        var preview = entry.OcrText?.Length > 50 
            ? entry.OcrText[..50] + "..." 
            : entry.OcrText ?? "Aucun texte";
        return $"{entry.Timestamp:HH:mm:ss} - {preview}";
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        // Exclure cette fenêtre de la capture pour éviter l'effet de mise en abyme
        _screenAnalyzer.AddExcludedWindow(this.Handle);
        
        // Charger et afficher l'historique
        RefreshHistoryList();
    }

    #region Window enumeration
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    [DllImport("user32.dll")] private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
    [DllImport("user32.dll", SetLastError = true)] private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
    [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr hWnd);

    private IEnumerable<IntPtr> FindWindowsByTitleParts(IEnumerable<string> parts)
    {
        var list = new List<IntPtr>();
        var lowered = parts.Select(p => p.ToLowerInvariant()).ToArray();
        EnumWindows((hWnd, l) =>
        {
            if (!IsWindowVisible(hWnd)) return true;
            var sb = new StringBuilder(256);
            if (GetWindowText(hWnd, sb, sb.Capacity) > 0)
            {
                var titleLower = sb.ToString().ToLowerInvariant();
                if (lowered.Any(p => titleLower.Contains(p)))
                    list.Add(hWnd);
            }
            return true;
        }, IntPtr.Zero);
        return list;
    }
    #endregion

    private void Toggle()
    {
        if (_running)
        {
            _running = false;
            _captureTimer.Stop();
            _toggleBtn.Text = "Démarrer";
            _status.Text = "Inactif";
        }
        else
        {
            _running = true;
            _captureTimer.Start();
            _toggleBtn.Text = "Arrêter";
            _status.Text = "Capture...";
            _ = CaptureCycleAsync();
        }
    }

    private async Task CaptureCycleAsync()
    {
        if (!_running) return;
        try
        {
            using var mat = _screenAnalyzer.CaptureScreen();
            if (!mat.Empty())
            {
                var bytes = mat.ImEncode(".png");
                using var ms = new MemoryStream(bytes);
                var bmp = new Bitmap(ms);
                var old = _picture.Image;
                _picture.Image = bmp;
                old?.Dispose();

                string ocrText = "";
                if (_ocrEngine != null)
                {
                    ocrText = _ocrEngine.RunTesseract(mat);
                    _ocrText.Text = ocrText;
                }

                string aiResponse = "";
                if (DateTime.UtcNow - _lastAiAnalysis > TimeSpan.FromSeconds(6) && !string.IsNullOrWhiteSpace(ocrText))
                {
                    _aiText.Text = "Analyse IA en cours...";
                    aiResponse = await _aiRouter.GetResponse("Analyse ce texte d'écran:\n" + ocrText[..Math.Min(400, ocrText.Length)]);
                    _aiText.Text = aiResponse;
                    _lastAiAnalysis = DateTime.UtcNow;

                    // Sauvegarder dans l'historique seulement quand on a une nouvelle analyse IA
                    await _historyService.AddCaptureAsync(
                        ocrText: ocrText,
                        aiAnalysis: aiResponse,
                        windowTitle: this.Text,
                        width: mat.Width,
                        height: mat.Height
                    );

                    // Rafraîchir la liste d'historique
                    RefreshHistoryList();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erreur cycle capture");
            _status.Text = "Erreur: " + ex.Message;
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _captureTimer.Stop();
        base.OnFormClosing(e);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _captureTimer.Dispose();
            _screenAnalyzer.Dispose();
            _ocrEngine?.Dispose();
            _picture.Image?.Dispose();
        }
        base.Dispose(disposing);
    }
}
