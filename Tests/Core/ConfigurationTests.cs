using Xunit;
using Microsoft.Extensions.Configuration;
using Core.Configuration;
using System.ComponentModel.DataAnnotations;

namespace Tests.Core
{
    public class ConfigurationTests
    {
        [Fact]
        public void AIConfiguration_ShouldValidateSuccessfully_WithValidData()
        {
            // Arrange
            var config = new AIConfiguration
            {
                PrimaryEngine = "Ollama",
                OllamaEndpoint = "http://localhost:11434",
                OllamaModel = "phi3:mini",
                TimeoutSeconds = 30,
                MaxRetries = 3
            };

            // Act & Assert
            var exception = Record.Exception(() => config.Validate());
            Assert.Null(exception);
        }

        [Fact]
        public void AIConfiguration_ShouldThrowException_WithInvalidEndpoint()
        {
            // Arrange
            var config = new AIConfiguration
            {
                OllamaEndpoint = "invalid-url",
                OllamaModel = "phi3:mini"
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => config.Validate());
        }

        [Fact]
        public void VisionConfiguration_ShouldThrowException_WithNonExistentTessDataPath()
        {
            // Arrange
            var config = new VisionConfiguration
            {
                TessDataPath = "/non/existent/path"
            };

            // Act & Assert
            Assert.Throws<DirectoryNotFoundException>(() => config.Validate());
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void AIConfiguration_ShouldThrowException_WithEmptyModel(string model)
        {
            // Arrange
            var config = new AIConfiguration
            {
                OllamaEndpoint = "http://localhost:11434",
                OllamaModel = model
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => config.Validate());
        }

        [Fact]
        public void AudioConfiguration_ShouldValidateSuccessfully_WithValidData()
        {
            // Arrange
            var config = new AudioConfiguration
            {
                Engine = "VOSK",
                SampleRate = 16000,
                BufferMilliseconds = 100,
                NumberOfBuffers = 3
            };

            // Act & Assert
            var exception = Record.Exception(() => config.Validate());
            Assert.Null(exception);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void AudioConfiguration_ShouldThrowException_WithEmptyEngine(string engine)
        {
            // Arrange
            var config = new AudioConfiguration
            {
                Engine = engine
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => config.Validate());
        }

        [Theory]
        [InlineData("Verbose")]
        [InlineData("Debug")]
        [InlineData("Information")]
        [InlineData("Warning")]
        [InlineData("Error")]
        [InlineData("Fatal")]
        public void LoggingConfiguration_ShouldValidateSuccessfully_WithValidLogLevels(string logLevel)
        {
            // Arrange
            var config = new LoggingConfiguration
            {
                MinimumLevel = logLevel
            };

            // Act & Assert
            var exception = Record.Exception(() => config.Validate());
            Assert.Null(exception);
        }

        [Theory]
        [InlineData("Invalid")]
        [InlineData("TRACE")]
        [InlineData("")]
        [InlineData(null)]
        public void LoggingConfiguration_ShouldThrowException_WithInvalidLogLevels(string logLevel)
        {
            // Arrange
            var config = new LoggingConfiguration
            {
                MinimumLevel = logLevel
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => config.Validate());
        }
    }
}
