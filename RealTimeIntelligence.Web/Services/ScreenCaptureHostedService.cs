using Microsoft.AspNetCore.SignalR;
using RealTimeIntelligence.Web.Hubs;
using Serilog;
using OpenCvSharp;

namespace RealTimeIntelligence.Web.Services;

public class ScreenCaptureHostedService : IHostedService, IDisposable
{
    private readonly IScreenCaptureService _capture;
    private readonly IHubContext<RealTimeHub> _hub;
    private readonly IConfiguration _config;
    private readonly Serilog.ILogger _logger = Log.ForContext<ScreenCaptureHostedService>();
    private Timer? _timer;
    private int _intervalMs;
    private bool _running;

    public ScreenCaptureHostedService(IScreenCaptureService capture, IHubContext<RealTimeHub> hub, IConfiguration config)
    {
        _capture = capture;
        _hub = hub;
        _config = config;
        _intervalMs = Math.Clamp(_config.GetValue("Vision:CaptureIntervalMs", 1000), 250, 10000);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_running) return Task.CompletedTask;
        _running = true;
        _timer = new Timer(async _ => await Tick(), null, _intervalMs, _intervalMs);
        _logger.Information("ScreenCaptureHostedService started (interval {Interval} ms)", _intervalMs);
        return Task.CompletedTask;
    }

    private Mat? _lastMat;
    private double _minChangeRatio = 0.01; // 1% pixels
    private bool _useJpeg = true;
    private int _jpegQuality = 70;

    public void Tune(double? minChangeRatio = null, bool? useJpeg = null, int? jpegQuality = null)
    {
        if (minChangeRatio.HasValue) _minChangeRatio = Math.Clamp(minChangeRatio.Value, 0, 0.5);
        if (useJpeg.HasValue) _useJpeg = useJpeg.Value;
        if (jpegQuality.HasValue) _jpegQuality = Math.Clamp(jpegQuality.Value, 10, 100);
    }

    private async Task Tick()
    {
        if (!_running) return;
        try
        {
            var result = await _capture.CaptureAsync();
            if (!result.Success || result.ImageBytes == null) return;

            // Decode to Mat for delta detection
            using var current = OpenCvSharp.Mat.ImDecode(result.ImageBytes);
            bool send = Vision.DeltaChangeDetector.ShouldSend(_lastMat, current, _minChangeRatio);

            if (send)
            {
                _lastMat?.Dispose();
                _lastMat = current.Clone();

                byte[] payloadBytes = result.ImageBytes;
                if (_useJpeg)
                {
                    try
                    {
                        var jpeg = current.ImEncode(".jpg", new int[] { (int)OpenCvSharp.ImwriteFlags.JpegQuality, _jpegQuality });
                        payloadBytes = jpeg;
                    }
                    catch { /* fallback to png bytes */ }
                }

                await _hub.Clients.Group("screen").SendAsync("ScreenFrame", new {
                    timestamp = result.Timestamp,
                    imageBase64 = Convert.ToBase64String(payloadBytes),
                    text = result.Text,
                    encoding = _useJpeg ? "jpeg" : "png"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Capture tick failed");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _running = false;
        _timer?.Change(Timeout.Infinite, Timeout.Infinite);
        _logger.Information("ScreenCaptureHostedService stopped");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    _lastMat?.Dispose();
    _capture.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }
}
