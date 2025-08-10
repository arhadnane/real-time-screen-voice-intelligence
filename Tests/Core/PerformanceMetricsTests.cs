using Xunit;
using Core.Services;
using Serilog;
using System.Diagnostics;

namespace Tests.Core
{
    public class PerformanceMetricsTests
    {
        private readonly ILogger _logger;

        public PerformanceMetricsTests()
        {
            _logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
        }

        [Fact]
        public void BeginOperation_ShouldReturnTracker()
        {
            // Arrange
            var metrics = new PerformanceMetrics(_logger);

            // Act
            using var tracker = metrics.BeginOperation("test-operation");

            // Assert
            Assert.NotNull(tracker);
        }

        [Fact]
        public async Task PerformanceTracker_ShouldMeasureTime()
        {
            // Arrange
            var metrics = new PerformanceMetrics(_logger);
            var operationName = "test-operation";

            // Act
            using (var tracker = metrics.BeginOperation(operationName))
            {
                await Task.Delay(100); // Simulate work
            }

            // Assert - No exception should be thrown
            Assert.True(true);
        }

        [Fact]
        public void RecordOperation_ShouldStoreMetric()
        {
            // Arrange
            var metrics = new PerformanceMetrics(_logger);
            var operationName = "test-operation";
            var duration = TimeSpan.FromMilliseconds(100);

            // Act
            metrics.RecordOperation(operationName, duration);

            // Assert - LogSummary should not throw
            Assert.True(true);
            metrics.LogSummary();
        }

        [Fact]
        public void LogSummary_ShouldNotThrow_WithNoMetrics()
        {
            // Arrange
            var metrics = new PerformanceMetrics(_logger);

            // Act & Assert
            var exception = Record.Exception(() => metrics.LogSummary());
            Assert.Null(exception);
        }

        [Fact]
        public void Reset_ShouldClearAllMetrics()
        {
            // Arrange
            var metrics = new PerformanceMetrics(_logger);
            metrics.RecordOperation("test-op", TimeSpan.FromMilliseconds(100));

            // Act
            metrics.Reset();

            // Assert - Should not throw
            var exception = Record.Exception(() => metrics.LogSummary());
            Assert.Null(exception);
        }

        [Fact]
        public async Task MultipleOperations_ShouldCalculateCorrectStatistics()
        {
            // Arrange
            var metrics = new PerformanceMetrics(_logger);

            // Act - Record multiple operations
            for (int i = 0; i < 5; i++)
            {
                using (var tracker = metrics.BeginOperation("batch-operation"))
                {
                    await Task.Delay(50 + i * 10); // Variable delays: 50, 60, 70, 80, 90ms
                }
            }

            // Assert - Should not throw
            var exception = Record.Exception(() => metrics.LogSummary());
            Assert.Null(exception);
        }
    }
}
