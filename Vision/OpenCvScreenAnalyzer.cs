using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using Rectangle = OpenCvSharp.Rect;

namespace Vision
{
    public class OpenCvScreenAnalyzer : IDisposable
    {
        private Mat? _previousFrame;
        private bool _disposed = false;
        private readonly double _changeThreshold = 0.05; // 5% change threshold
    private readonly HashSet<IntPtr> _excludedWindows = new();
    public MaskStyle CurrentMaskStyle { get; private set; } = MaskStyle.Black;
    public int BlurDownscaleFactor { get; private set; } = 8;
        
        // Capture configuration
        public CaptureMode CaptureMode { get; set; } = CaptureMode.FullScreen;
        public IntPtr TargetWindowHandle { get; set; } = IntPtr.Zero;
        public CaptureRegion CustomRegion { get; set; }
        public int TargetScreenIndex { get; set; } = 0;

        #region Windows API for window capture
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        
        [DllImport("user32.dll")]
        private static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        #endregion

        public OpenCvScreenAnalyzer()
        {
            // Enable OpenCV optimizations
            Cv2.SetUseOptimized(true);
        }

        // Cross-platform screen capture with multiple modes
        public Mat CaptureScreen()
        {
            try
            {
                return CaptureMode switch
                {
                    CaptureMode.FullScreen => CaptureFullScreen(),
                    CaptureMode.AllScreens => CaptureAllScreens(),
                    CaptureMode.SpecificWindow => CaptureSpecificWindow(),
                    CaptureMode.CustomRegion => CaptureCustomRegion(),
                    _ => CaptureFullScreen()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to capture screen: {ex.Message}");
                throw;
            }
        }

        public void AddExcludedWindow(IntPtr handle)
        {
            if (handle != IntPtr.Zero)
            {
                _excludedWindows.Add(handle);
            }
        }

        public void RemoveExcludedWindow(IntPtr handle)
        {
            if (handle != IntPtr.Zero)
            {
                _excludedWindows.Remove(handle);
            }
        }

        public void ConfigureMask(MaskStyle style, int blurDownscaleFactor = 8)
        {
            CurrentMaskStyle = style;
            BlurDownscaleFactor = Math.Clamp(blurDownscaleFactor, 2, 32);
        }

        private void MaskExcludedWindows(Graphics g)
        {
            if (_excludedWindows.Count == 0) return;
            foreach (var hWnd in _excludedWindows.ToArray())
            {
                if (hWnd == IntPtr.Zero) continue;
                if (!GetWindowRect(hWnd, out RECT r)) continue;
                try
                {
                    var rect = new System.Drawing.Rectangle(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top);
                    if (rect.Width <= 0 || rect.Height <= 0) continue;
                    switch (CurrentMaskStyle)
                    {
                        case MaskStyle.None:
                            break;
                        case MaskStyle.Black:
                            using (var brush = new SolidBrush(System.Drawing.Color.Black))
                                g.FillRectangle(brush, rect);
                            break;
                        case MaskStyle.Blur:
                            try
                            {
                                // Technique: downscale then upscale to simulate blur
                                int factor = BlurDownscaleFactor;
                                int dw = Math.Max(1, rect.Width / factor);
                                int dh = Math.Max(1, rect.Height / factor);
                                using var smallBmp = new Bitmap(dw, dh);
                                using (var sg = Graphics.FromImage(smallBmp))
                                {
                                    sg.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
                                    sg.DrawImage(g.VisibleClipBounds.IsEmpty ? new Bitmap(rect.Width, rect.Height) : ((System.Drawing.Bitmap)g.GetType().GetProperty("InternalImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(g) ?? new Bitmap(rect.Width, rect.Height)),
                                        new System.Drawing.Rectangle(0, 0, dw, dh), rect, GraphicsUnit.Pixel);
                                }
                                // Draw back stretched
                                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
                                g.DrawImage(smallBmp, rect);
                            }
                            catch
                            {
                                using var brush = new SolidBrush(System.Drawing.Color.Black);
                                g.FillRectangle(brush, rect);
                            }
                            break;
                    }
                }
                catch { /* ignore individual failures */ }
            }
        }

        private Mat CaptureFullScreen()
        {
            var screen = System.Windows.Forms.Screen.PrimaryScreen;
            if (screen?.Bounds == null)
                throw new InvalidOperationException("Unable to get primary screen bounds");

            var bounds = screen.Bounds;
            using (var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bmp.Size);
                    if (CurrentMaskStyle == MaskStyle.Black || CurrentMaskStyle == MaskStyle.Blur)
                        MaskExcludedWindows(g);
                }
                var mat = BitmapConverter.ToMat(bmp);
                if (CurrentMaskStyle == MaskStyle.Gaussian)
                {
                    ApplyMatMask(mat, bounds);
                }
                return mat;
            }
        }

    private Mat CaptureAllScreens()
        {
            var allScreens = System.Windows.Forms.Screen.AllScreens;
            var totalBounds = allScreens.Aggregate(System.Drawing.Rectangle.Empty, 
                (current, screen) => System.Drawing.Rectangle.Union(current, screen.Bounds));

            using (var bmp = new Bitmap(totalBounds.Width, totalBounds.Height, PixelFormat.Format24bppRgb))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    foreach (var screen in allScreens)
                    {
                        var offsetX = screen.Bounds.X - totalBounds.X;
                        var offsetY = screen.Bounds.Y - totalBounds.Y;
                        g.CopyFromScreen(screen.Bounds.X, screen.Bounds.Y, offsetX, offsetY, screen.Bounds.Size);
                    }
                    if (CurrentMaskStyle == MaskStyle.Black || CurrentMaskStyle == MaskStyle.Blur)
                        MaskExcludedWindows(g);
                }
                var mat = BitmapConverter.ToMat(bmp);
                if (CurrentMaskStyle == MaskStyle.Gaussian)
                {
                    ApplyMatMask(mat, totalBounds);
                }
                return mat;
            }
        }

