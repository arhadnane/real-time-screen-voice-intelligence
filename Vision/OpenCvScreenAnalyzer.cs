using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Rectangle = OpenCvSharp.Rect;

namespace Vision
{
    public class OpenCvScreenAnalyzer : IDisposable
    {
        private Mat? _previousFrame;
        private bool _disposed = false;
        private readonly double _changeThreshold = 0.05; // 5% change threshold

        public OpenCvScreenAnalyzer()
        {
            // Enable OpenCV optimizations
            Cv2.SetUseOptimized(true);
        }

        // Cross-platform screen capture for Windows using System.Drawing
        public Mat CaptureScreen()
        {
            try
            {
                // Use System.Windows.Forms for screen bounds
                var screen = System.Windows.Forms.Screen.PrimaryScreen;
                if (screen?.Bounds == null)
                    throw new InvalidOperationException("Unable to get primary screen bounds");

                var bounds = screen.Bounds;
                using (var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb))
                {
                    using (var g = Graphics.FromImage(bmp))
                    {
                        g.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bmp.Size);
                    }
                    // Use OpenCvSharp.Extensions to convert Bitmap to Mat
                    return BitmapConverter.ToMat(bmp);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to capture screen: {ex.Message}");
                throw;
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
    }
}
