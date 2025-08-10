using System;
using Microsoft.Extensions.Configuration;
using AI;

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("Core/appsettings.json", optional: false)
    .Build();

var ollamaModel = config["AI:OllamaModel"] ?? "mistral:7b";
var ollamaEndpoint = config["AI:OllamaEndpoint"] ?? "http://localhost:11434";

Console.WriteLine($"🦙 Configuration Ollama détectée:");
Console.WriteLine($"   Endpoint: {ollamaEndpoint}");
Console.WriteLine($"   Modèle: {ollamaModel}");

var provider = new OllamaProvider(ollamaEndpoint, ollamaModel);
Console.WriteLine($"   Modèle configuré dans le provider: {provider.GetModel()}");

Console.WriteLine("\n🧪 Test rapide du modèle...");
try 
{
    var response = await provider.GetResponseAsync("Hello, which model are you?");
    Console.WriteLine($"✅ Réponse reçue: {response}");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Erreur: {ex.Message}");
}

Console.WriteLine("\nTest terminé. Appuyez sur une touche pour continuer...");
Console.ReadKey();
