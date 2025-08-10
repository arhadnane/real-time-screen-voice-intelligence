using System;
using System.Threading.Tasks;

namespace AI
{
    public interface IOllamaProvider : IDisposable
    {
        Task<string> GetResponseAsync(string prompt);
        string GetModel();
    }

    public interface IHuggingFaceProvider : IDisposable
    {
        Task<string> GetResponseAsync(string prompt);
        string GetToken();
    }

    public interface IOpenRouterProvider : IDisposable
    {
        Task<string> GetResponseAsync(string prompt);
        string GetApiKey();
    }

    public interface IAIRouter : IDisposable
    {
        Task<string> GetResponse(string context);
    }
}
