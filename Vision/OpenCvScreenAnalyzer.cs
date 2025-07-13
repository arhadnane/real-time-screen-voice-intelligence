using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Rectangle = OpenCvSharp.Rect;

namespace Vision
{
    public class OpenCvScreenAnalyzer
    {
        // Cross-platform screen capture for Windows using System.Drawing
        public Mat CaptureScreen()
        {
            // Use System.Windows.Forms for screen bounds
            var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
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

        public Rect DetectROI(Mat image)
        {
            using (var gray = new Mat())
            {
                Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
                Cv2.Threshold(gray, gray, 0, 255, ThresholdTypes.BinaryInv | ThresholdTypes.Otsu);
                Cv2.FindContours(gray, out var contours, out var hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
                return Cv2.BoundingRect(contours.OrderByDescending(c => Cv2.ContourArea(c)).First());
            }
        }

        // Optionally, add Analyze() for pipeline
        public string Analyze()
        {
            var mat = CaptureScreen();
            var roi = DetectROI(mat);
            var roiMat = new Mat(mat, roi);
            // OCR should be called externally
            return "[Screen text extraction not implemented here]";
        }
    }
}
