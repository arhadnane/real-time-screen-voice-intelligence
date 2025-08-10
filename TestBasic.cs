using System;

class TestBasic
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== TEST BASIQUE ===");
        
        try
        {
            Console.WriteLine("1. Test de base...");
            
            Console.WriteLine("2. Test OpenCV...");
            var analyzer = new Vision.OpenCvScreenAnalyzer();
            Console.WriteLine("   ✅ OpenCvScreenAnalyzer créé");
            
            Console.WriteLine("3. Test capture...");
            using (var mat = analyzer.CaptureScreen())
            {
                Console.WriteLine($"   ✅ Capture: {mat.Width}x{mat.Height}");
            }
            
            analyzer.Dispose();
            Console.WriteLine("✅ SUCCÈS");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERREUR: {ex.Message}");
            Console.WriteLine($"Type: {ex.GetType().Name}");
            Console.WriteLine($"Stack: {ex.StackTrace}");
        }
        
        Console.WriteLine("Appuyez sur une touche...");
        Console.ReadKey();
    }
}
