using System;
using System.Threading.Tasks;
using AI;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🧪 Test OpenRouter...");
        
        var provider = new OpenRouterProvider("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");
        
        try
        {
            var response = await provider.GetResponseAsync("Hello! Can you analyze this simple test message?");
            Console.WriteLine($"✅ OpenRouter response: {response}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
        
        provider.Dispose();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
