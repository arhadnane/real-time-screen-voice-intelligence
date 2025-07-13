using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net.Http.Json;

namespace AI
{
    public class OllamaProvider
    {
        private readonly string _endpoint;
        public OllamaProvider(string endpoint)
        {
            _endpoint = endpoint;
        }

        public async Task<string> QueryOllama(string prompt)
        {
            using var client = new HttpClient();
            var request = new {
                model = "llama3",
                prompt = prompt,
                stream = false
            };
            var response = await client.PostAsJsonAsync(_endpoint + "/api/generate", request);
            return await response.Content.ReadAsStringAsync();
        }
    }

    public class HuggingFaceProvider
    {
        private readonly string _token;
        public HuggingFaceProvider(string token)
        {
            _token = token;
        }

        public async Task<string> CallHuggingFace(string text)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            var response = await client.PostAsJsonAsync(
                "https://api-inference.huggingface.co/models/gpt2",
                new { inputs = text });
            return await response.Content.ReadAsStringAsync();
        }
    }

    public class AIRouter
    {
        private readonly OllamaProvider _ollama;
        private readonly HuggingFaceProvider _hf;
        public AIRouter(OllamaProvider ollama, HuggingFaceProvider hf)
        {
            _ollama = ollama;
            _hf = hf;
        }

        public async Task<string> GetResponse(string context)
        {
            // Try Ollama first, fallback to HuggingFace
            try
            {
                return await _ollama.QueryOllama(context);
            }
            catch
            {
                return await _hf.CallHuggingFace(context);
            }
        }
    }
}
