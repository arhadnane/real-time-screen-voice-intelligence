using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Vision;
using Audio;
using AI;

namespace Core
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Load configuration
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("Core/appsettings.json", optional: false)
                .Build();

            // Vision
            var screenAnalyzer = new Vision.OpenCvScreenAnalyzer();
            var ocrEngine = new Vision.OcrEngine("./tessdata");

            // Audio
            var voiceEngine = new Audio.VoskSpeechRecognizer("vosk-model-en-us-0.22");

            // AI
            var aiRouter = new AI.AIRouter(
                new AI.OllamaProvider(config["AI:OllamaEndpoint"] ?? "http://localhost:11434"),
                new AI.HuggingFaceProvider(config["AI:HF_Token"] ?? "")
            );

            while (true)
            {
                // Capture and analyze screen
                var screenMat = screenAnalyzer.CaptureScreen();
                var roi = screenAnalyzer.DetectROI(screenMat);
                var roiMat = new OpenCvSharp.Mat(screenMat, roi);
                var screenText = ocrEngine.RunTesseract(roiMat);

                // Get latest voice transcription
                var voiceText = voiceEngine.GetLatestTranscription();

                // Combine contexts
                var context = $"Screen: {screenText}\nVoice: {voiceText}";

                // Get AI response
                var response = await aiRouter.GetResponse(context);

                Console.WriteLine($"AI: {response}");
                await Task.Delay(2000); // 2 second interval
            }
        }
    }
}
