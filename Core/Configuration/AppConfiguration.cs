using System.ComponentModel.DataAnnotations;

namespace Core.Configuration
{
    public class AppConfiguration
    {
        public AIConfiguration AI { get; set; } = new();
        public VisionConfiguration Vision { get; set; } = new();
        public AudioConfiguration Audio { get; set; } = new();
        public LoggingConfiguration Logging { get; set; } = new();

        public void Validate()
        {
            AI.Validate();
            Vision.Validate();
            Audio.Validate();
            Logging.Validate();
        }
    }

    public class AIConfiguration
    {
        [Required]
        public string PrimaryEngine { get; set; } = "Ollama";
        
        public string FallbackEngine { get; set; } = "HuggingFace";
        
        [Required]
        [Url]
        public string OllamaEndpoint { get; set; } = "http://localhost:11434";
        
        [Required]
        public string OllamaModel { get; set; } = "phi3:mini";
        
        public string HF_Token { get; set; } = "";
        
        [Range(1, 60)]
        public int TimeoutSeconds { get; set; } = 30;
        
        [Range(1, 10)]
        public int MaxRetries { get; set; } = 3;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(OllamaEndpoint))
                throw new ArgumentException("OllamaEndpoint is required");
            
            if (!Uri.TryCreate(OllamaEndpoint, UriKind.Absolute, out _))
                throw new ArgumentException("OllamaEndpoint must be a valid URL");
            
            if (string.IsNullOrWhiteSpace(OllamaModel))
                throw new ArgumentException("OllamaModel is required");
        }
    }

    public class VisionConfiguration
    {
        public bool UseOpenCV { get; set; } = true;
        
        [Range(1, 30)]
        public int CaptureFPS { get; set; } = 5;
        
        public bool ROIDetection { get; set; } = true;
        
        [Required]
        public string TessDataPath { get; set; } = "./tessdata";
        
        public string OcrLanguages { get; set; } = "eng+fra";
        
        [Range(0.01, 1.0)]
        public double ChangeThreshold { get; set; } = 0.05;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(TessDataPath))
                throw new ArgumentException("TessDataPath is required");
                
            if (!Directory.Exists(TessDataPath))
                throw new DirectoryNotFoundException($"TessData directory not found: {TessDataPath}");
        }
    }

    public class AudioConfiguration
    {
        [Required]
        public string Engine { get; set; } = "VOSK";
        
        [Range(8000, 48000)]
        public int SampleRate { get; set; } = 16000;
        
        public string ModelPath { get; set; } = "./vosk-models";
        
        [Range(50, 1000)]
        public int BufferMilliseconds { get; set; } = 100;
        
        [Range(2, 10)]
        public int NumberOfBuffers { get; set; } = 3;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Engine))
                throw new ArgumentException("Audio Engine is required");
                
            // Ne pas valider le ModelPath si l'audio est désactivé
            if (Engine != "None" && !Directory.Exists(ModelPath))
                throw new DirectoryNotFoundException($"Audio model directory not found: {ModelPath}");
        }
    }

    public class LoggingConfiguration
    {
        public string MinimumLevel { get; set; } = "Information";
        public bool EnableConsole { get; set; } = true;
        public bool EnableFile { get; set; } = true;
        public string LogFilePath { get; set; } = "logs/app-.log";
        public int RetainedFileCountLimit { get; set; } = 7;

        public void Validate()
        {
            var validLevels = new[] { "Verbose", "Debug", "Information", "Warning", "Error", "Fatal" };
            if (!validLevels.Contains(MinimumLevel))
                throw new ArgumentException($"Invalid log level: {MinimumLevel}");
        }
    }
}
