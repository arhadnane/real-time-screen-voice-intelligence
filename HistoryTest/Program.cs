using Microsoft.Extensions.Configuration;
using Serilog;
using Core.Services;

Console.WriteLine("🚀 === TEST HISTORIQUE CAPTURE ===");

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .Build();

    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(configuration)
        .CreateLogger();

    var logger = Log.ForContext("Program", "HistoryTest");
    
    // Initialisation service d'historique
    var historyService = new CaptureHistoryService(logger);
    await historyService.LoadHistoryAsync();

    // Test: Ajouter quelques entrées de test si l'historique est vide
    if (historyService.GetHistory().Count == 0)
    {
        Console.WriteLine("📝 Ajout d'entrées de test à l'historique...");
        
        await historyService.AddCaptureAsync(
            "Premier test de capture d'écran avec du texte OCR détecté",
            "Analyse IA: Ce texte semble provenir d'un test de l'application. Le contenu indique une interface utilisateur basique.",
            "Test Window 1",
            1920, 1080
        );

        await Task.Delay(1000);

        await historyService.AddCaptureAsync(
            "Deuxième capture avec un texte différent pour montrer l'historique en action",
            "Analyse IA: Cette capture montre l'évolution du système. Le système d'historique fonctionne correctement.",
            "Test Window 2",
            1366, 768
        );

        await Task.Delay(1000);

        await historyService.AddCaptureAsync(
            "Troisième test - texte plus long pour voir comment l'affichage gère les textes étendus avec beaucoup de contenu textuel",
            "Analyse IA: Cette capture plus longue démontre la capacité du système à gérer différents types de contenu et à maintenir un historique cohérent des analyses précédentes. Le système semble robuste.",
            "Test Window 3",
            2560, 1440
        );
        
        Console.WriteLine("✅ Entrées de test ajoutées!");
    }

    bool running = true;
    Console.WriteLine("\n📋 Commandes disponibles:");
    Console.WriteLine("  'h' - Afficher l'historique");
    Console.WriteLine("  'c' - Effacer l'historique");
    Console.WriteLine("  'a' - Ajouter une entrée de test");
    Console.WriteLine("  'q' - Quitter");
    Console.WriteLine();

    while (running)
    {
        Console.Write("Commande (h/c/a/q): ");
        var input = Console.ReadLine()?.ToLower();

        switch (input)
        {
            case "h":
                ShowHistory(historyService, logger);
                break;
            case "c":
                await ClearHistory(historyService, logger);
                break;
            case "a":
                await AddTestEntry(historyService, logger);
                break;
            case "q":
                running = false;
                break;
            default:
                Console.WriteLine("Commande non reconnue. Utilisez 'h', 'c', 'a' ou 'q'.");
                break;
        }

        Console.WriteLine();
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Erreur: {ex.Message}");
    Log.Fatal(ex, "Erreur fatale");
}
finally
{
    await Log.CloseAndFlushAsync();
}

Console.WriteLine("Test terminé. Appuyez sur une touche pour quitter...");
Console.ReadKey();

static void ShowHistory(CaptureHistoryService historyService, Serilog.ILogger logger)
{
    try
    {
        var history = historyService.GetHistory();
        
        if (history.Count == 0)
        {
            Console.WriteLine("📝 Aucune capture dans l'historique");
            return;
        }

        Console.WriteLine($"\n📚 === HISTORIQUE ({history.Count} entrées) ===");
        Console.WriteLine("(Les plus récentes en haut - nouveaux ajouts apparaissent en premier)\n");
        
        for (int i = 0; i < history.Count; i++)
        {
            var entry = history[i];
            var preview = entry.OcrText?.Length > 80 
                ? entry.OcrText[..80] + "..." 
                : entry.OcrText ?? "Aucun texte";
            
            Console.WriteLine($"═══ ENTRÉE {i + 1} ═══");
            Console.WriteLine($"📅 Date: {entry.Timestamp:dd/MM/yyyy HH:mm:ss}");
            Console.WriteLine($"🪟 Fenêtre: {entry.WindowTitle ?? "Inconnue"} ({entry.Width}x{entry.Height})");
            Console.WriteLine($"📝 TEXTE OCR: {preview}");
            if (!string.IsNullOrEmpty(entry.AiAnalysis))
            {
                var aiPreview = entry.AiAnalysis.Length > 150 
                    ? entry.AiAnalysis[..150] + "..." 
                    : entry.AiAnalysis;
                Console.WriteLine($"🤖 ANALYSE IA: {aiPreview}");
            }
            Console.WriteLine();
        }

        logger.Information("Historique affiché: {Count} entrées", history.Count);
    }
    catch (Exception ex)
    {
        logger.Error(ex, "Erreur lors de l'affichage de l'historique");
    }
}

static async Task ClearHistory(CaptureHistoryService historyService, Serilog.ILogger logger)
{
    try
    {
        Console.Write("Êtes-vous sûr de vouloir effacer tout l'historique ? (o/N): ");
        var response = Console.ReadLine()?.ToLower();
        
        if (response == "o" || response == "oui")
        {
            await historyService.ClearHistoryAsync();
            Console.WriteLine("✅ Historique effacé avec succès");
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

static async Task AddTestEntry(CaptureHistoryService historyService, Serilog.ILogger logger)
{
    try
    {
        var random = new Random();
        var testTexts = new[]
        {
            "Nouvelle entrée de test ajoutée manuellement",
            "Test de capture avec un contenu différent pour vérifier l'ordre chronologique",
            "Entrée de test pour démontrer l'ajout dynamique à l'historique",
            "Capture de test - vérification du tri par date (nouveaux en haut)",
            "Test d'une entrée avec plus de contenu pour voir comment le système gère les textes longs et détaillés"
        };

        var testAnalyses = new[]
        {
            "Analyse IA: Cette entrée a été ajoutée pour tester l'interface utilisateur et vérifier que les nouveaux éléments apparaissent bien en haut.",
            "Analyse IA: Test de la fonctionnalité d'ajout manuel d'entrées. Le système maintient bien l'ordre chronologique inversé.",
            "Analyse IA: Démonstration du système d'historique en temps réel. Les captures sont bien sauvegardées et organisées.",
            "Analyse IA: Vérification du bon fonctionnement du tri chronologique avec les nouveaux éléments en première position.",
            "Analyse IA: Test approfondi du système de sauvegarde. L'historique conserve bien toutes les informations importantes."
        };

        var text = testTexts[random.Next(testTexts.Length)];
        var analysis = testAnalyses[random.Next(testAnalyses.Length)];

        await historyService.AddCaptureAsync(
            ocrText: text,
            aiAnalysis: analysis,
            windowTitle: $"Test Entry {DateTime.Now:HH:mm:ss}",
            width: 1920 + random.Next(0, 640),
            height: 1080 + random.Next(0, 360)
        );

        Console.WriteLine("✅ Nouvelle entrée de test ajoutée à l'historique (elle devrait apparaître en haut)");
        logger.Information("Entrée de test ajoutée à l'historique");
    }
    catch (Exception ex)
    {
        logger.Error(ex, "Erreur lors de l'ajout d'une entrée de test");
    }
}
