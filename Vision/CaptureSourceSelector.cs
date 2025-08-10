using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Vision
{
    public enum CaptureMode
    {
        FullScreen,
        AllScreens,
        SpecificWindow,
        CustomRegion
    }

    public struct CaptureRegion
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Rectangle ToRectangle() => new Rectangle(X, Y, Width, Height);
    }

    public class WindowInfo
    {
        public IntPtr Handle { get; set; }
        public string Title { get; set; } = "";
        public string ProcessName { get; set; } = "";
        public Rectangle Bounds { get; set; }
        public bool IsVisible { get; set; }
    }

    public class CaptureSourceSelector
    {
        #region Windows API
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        #endregion

        public List<WindowInfo> GetAvailableWindows()
        {
            var windows = new List<WindowInfo>();
            
            EnumWindows((hWnd, lParam) =>
            {
                if (IsWindowVisible(hWnd))
                {
                    var length = GetWindowTextLength(hWnd);
                    if (length > 0)
                    {
                        var sb = new StringBuilder(length + 1);
                        GetWindowText(hWnd, sb, sb.Capacity);
                        var title = sb.ToString();

                        if (!string.IsNullOrWhiteSpace(title) && title.Length > 3)
                        {
                            GetWindowRect(hWnd, out RECT rect);
                            GetWindowThreadProcessId(hWnd, out uint processId);
                            
                            var bounds = new Rectangle(rect.Left, rect.Top, 
                                rect.Right - rect.Left, rect.Bottom - rect.Top);

                            // Filter out tiny windows and system windows
                            if (bounds.Width > 100 && bounds.Height > 100)
                            {
                                try
                                {
                                    var process = Process.GetProcessById((int)processId);
                                    windows.Add(new WindowInfo
                                    {
                                        Handle = hWnd,
                                        Title = title,
                                        ProcessName = process.ProcessName,
                                        Bounds = bounds,
                                        IsVisible = true
                                    });
                                }
                                catch
                                {
                                    // Ignore processes we can't access
                                }
                            }
                        }
                    }
                }
                return true;
            }, IntPtr.Zero);

            return windows;
        }

        public List<Screen> GetAvailableScreens()
        {
            return Screen.AllScreens.ToList();
        }

        public CaptureRegion? ShowRegionSelector()
        {
            using (var selector = new RegionSelectorForm())
            {
                if (selector.ShowDialog() == DialogResult.OK)
                {
                    return selector.SelectedRegion;
                }
            }
            return null;
        }
    }

    // Simple form for region selection
    public partial class RegionSelectorForm : Form
    {
        public CaptureRegion SelectedRegion { get; private set; }
        
        private bool _isSelecting = false;
        private Point _startPoint;
        private Rectangle _selectionRect;

        public RegionSelectorForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;
            this.Cursor = Cursors.Cross;
            this.BackColor = Color.Black;
            this.Opacity = 0.3;
            this.ShowInTaskbar = false;
            
            this.Text = "Sélectionnez une région - Échap pour annuler";
            
            this.MouseDown += OnMouseDown;
            this.MouseMove += OnMouseMove;
            this.MouseUp += OnMouseUp;
            this.KeyDown += OnKeyDown;
            this.Paint += OnPaint;
            
            this.KeyPreview = true;
        }

        private void OnMouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isSelecting = true;
                _startPoint = e.Location;
                _selectionRect = new Rectangle(e.Location, Size.Empty);
            }
        }

        private void OnMouseMove(object? sender, MouseEventArgs e)
        {
            if (_isSelecting)
            {
                var x = Math.Min(_startPoint.X, e.X);
                var y = Math.Min(_startPoint.Y, e.Y);
                var width = Math.Abs(e.X - _startPoint.X);
                var height = Math.Abs(e.Y - _startPoint.Y);
                
                _selectionRect = new Rectangle(x, y, width, height);
                this.Invalidate();
            }
        }

        private void OnMouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && _isSelecting)
            {
                _isSelecting = false;
                
                if (_selectionRect.Width > 10 && _selectionRect.Height > 10)
                {
                    SelectedRegion = new CaptureRegion
                    {
                        X = _selectionRect.X,
                        Y = _selectionRect.Y,
                        Width = _selectionRect.Width,
                        Height = _selectionRect.Height
                    };
                    
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void OnPaint(object? sender, PaintEventArgs e)
        {
            if (_isSelecting && _selectionRect.Width > 0 && _selectionRect.Height > 0)
            {
                using (var brush = new SolidBrush(Color.FromArgb(128, Color.Red)))
                using (var pen = new Pen(Color.Red, 2))
                {
                    e.Graphics.FillRectangle(brush, _selectionRect);
                    e.Graphics.DrawRectangle(pen, _selectionRect);
                }
                
                // Show dimensions
                var text = $"{_selectionRect.Width} x {_selectionRect.Height}";
                using (var font = new Font("Arial", 12))
                using (var textBrush = new SolidBrush(Color.White))
                {
                    var textSize = e.Graphics.MeasureString(text, font);
                    var textPoint = new PointF(
                        _selectionRect.X + (_selectionRect.Width - textSize.Width) / 2,
                        _selectionRect.Y + (_selectionRect.Height - textSize.Height) / 2
                    );
                    e.Graphics.DrawString(text, font, textBrush, textPoint);
                }
            }
        }
    }
}
