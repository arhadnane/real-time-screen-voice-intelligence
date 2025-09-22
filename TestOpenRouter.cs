using System;
using System.Threading.Tasks;
using AI;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🧪 Test OpenRouter...");
        
        // Replace with your actual OpenRouter API key
        var provider = new OpenRouterProvider("YOUR_OPENROUTER_API_KEY_HERE");
        
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
