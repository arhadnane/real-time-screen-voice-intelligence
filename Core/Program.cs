using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Vision;
using Audio;
using AI;
using System.Threading;

namespace Core
{
    class Program
    {
        private static bool _isRunning = true;
        private static OpenCvScreenAnalyzer? _screenAnalyzer;
        private static OcrEngine? _ocrEngine;
        private static VoskSpeechRecognizer? _voiceEngine;
        private static AIRouter? _aiRouter;

        static async Task Main(string[] args)
        {
            Console.WriteLine("🚀 Starting Real-Time Screen & Voice Intelligence System...");
            
            // Handle graceful shutdown
            Console.CancelKeyPress += (_, e) => {
                e.Cancel = true;
                _isRunning = false;
                Console.WriteLine("\n🛑 Shutdown signal received...");
            };

            try
            {
                await InitializeSystem();
                await RunMainLoop();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Critical error: {ex.Message}");
            }
            finally
            {
                await Cleanup();
            }
        }

        private static Task InitializeSystem()
        {
            Console.WriteLine("⚙️ Loading configuration...");
            
            // Load configuration
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("Core/appsettings.json", optional: false)
                .Build();

            Console.WriteLine("📹 Initializing Vision components...");
            // Vision
            _screenAnalyzer = new OpenCvScreenAnalyzer();
            _ocrEngine = new OcrEngine("./tessdata");

            Console.WriteLine("🎤 Initializing Audio components...");
            // Audio
            try
            {
                _voiceEngine = new VoskSpeechRecognizer("vosk-model-en-us-0.22");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Voice engine initialization failed: {ex.Message}");
                Console.WriteLine("📝 Continuing without voice recognition...");
            }

            Console.WriteLine("🤖 Initializing AI components...");
            // AI
            _aiRouter = new AIRouter(
                new OllamaProvider(config["AI:OllamaEndpoint"] ?? "http://localhost:11434"),
                new HuggingFaceProvider(config["AI:HF_Token"] ?? "")
            );

            Console.WriteLine("✅ System initialization complete!");
            Console.WriteLine("📊 Press Ctrl+C to stop gracefully...\n");
            
            return Task.CompletedTask;
        }

        private static async Task RunMainLoop()
        {
            int iteration = 0;
            DateTime lastProcessTime = DateTime.UtcNow;

            while (_isRunning)
            {
                try
                {
                    iteration++;
                    var currentTime = DateTime.UtcNow;
                    
                    Console.WriteLine($"🔄 Processing iteration {iteration} [{currentTime:HH:mm:ss}]");

                    // Capture and analyze screen with change detection
                    string screenText = "[No screen analysis]";
                    if (_screenAnalyzer != null && _ocrEngine != null)
                    {
                        try
                        {
                            using (var screenMat = _screenAnalyzer.CaptureScreen())
                            {
                                if (_screenAnalyzer.HasSignificantChange(screenMat))
                                {
                                    Console.WriteLine("📸 Significant screen changes detected, analyzing...");
                                    var roi = _screenAnalyzer.DetectROI(screenMat);
                                    using (var roiMat = new OpenCvSharp.Mat(screenMat, roi))
                                    {
                                        screenText = _ocrEngine.RunTesseract(roiMat);
                                        if (!string.IsNullOrWhiteSpace(screenText))
                                        {
                                            Console.WriteLine($"📄 Screen text: {screenText.Substring(0, Math.Min(100, screenText.Length))}...");
                                        }
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("⏭️ No significant screen changes");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Screen analysis error: {ex.Message}");
                            screenText = "[Screen analysis error]";
                        }
                    }

                    // Get latest voice transcription
                    string voiceText = "[No voice recognition]";
                    if (_voiceEngine != null)
                    {
                        try
                        {
                            voiceText = _voiceEngine.GetLatestTranscription();
                            if (!string.IsNullOrWhiteSpace(voiceText))
                            {
                                Console.WriteLine($"🎤 Voice input: {voiceText}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Voice recognition error: {ex.Message}");
                            voiceText = "[Voice recognition error]";
                        }
                    }

                    // Only call AI if we have meaningful input
                    if ((!string.IsNullOrWhiteSpace(screenText) && screenText != "[No screen analysis]" && screenText != "[Screen analysis error]") ||
                        (!string.IsNullOrWhiteSpace(voiceText) && voiceText != "[No voice recognition]" && voiceText != "[Voice recognition error]"))
                    {
                        // Combine contexts
                        var context = $"Screen: {screenText}\nVoice: {voiceText}\nTime: {currentTime:yyyy-MM-dd HH:mm:ss}";

                        // Get AI response
                        if (_aiRouter != null)
                        {
                            try
                            {
                                Console.WriteLine("🤖 Requesting AI analysis...");
                                var response = await _aiRouter.GetResponse(context);
                                Console.WriteLine($"💡 AI Response: {response}\n");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"⚠️ AI processing error: {ex.Message}\n");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("⏸️ No meaningful input detected, skipping AI call\n");
                    }

                    // Adaptive delay based on activity
                    var processingTime = DateTime.UtcNow - currentTime;
                    var delayTime = processingTime.TotalMilliseconds > 1000 ? 1000 : 2000;
                    
                    // Use cancellation token for responsive shutdown
                    var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(delayTime));
                    try
                    {
                        await Task.Delay(delayTime, cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        // Expected when shutting down
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error in main loop iteration {iteration}: {ex.Message}");
                    Console.WriteLine("⏳ Waiting 5 seconds before retry...");
                    await Task.Delay(5000);
                }
            }
        }

        private static Task Cleanup()
        {
            Console.WriteLine("🧹 Cleaning up resources...");
            
            try
            {
                _screenAnalyzer?.Dispose();
                // Note: OcrEngine and VoskSpeechRecognizer don't implement IDisposable
                // They should be enhanced to do so for proper resource management
                _aiRouter?.Dispose();
                
                Console.WriteLine("✅ Cleanup completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error during cleanup: {ex.Message}");
            }
            
            Console.WriteLine("👋 System shutdown complete. Goodbye!");
            return Task.CompletedTask;
        }
    }
}
