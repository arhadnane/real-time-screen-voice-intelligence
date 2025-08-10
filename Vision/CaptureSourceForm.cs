using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

namespace Vision
{
    public partial class CaptureSourceForm : Form
    {
        public CaptureMode SelectedMode { get; private set; }
        public IntPtr SelectedWindowHandle { get; private set; }
        public CaptureRegion SelectedRegion { get; private set; }
        public int SelectedScreenIndex { get; private set; }

        private readonly CaptureSourceSelector _selector;
        private List<WindowInfo> _availableWindows;

        public CaptureSourceForm()
        {
            _selector = new CaptureSourceSelector();
            _availableWindows = new List<WindowInfo>();
            InitializeComponent();
            LoadAvailableSources();
        }

        private void InitializeComponent()
        {
            this.Text = "🎯 Configuration Source de Capture";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                Padding = new Padding(10)
            };
            this.Controls.Add(mainPanel);

            // Title
            var titleLabel = new Label
            {
                Text = "Choisissez votre source de capture d'écran:",
                Font = new Font("Arial", 12, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            mainPanel.Controls.Add(titleLabel);

            // Mode selection
            var modeGroup = new GroupBox
            {
                Text = "Mode de capture",
                Dock = DockStyle.Fill,
                Height = 120
            };
            var modePanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown };
            
            var fullScreenRadio = new RadioButton { Text = "🖥️ Écran principal complet", Tag = CaptureMode.FullScreen, Checked = true };
            var allScreensRadio = new RadioButton { Text = "🖥️ Tous les écrans", Tag = CaptureMode.AllScreens };
            var windowRadio = new RadioButton { Text = "🪟 Fenêtre spécifique", Tag = CaptureMode.SpecificWindow };
            var regionRadio = new RadioButton { Text = "📐 Région personnalisée", Tag = CaptureMode.CustomRegion };

            modePanel.Controls.AddRange(new Control[] { fullScreenRadio, allScreensRadio, windowRadio, regionRadio });
            modeGroup.Controls.Add(modePanel);
            mainPanel.Controls.Add(modeGroup);

            // Window selection
            var windowGroup = new GroupBox
            {
                Text = "Sélection de fenêtre",
                Dock = DockStyle.Fill,
                Height = 150,
                Enabled = false
            };
            var windowList = new ListBox { Dock = DockStyle.Fill, Tag = "WindowList" };
            windowGroup.Controls.Add(windowList);
            mainPanel.Controls.Add(windowGroup);

            // Region selection
            var regionGroup = new GroupBox
            {
                Text = "Région personnalisée",
                Dock = DockStyle.Fill,
                Height = 80,
                Enabled = false
            };
            var regionPanel = new FlowLayoutPanel { Dock = DockStyle.Fill };
            var regionButton = new Button { Text = "🎯 Sélectionner une région", Tag = "RegionSelect" };
            var regionLabel = new Label { Text = "Aucune région sélectionnée", Tag = "RegionLabel" };
            regionPanel.Controls.AddRange(new Control[] { regionButton, regionLabel });
            regionGroup.Controls.Add(regionPanel);
            mainPanel.Controls.Add(regionGroup);

            // Preview area (placeholder)
            var previewGroup = new GroupBox
            {
                Text = "Aperçu",
                Dock = DockStyle.Fill,
                Height = 100
            };
            var previewLabel = new Label
            {
                Text = "L'aperçu sera affiché ici lors de la capture",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.LightGray
            };
            previewGroup.Controls.Add(previewLabel);
            mainPanel.Controls.Add(previewGroup);

            // Buttons
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 40
            };
            var okButton = new Button { Text = "✅ Appliquer", DialogResult = DialogResult.OK };
            var cancelButton = new Button { Text = "❌ Annuler", DialogResult = DialogResult.Cancel };
            var refreshButton = new Button { Text = "🔄 Actualiser", Tag = "Refresh" };
            buttonPanel.Controls.AddRange(new Control[] { okButton, cancelButton, refreshButton });
            mainPanel.Controls.Add(buttonPanel);

            // Event handlers
            foreach (var radio in modePanel.Controls.OfType<RadioButton>())
            {
                radio.CheckedChanged += OnModeChanged;
            }

            windowRadio.CheckedChanged += (s, e) => windowGroup.Enabled = windowRadio.Checked;
            regionRadio.CheckedChanged += (s, e) => regionGroup.Enabled = regionRadio.Checked;

            windowList.SelectedIndexChanged += OnWindowSelected;
            regionButton.Click += OnRegionSelectClick;
            refreshButton.Click += OnRefreshClick;
        }

        private void LoadAvailableSources()
        {
            try
            {
                _availableWindows = _selector.GetAvailableWindows();
                var windowList = this.Controls.Find("WindowList", true).FirstOrDefault() as ListBox;
                
                if (windowList != null)
                {
                    windowList.Items.Clear();
                    foreach (var window in _availableWindows)
                    {
                        var displayText = $"🪟 {window.Title} ({window.ProcessName}) - {window.Bounds.Width}x{window.Bounds.Height}";
                        windowList.Items.Add(displayText);
                    }
                }

                // Update screens info
                var screens = _selector.GetAvailableScreens();
                var allScreensRadio = this.Controls.Find("", true)
                    .OfType<RadioButton>()
                    .FirstOrDefault(r => r.Text.Contains("Tous les écrans"));
                
                if (allScreensRadio != null)
                {
                    allScreensRadio.Text = $"🖥️ Tous les écrans ({screens.Count})";
                    allScreensRadio.Enabled = screens.Count > 1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des sources: {ex.Message}", "Erreur", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OnModeChanged(object? sender, EventArgs e)
        {
            if (sender is RadioButton radio && radio.Checked)
            {
                SelectedMode = (CaptureMode)radio.Tag;
            }
        }

        private void OnWindowSelected(object? sender, EventArgs e)
        {
            if (sender is ListBox list && list.SelectedIndex >= 0 && list.SelectedIndex < _availableWindows.Count)
            {
                SelectedWindowHandle = _availableWindows[list.SelectedIndex].Handle;
            }
        }

        private void OnRegionSelectClick(object? sender, EventArgs e)
        {
            this.Hide();
            var region = _selector.ShowRegionSelector();
            this.Show();
            
            if (region.HasValue)
            {
                SelectedRegion = region.Value;
                var label = this.Controls.Find("RegionLabel", true).FirstOrDefault() as Label;
                if (label != null)
                {
                    label.Text = $"Région: {region.Value.X},{region.Value.Y} - {region.Value.Width}x{region.Value.Height}";
                }
            }
        }

        private void OnRefreshClick(object? sender, EventArgs e)
        {
            LoadAvailableSources();
        }
    }
}
