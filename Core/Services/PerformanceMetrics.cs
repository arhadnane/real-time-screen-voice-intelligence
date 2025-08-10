using System.Diagnostics;
using Serilog;

namespace Core.Services
{
    public class PerformanceMetrics
    {
        private readonly ILogger _logger;
        private readonly Dictionary<string, List<TimeSpan>> _metrics;
        private readonly object _lock = new object();

        public PerformanceMetrics(ILogger logger)
        {
            _logger = logger;
            _metrics = new Dictionary<string, List<TimeSpan>>();
        }

        public IDisposable BeginOperation(string operationName)
        {
            return new PerformanceTracker(operationName, this);
        }

        public void RecordOperation(string operationName, TimeSpan duration)
        {
            lock (_lock)
            {
                if (!_metrics.ContainsKey(operationName))
                {
                    _metrics[operationName] = new List<TimeSpan>();
                }

                _metrics[operationName].Add(duration);

                // Log if operation is slow
                if (duration.TotalSeconds > 5)
                {
                    _logger.Warning("Slow operation detected: {Operation} took {Duration}ms", 
                        operationName, duration.TotalMilliseconds);
                }
                else
                {
                    _logger.Debug("Operation {Operation} completed in {Duration}ms", 
                        operationName, duration.TotalMilliseconds);
                }
            }
        }

        public void LogSummary()
        {
            lock (_lock)
            {
                _logger.Information("=== PERFORMANCE SUMMARY ===");
                
                foreach (var kvp in _metrics)
                {
                    var operations = kvp.Value;
                    if (operations.Count == 0) continue;

                    var average = operations.Average(t => t.TotalMilliseconds);
                    var min = operations.Min(t => t.TotalMilliseconds);
                    var max = operations.Max(t => t.TotalMilliseconds);
                    
                    _logger.Information("{Operation}: Count={Count}, Avg={Average:F2}ms, Min={Min:F2}ms, Max={Max:F2}ms", 
                        kvp.Key, operations.Count, average, min, max);
                }
                
                _logger.Information("===============================");
            }
        }

        public void Reset()
        {
            lock (_lock)
            {
                _metrics.Clear();
            }
        }
    }

    public class PerformanceTracker : IDisposable
    {
        private readonly string _operationName;
        private readonly PerformanceMetrics _metrics;
        private readonly Stopwatch _stopwatch;
        private bool _disposed = false;

        public PerformanceTracker(string operationName, PerformanceMetrics metrics)
        {
            _operationName = operationName;
            _metrics = metrics;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _stopwatch.Stop();
                _metrics.RecordOperation(_operationName, _stopwatch.Elapsed);
                _disposed = true;
            }
        }
    }
}
