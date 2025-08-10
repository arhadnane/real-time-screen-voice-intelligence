using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System.Threading;

namespace AI
{
    public class OllamaProvider : IDisposable
    {
        private readonly string _endpoint;
        private readonly HttpClient _httpClient;
        private readonly SemaphoreSlim _rateLimiter;

        public OllamaProvider(string endpoint)
        {
            _endpoint = endpoint;
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
            _rateLimiter = new SemaphoreSlim(3, 3); // Max 3 concurrent requests
        }

        private string OptimizePromptForDeepSeekCoder(string context)
        {
            // DeepSeek Coder responds better to structured, clear instructions
            var optimizedPrompt = $@"# Screen & Voice Analysis Task

## Context:
{context}

## Instructions:
Analyze the provided screen content and voice input. Provide a concise, actionable response in this format:

**Analysis:** [Brief summary of what you observe]
**Action:** [Suggested action or response]
**Priority:** [High/Medium/Low]

Keep response under 200 words. Focus on the most important insights.";

            return optimizedPrompt;
        }

        public async Task<string> QueryOllama(string prompt)
        {
            await _rateLimiter.WaitAsync();
            try
            {
                // Optimize prompt for DeepSeek Coder
                var optimizedPrompt = OptimizePromptForDeepSeekCoder(prompt);
                
                var request = new {
                    model = "phi3:mini",
                    prompt = optimizedPrompt,
                    stream = false,
                    options = new {
                        temperature = 0.3,  // Lower for more focused responses
                        max_tokens = 300,   // Reduced for faster responses
                        top_p = 0.9,
                        num_predict = 300
                    }
                };
                
                var response = await _httpClient.PostAsJsonAsync(_endpoint + "/api/generate", request);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            finally
            {
                _rateLimiter.Release();
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
            _rateLimiter?.Dispose();
        }
    }

    public class HuggingFaceProvider : IDisposable
    {
        private readonly string _token;
        private readonly HttpClient _httpClient;
        private readonly SemaphoreSlim _rateLimiter;

        public HuggingFaceProvider(string token)
        {
            _token = token;
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            _rateLimiter = new SemaphoreSlim(2, 2); // Max 2 concurrent requests for free tier
        }

        public async Task<string> CallHuggingFace(string text)
        {
            await _rateLimiter.WaitAsync();
            try
            {
                var requestData = new { 
                    inputs = text,
                    parameters = new {
                        max_length = 500,
                        temperature = 0.7,
                        do_sample = true
                    }
                };
                
                var response = await _httpClient.PostAsJsonAsync(
                    "https://api-inference.huggingface.co/models/gpt2",
                    requestData);
                
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            finally
            {
                _rateLimiter.Release();
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
            _rateLimiter?.Dispose();
        }
    }

    public class AIRouter : IDisposable
    {
        private readonly OllamaProvider _ollama;
        private readonly HuggingFaceProvider _hf;
        private int _ollamaFailureCount = 0;
        private DateTime _lastOllamaFailure = DateTime.MinValue;
        private readonly TimeSpan _circuitBreakerTimeout = TimeSpan.FromMinutes(5);
        private readonly int _maxFailures = 3;

        public AIRouter(OllamaProvider ollama, HuggingFaceProvider hf)
        {
            _ollama = ollama;
            _hf = hf;
        }

        public async Task<string> GetResponse(string context)
        {
            // Try Ollama only (no fallback to avoid HF 401 errors)
            try
            {
                var result = await _ollama.QueryOllama(context);
                Console.WriteLine("✅ Response from Ollama");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ollama failed: {ex.Message}");
                return $"[AI temporarily unavailable: {ex.Message}]";
            }
        }

        public void Dispose()
        {
            _ollama?.Dispose();
            _hf?.Dispose();
        }
    }
}