        private Mat CaptureSpecificWindow()
        {
            if (TargetWindowHandle == IntPtr.Zero)
                throw new InvalidOperationException("No target window specified");

            if (!GetWindowRect(TargetWindowHandle, out RECT rect))
                throw new InvalidOperationException("Unable to get window bounds");

            var width = rect.Right - rect.Left;
            var height = rect.Bottom - rect.Top;

            if (width <= 0 || height <= 0)
                throw new InvalidOperationException("Invalid window dimensions");

            using (var bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    var hdc = g.GetHdc();
                    try
                    {
                        // Try PrintWindow first (works for most applications)
                        if (!PrintWindow(TargetWindowHandle, hdc, 0))
                        {
                            // Fallback to screen capture of window area
                            g.ReleaseHdc(hdc);
                            g.CopyFromScreen(rect.Left, rect.Top, 0, 0, bmp.Size);
                        }
                        else
                        {
                            g.ReleaseHdc(hdc);
                        }
                    }
                    catch
                    {
                        g.ReleaseHdc(hdc);
                        g.CopyFromScreen(rect.Left, rect.Top, 0, 0, bmp.Size);
                    }
                }
                return BitmapConverter.ToMat(bmp);
            }
        }

        private Mat CaptureCustomRegion()
        {
            if (CustomRegion.Width <= 0 || CustomRegion.Height <= 0)
                throw new InvalidOperationException("Invalid custom region specified");

            using (var bmp = new Bitmap(CustomRegion.Width, CustomRegion.Height, PixelFormat.Format24bppRgb))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(CustomRegion.X, CustomRegion.Y, 0, 0, 
                        new System.Drawing.Size(CustomRegion.Width, CustomRegion.Height));
                }
                return BitmapConverter.ToMat(bmp);
            }
        }

        public bool HasSignificantChange(Mat currentFrame)
        {
            if (_previousFrame == null)
            {
                _previousFrame = currentFrame.Clone();
                return true;
            }

            try
            {
                using (var diff = new Mat())
                {
                    Cv2.Absdiff(_previousFrame, currentFrame, diff);
                    Cv2.Threshold(diff, diff, 30, 255, ThresholdTypes.Binary);
                    var changePixels = Cv2.CountNonZero(diff);
                    var totalPixels = diff.Width * diff.Height;
                    var changePercent = changePixels / (double)totalPixels;
                    if (changePercent > _changeThreshold)
                    {
                        _previousFrame?.Dispose();
                        _previousFrame = currentFrame.Clone();
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error detecting frame changes: {ex.Message}");
                return true; // Process frame on error to be safe
            }
        }

        private void ApplyMatMask(Mat mat, System.Drawing.Rectangle desktopBounds)
        {
            if (CurrentMaskStyle != MaskStyle.Gaussian || _excludedWindows.Count == 0) return;

            foreach (var hWnd in _excludedWindows.ToArray())
            {
                if (hWnd == IntPtr.Zero) continue;
                if (!GetWindowRect(hWnd, out RECT r)) continue;
                int x = r.Left - desktopBounds.Left;
                int y = r.Top - desktopBounds.Top;
                int w = (r.Right - r.Left);
                int h = (r.Bottom - r.Top);
                if (w <= 0 || h <= 0) continue;
                x = Math.Max(0, x); y = Math.Max(0, y);
                if (x >= mat.Width || y >= mat.Height) continue;
                w = Math.Min(w, mat.Width - x);
                h = Math.Min(h, mat.Height - y);
                if (w <= 1 || h <= 1) continue;
                var roiRect = new Rect(x, y, w, h);
                using var roi = new Mat(mat, roiRect);
                int factor = BlurDownscaleFactor;
                int dw = Math.Max(1, roi.Width / factor);
                int dh = Math.Max(1, roi.Height / factor);
                using var small = new Mat();
                Cv2.Resize(roi, small, new OpenCvSharp.Size(dw, dh), 0, 0, InterpolationFlags.Area);
                int k = Math.Clamp(factor * 2 + 1, 3, 49);
                Cv2.GaussianBlur(small, small, new OpenCvSharp.Size(k, k), 0);
                Cv2.Resize(small, roi, new OpenCvSharp.Size(roi.Width, roi.Height), 0, 0, InterpolationFlags.Linear);
            }
        }

        public Rect DetectROI(Mat image)
        {
            try
            {
                using (var gray = new Mat())
                using (var thresh = new Mat())
                {
                    Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
                    Cv2.Threshold(gray, thresh, 0, 255, ThresholdTypes.BinaryInv | ThresholdTypes.Otsu);
                    Cv2.FindContours(thresh, out var contours, out var hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
                    
                    if (contours.Length == 0)
                    {
                        // Return full image if no contours found
                        return new Rect(0, 0, image.Width, image.Height);
                    }
                    
                    return Cv2.BoundingRect(contours.OrderByDescending(c => Cv2.ContourArea(c)).First());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error detecting ROI: {ex.Message}");
                // Return full image as fallback
                return new Rect(0, 0, image.Width, image.Height);
            }
        }

        // Optionally, add Analyze() for pipeline
        public string Analyze()
        {
            try
            {
                using (var mat = CaptureScreen())
                {
                    if (!HasSignificantChange(mat))
                    {
                        return "[No significant changes detected]";
                    }

                    var roi = DetectROI(mat);
                    using (var roiMat = new Mat(mat, roi))
                    {
                        // OCR should be called externally
                        return "[Screen text extraction ready for OCR]";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in screen analysis: {ex.Message}");
                return "[Error in screen analysis]";
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _previousFrame?.Dispose();
                _disposed = true;
            }
        }

        // Configuration methods for different capture modes
        public void SetCaptureMode(CaptureMode mode)
        {
            CaptureMode = mode;
            Console.WriteLine($"🎯 Mode de capture défini: {mode}");
        }

        public void SetTargetWindow(IntPtr windowHandle, string windowTitle = "")
        {
            TargetWindowHandle = windowHandle;
            CaptureMode = CaptureMode.SpecificWindow;
            Console.WriteLine($"🪟 Fenêtre cible définie: {windowTitle} (Handle: {windowHandle})");
        }

        public void SetCustomRegion(CaptureRegion region)
        {
            CustomRegion = region;
            CaptureMode = CaptureMode.CustomRegion;
            Console.WriteLine($"📐 Région personnalisée définie: {region.X},{region.Y} {region.Width}x{region.Height}");
        }

        public void SetTargetScreen(int screenIndex)
        {
            TargetScreenIndex = screenIndex;
            if (System.Windows.Forms.Screen.AllScreens.Length > 1)
            {
                CaptureMode = CaptureMode.AllScreens;
                Console.WriteLine($"🖥️ Capture multi-écrans activée (écran cible: {screenIndex})");
            }
            else
            {
                CaptureMode = CaptureMode.FullScreen;
                Console.WriteLine("🖥️ Un seul écran détecté, utilisation du mode plein écran");
            }
        }

        // Helper method to get current capture info
        public string GetCaptureInfo()
        {
            return CaptureMode switch
            {
                CaptureMode.FullScreen => "📱 Écran principal complet",
                CaptureMode.AllScreens => $"🖥️ Tous les écrans ({System.Windows.Forms.Screen.AllScreens.Length})",
                CaptureMode.SpecificWindow => $"🪟 Fenêtre spécifique (Handle: {TargetWindowHandle})",
                CaptureMode.CustomRegion => $"📐 Région personnalisée ({CustomRegion.Width}x{CustomRegion.Height})",
                _ => "❓ Mode inconnu"
            };
        }
    }

    public enum MaskStyle
    {
    None,
    Black,
    Blur,
    Gaussian
    }
}
