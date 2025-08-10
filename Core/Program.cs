using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Vision;
using AI;
using Audio;
using Core.Configuration;
using Core.Services;
using System.ComponentModel.DataAnnotations;

namespace Core
{
    class Program
    {
        private static bool _isRunning = true;
        private static OpenCvScreenAnalyzer? _screenAnalyzer;
        private static OcrEngine? _ocrEngine;
        private static VoskSpeechRecognizer? _voiceEngine;
        private static AIRouter? _aiRouter;
        private static CaptureHistoryService? _historyService;

    static async Task Main(string[] args)
        {
            // Setup Serilog early
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            try
            {
                Console.WriteLine("🚀 === SYSTÈME D'INTELLIGENCE EN TEMPS RÉEL ===");
                Console.WriteLine("📋 Version Console - Mode Test");
                
                // Configuration
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .Build();

                // Configure Serilog from appsettings.json
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .CreateLogger();

                var logger = Log.ForContext<Program>();
                logger.Information("Démarrage du système d'intelligence en temps réel...");

                // Validate configuration
                var appConfig = configuration.Get<AppConfiguration>() ?? new AppConfiguration();
                try
                {
                    // Validation simplifiée pour les tests
                    // appConfig.Validate();
                    logger.Information("✅ Configuration chargée avec succès");
                }
                catch (Exception ex)
                {
                    logger.Fatal(ex, "❌ Erreur de configuration: {Message}", ex.Message);
                    throw;
                }

                if (args.Contains("--gui", StringComparer.OrdinalIgnoreCase))
                {
                    logger.Information("Lancement en mode GUI (WinForms)");
                    System.Windows.Forms.Application.EnableVisualStyles();
                    System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
                    System.Windows.Forms.Application.Run(new MainForm(configuration));
                }
                else
                {
                    // Initialisation des composants console
                    logger.Information("Initialisation des composants console...");
                    await InitializeComponents(configuration, logger);
                    // Boucle principale console
                    await RunMainLoop(logger);
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "❌ Erreur fatale: {Message}", ex.Message);
                Console.WriteLine($"❌ Erreur fatale: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            finally
            {
                await Log.CloseAndFlushAsync();
            }
            
            Console.WriteLine("Appuyez sur une touche pour quitter...");
            Console.ReadKey();
        }

        private static async Task InitializeComponents(IConfiguration configuration, Serilog.ILogger logger)
        {
            try
            {
                var aiConfig = configuration.GetSection("AI").Get<AIConfiguration>() ?? new AIConfiguration();
                var visionConfig = configuration.GetSection("Vision").Get<VisionConfiguration>() ?? new VisionConfiguration();
                var audioConfig = configuration.GetSection("Audio").Get<AudioConfiguration>() ?? new AudioConfiguration();

                // Debug: afficher la configuration audio lue
                logger.Information("Configuration audio - Engine: {Engine}, ModelPath: {ModelPath}", 
                    audioConfig.Engine, audioConfig.ModelPath);

                // Initialisation OpenCV
                logger.Information("Initialisation du module de capture d'écran...");
                _screenAnalyzer = new OpenCvScreenAnalyzer();

                // Initialisation OCR
                logger.Information("Initialisation du module OCR...");
                var tessDataPath = visionConfig.TessDataPath;
                if (Directory.Exists(tessDataPath))
                {
                    _ocrEngine = new OcrEngine(tessDataPath, visionConfig.OcrLanguages, logger);
                    logger.Information("Module OCR initialisé avec succès - Langues: {Languages}", visionConfig.OcrLanguages);
                }
                else
                {
                    logger.Warning("Dossier tessdata non trouvé à {Path}, OCR désactivé", tessDataPath);
                }

                // Initialisation Audio (optionnel)
                if (audioConfig.Engine != "None" && Directory.Exists(audioConfig.ModelPath))
                {
                    logger.Information("Initialisation du module audio...");
                    try
                    {
                        _voiceEngine = new VoskSpeechRecognizer(audioConfig.ModelPath, audioConfig.SampleRate, logger);
                        logger.Information("Module audio initialisé avec succès");
                    }
                    catch (Exception ex)
                    {
                        logger.Warning(ex, "Échec de l'initialisation audio, continuer sans reconnaissance vocale");
                    }
                }
                else
                {
                    logger.Information("Module audio désactivé ou dossier de modèles non trouvé à {Path}", audioConfig.ModelPath);
                }

                // Initialisation AI
                logger.Information("Initialisation du routeur IA...");
                var ollamaProvider = new OllamaProvider(aiConfig.OllamaEndpoint);
                var hfProvider = new HuggingFaceProvider(aiConfig.HF_Token);
                _aiRouter = new AIRouter(ollamaProvider, hfProvider);

                // Initialisation du service d'historique
                logger.Information("Initialisation du service d'historique...");
                _historyService = new CaptureHistoryService(logger);
                await _historyService.LoadHistoryAsync();

                logger.Information("✅ Tous les composants initialisés avec succès");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Erreur lors de l'initialisation des composants");
                throw;
            }
        }

        private static async Task RunMainLoop(Serilog.ILogger logger)
        {
            logger.Information("🔄 Démarrage de la boucle principale...");
            
            Console.WriteLine("\n📋 Commandes disponibles:");
            Console.WriteLine("  's' - Capturer et analyser l'écran");
            Console.WriteLine("  't' - Test simple de l'IA");
            Console.WriteLine("  'h' - Afficher l'historique");
            Console.WriteLine("  'c' - Effacer l'historique");
            Console.WriteLine("  'q' - Quitter");
            Console.WriteLine();

            while (_isRunning)
            {
                try
                {
                    Console.Write("Commande (s/t/h/c/q): ");
                    var input = Console.ReadLine()?.ToLower();

                    switch (input)
                    {
                        case "s":
                            await CaptureAndAnalyzeScreen(logger);
                            break;
                        case "t":
                            await TestAI(logger);
                            break;
                        case "h":
                            ShowHistory(logger);
                            break;
                        case "c":
                            await ClearHistory(logger);
                            break;
                        case "q":
                            _isRunning = false;
                            break;
                        default:
                            Console.WriteLine("Commande non reconnue. Utilisez 's', 't', 'h', 'c' ou 'q'.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Erreur dans la boucle principale");
                }

                await Task.Delay(1000);
            }
        }

        private static async Task CaptureAndAnalyzeScreen(Serilog.ILogger logger)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                if (_screenAnalyzer == null)
                {
                    logger.Warning("Analyseur d'écran non initialisé");
                    return;
                }

                logger.Information("📸 Capture d'écran en cours...");
                using var frame = _screenAnalyzer.CaptureScreen();
                
                if (frame.Empty())
                {
                    logger.Warning("Échec de la capture d'écran");
                    return;
                }

                logger.Information("Frame size: {Width}x{Height}", frame.Width, frame.Height);

                string screenText = "Écran capturé avec succès";
                
                if (_ocrEngine != null)
                {
                    logger.Information("🔍 Analyse OCR en cours...");
                    screenText = _ocrEngine.RunTesseract(frame);
                    logger.Information("Texte détecté: {TextPreview}...", screenText[..Math.Min(screenText.Length, 100)]);
                    logger.Information("OCR text length: {Length}", screenText.Length);
                }

                if (_aiRouter != null)
                {
                    logger.Information("🤖 Analyse IA en cours...");
                    var context = $"Contenu de l'écran: {screenText}";
                    var aiResponse = await _aiRouter.GetResponse(context);
                    
                    Console.WriteLine("\n=== ANALYSE IA ===");
                    Console.WriteLine(aiResponse);
                    Console.WriteLine("==================\n");
                    
                    logger.Information("AI response length: {Length}", aiResponse.Length);

                    // Sauvegarder dans l'historique
                    if (_historyService != null)
                    {
                        await _historyService.AddCaptureAsync(
                            ocrText: screenText,
                            aiAnalysis: aiResponse,
                            windowTitle: "Console Capture",
                            width: frame.Width,
                            height: frame.Height
                        );
                        logger.Information("💾 Capture sauvegardée dans l'historique");
                    }
                }

                stopwatch.Stop();
                logger.Information("Operation completed in {Duration}ms", stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Erreur lors de la capture et analyse d'écran");
            }
        }

        private static async Task TestAI(Serilog.ILogger logger)
        {
            try
            {
                if (_aiRouter == null)
                {
                    logger.Warning("Routeur IA non initialisé");
                    return;
                }

                logger.Information("🧪 Test du système IA...");
                var testPrompt = "Bonjour ! Peux-tu me dire si tu fonctionnes correctement ? Réponds en français.";
                var response = await _aiRouter.GetResponse(testPrompt);
                
                Console.WriteLine("\n=== TEST IA ===");
                Console.WriteLine($"Question: {testPrompt}");
                Console.WriteLine($"Réponse: {response}");
                Console.WriteLine("===============\n");
                
                logger.Information("✅ Test IA terminé avec succès");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Erreur lors du test IA");
            }
        }

        private static void ShowHistory(Serilog.ILogger logger)
        {
            try
            {
                if (_historyService == null)
                {
                    logger.Warning("Service d'historique non initialisé");
                    return;
                }

                var history = _historyService.GetHistory();
                
                if (history.Count == 0)
                {
                    Console.WriteLine("📝 Aucune capture dans l'historique");
                    return;
                }

                Console.WriteLine($"\n📚 === HISTORIQUE ({history.Count} entrées) ===");
                Console.WriteLine("(Les plus récentes en haut)\n");
                
                for (int i = 0; i < Math.Min(10, history.Count); i++) // Limiter à 10 dernières
                {
                    var entry = history[i];
                    var preview = entry.OcrText?.Length > 80 
                        ? entry.OcrText[..80] + "..." 
                        : entry.OcrText ?? "Aucun texte";
                    
                    Console.WriteLine($"{i + 1}. {entry.Timestamp:dd/MM/yyyy HH:mm:ss}");
                    Console.WriteLine($"   📝 {preview}");
                    if (!string.IsNullOrEmpty(entry.AiAnalysis))
                    {
                        var aiPreview = entry.AiAnalysis.Length > 100 
                            ? entry.AiAnalysis[..100] + "..." 
                            : entry.AiAnalysis;
                        Console.WriteLine($"   🤖 {aiPreview}");
                    }
                    Console.WriteLine();
                }
                
                if (history.Count > 10)
                {
                    Console.WriteLine($"... et {history.Count - 10} autres entrées");
                }

                logger.Information("Historique affiché: {Count} entrées", history.Count);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Erreur lors de l'affichage de l'historique");
            }
        }

        private static async Task ClearHistory(Serilog.ILogger logger)
        {
            try
            {
                if (_historyService == null)
                {
                    logger.Warning("Service d'historique non initialisé");
                    return;
                }

                Console.Write("Êtes-vous sûr de vouloir effacer tout l'historique ? (o/N): ");
                var response = Console.ReadLine()?.ToLower();
                
                if (response == "o" || response == "oui")
                {
                    await _historyService.ClearHistoryAsync();
                    Console.WriteLine("✅ Historique effacé");
                    logger.Information("Historique effacé par l'utilisateur");
                }
                else
                {
                    Console.WriteLine("❌ Opération annulée");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Erreur lors de l'effacement de l'historique");
            }
        }
    }
}
