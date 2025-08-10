using OpenCvSharp;

namespace Vision;

/// <summary>
/// Provides a reusable method to decide if a new frame should be sent based on pixel difference ratio.
/// </summary>
public static class DeltaChangeDetector
{
    /// <summary>
    /// Computes whether the change ratio between last and current frames exceeds the threshold.
    /// Both Mats must be non-empty, same size and type.
    /// </summary>
    public static bool ShouldSend(Mat? last, Mat current, double threshold)
    {
        if (current.Empty()) return false;
        if (threshold <= 0) return true; // always send
        if (last == null || last.Empty()) return true; // first frame
        if (last.Size() != current.Size() || last.Type() != current.Type()) return true; // size change

        using var diff = new Mat();
        Cv2.Absdiff(last, current, diff);
        if (diff.Empty()) return true;
        // Convert to grayscale for simpler count
        if (diff.Channels() > 1)
            Cv2.CvtColor(diff, diff, ColorConversionCodes.BGR2GRAY);
        var changed = Cv2.CountNonZero(diff);
        double total = diff.Width * diff.Height;
        if (total <= 0) return true;
        double ratio = changed / total;
        return ratio >= threshold;
    }
}
