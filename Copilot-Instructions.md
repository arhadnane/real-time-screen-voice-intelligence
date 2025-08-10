# Copilot Instructions - Real-Time Screen & Voice Analysis System

## Project Overview
This C# project creates an intelligent assistant with:
- Real-time screen capture (OpenCV)
- Live voice transcription (free APIs)
- Multi-engine AI support (Ollama + free APIs)
- Combined visual/audio analysis

## Technical Architecture

### 1. Screen Capture with OpenCV
```csharp
// Using OpenCVSharp for efficient screen capture
using OpenCvSharp;

public Mat CaptureScreen()
{
    using (var screen = new ScreenCapture())
    {
        return screen.Capture();
    }
}

// Region of Interest detection
public Rect DetectROI(Mat image)
{
    using (var gray = new Mat())
    {
        Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
        Cv2.Threshold(gray, gray, 0, 255, ThresholdTypes.BinaryInv | ThresholdTypes.Otsu);
        Cv2.FindContours(gray, out var contours, out _, RetrievalModes.External);
        return Cv2.BoundingRect(contours.OrderByDescending(c => c.ContourArea()).First());
    }
}
```

### 2. Free OCR Options
**Option 1: Tesseract (Offline)**
```csharp
var engine = new TesseractEngine("./tessdata", "eng+fra", EngineMode.LstmOnly);
```

**Option 2: Free OCR API (Online)**
```csharp
public async Task<string> CallFreeOCRAPI(byte[] image)
{
    using var client = new HttpClient();
    var content = new MultipartFormDataContent();
    content.Add(new ByteArrayContent(image), "file", "screen.jpg");
    
    var response = await client.PostAsync("https://api.ocr.space/parse/image", content);
    return await response.Content.ReadAsStringAsync();
}
```

### 3. Free Speech-to-Text Options
**Option 1: VOSK (Offline)**
```csharp
using Vosk;

var model = new Model("vosk-model-en-us-0.22");
var recognizer = new VoskRecognizer(model, 16000.0f);
```

**Option 2: Web Speech API (Browser-based)**

### 4. AI Engine Options
**Option 1: Ollama (Local)**
```csharp
public async Task<string> QueryOllama(string prompt)
{
    using var client = new HttpClient();
    var request = new {
        model = "llama3",
        prompt = prompt,
        stream = false
    };
    
    var response = await client.PostAsJsonAsync("http://localhost:11434/api/generate", request);
    return await response.Content.ReadAsStringAsync();
}
```

**Option 2: Free AI APIs**
```csharp
// Hugging Face Inference API (Free tier)
public async Task<string> CallHuggingFace(string text)
{
    using var client = new HttpClient();
    client.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Bearer", "YOUR_HF_TOKEN");
    
    var response = await client.PostAsJsonAsync(
        "https://api-inference.huggingface.co/models/gpt2",
        new { inputs = text });
    
    return await response.Content.ReadAsStringAsync();
}
```

## Hybrid Processing Pipeline

1. **Initialization**:
```csharp
var screenAnalyzer = new OpenCvScreenAnalyzer();
var voiceEngine = new VoskSpeechRecognizer();
var aiRouter = new AIRouter(
    new OllamaProvider(),
    new HuggingFaceProvider()
);
```

2. **Processing Loop**:
```csharp
while (true)
{
    // Capture and analyze screen
    var screenText = screenAnalyzer.Analyze();
    
    // Get latest voice transcription
    var voiceText = voiceEngine.GetLatestTranscription();
    
    // Combine contexts
    var context = $"Screen: {screenText}\nVoice: {voiceText}";
    
    // Get AI response
    var response = await aiRouter.GetResponse(context);
    
    await Task.Delay(2000); // 2 second interval
}
```

## Configuration Guide

### `appsettings.json`
```json
{
  "AI": {
    "PrimaryEngine": "Ollama",
    "FallbackEngine": "HuggingFace",
    "OllamaEndpoint": "http://localhost:11434",
    "HF_Token": ""
  },
  "Vision": {
    "UseOpenCV": true,
    "CaptureFPS": 5,
    "ROIDetection": true
  },
  "Audio": {
    "Engine": "VOSK",
    "SampleRate": 16000
  }
}
```

## Performance Optimization

1. **OpenCV-Specific**:
```csharp
// Enable GPU acceleration
Cv2.SetUseOptimized(true);
if (Cv2.Cuda.GetCudaEnabledDeviceCount() > 0)
{
    Cv2.Cuda.SetDevice(0);
}
```

2. **Memory Management**:
```csharp
// Proper disposal of OpenCV objects with async disposal
using (var mat = new Mat())
{
    // Processing code
}

// Object pooling for frequent allocations
private readonly ObjectPool<Mat> _matPool = ObjectPool.Create<Mat>();
```

3. **Adaptive Capture**:
```csharp
// Only analyze changed regions with frame differencing
var diff = new Mat();
Cv2.Absdiff(lastFrame, currentFrame, diff);
Cv2.Threshold(diff, diff, 30, 255, ThresholdTypes.Binary);
var changePercent = Cv2.CountNonZero(diff) / (double)(diff.Width * diff.Height);
if (changePercent > 0.05) // 5% change threshold
{
    ProcessFrame(currentFrame);
}
```

4. **Async Processing**:
```csharp
// Parallel processing of vision and audio
var visionTask = Task.Run(() => ProcessVision());
var audioTask = Task.Run(() => ProcessAudio());
await Task.WhenAll(visionTask, audioTask);
```

5. **Caching and Rate Limiting**:
```csharp
// Cache AI responses to avoid redundant calls
private readonly MemoryCache _responseCache = new MemoryCache();
private readonly SemaphoreSlim _aiThrottle = new SemaphoreSlim(1, 1);
```

## Error Handling & Logging

### Structured Logging with Serilog
```csharp
// Program.cs
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
```

### Robust Error Handling
```csharp
public async Task<string> GetResponseWithRetry(string context, int maxRetries = 3)
{
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            var response = await _ollama.QueryOllama(context);
            Log.Information("Successfully got AI response on attempt {Attempt}", i + 1);
            return response;
        }
        catch (HttpRequestException ex)
        {
            Log.Warning("Network error on attempt {Attempt}: {Error}", i + 1, ex.Message);
            if (i == maxRetries - 1)
            {
                Log.Error("All AI provider attempts failed, using fallback");
                return await _hf.CallHuggingFace(context);
            }
            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i))); // Exponential backoff
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected error on attempt {Attempt}", i + 1);
            throw;
        }
    }
    return string.Empty;
}
```

### Circuit Breaker Pattern
```csharp
public class CircuitBreakerAIProvider
{
    private int _failureCount = 0;
    private DateTime _lastFailureTime = DateTime.MinValue;
    private readonly int _threshold = 5;
    private readonly TimeSpan _timeout = TimeSpan.FromMinutes(1);
    
    public async Task<string> GetResponse(string prompt)
    {
        if (_failureCount >= _threshold && 
            DateTime.UtcNow - _lastFailureTime < _timeout)
        {
            throw new InvalidOperationException("Circuit breaker is open");
        }
        
        try
        {
            var result = await CallAI(prompt);
            _failureCount = 0; // Reset on success
            return result;
        }
        catch
        {
            _failureCount++;
            _lastFailureTime = DateTime.UtcNow;
            throw;
        }
    }
}
```

## Best Practices & Code Quality

### 1. Resource Management
```csharp
// Always implement IDisposable for classes that hold unmanaged resources
public class VisionComponent : IDisposable
{
    private bool _disposed = false;
    
    public void Dispose()
    {
        if (!_disposed)
        {
            // Dispose managed resources
            _disposed = true;
        }
    }
}

// Use object pooling for frequently allocated objects
private readonly ObjectPool<Mat> _matPool = ObjectPool.Create<Mat>();
```

### 2. Async/Await Best Practices
```csharp
// Use ConfigureAwait(false) for library code
public async Task<string> ProcessAsync()
{
    var result = await SomeAsyncOperation().ConfigureAwait(false);
    return result;
}

// Use cancellation tokens for long-running operations
public async Task ProcessWithCancellation(CancellationToken cancellationToken)
{
    while (!cancellationToken.IsCancellationRequested)
    {
        await DoWork();
        await Task.Delay(1000, cancellationToken);
    }
}
```

### 3. Configuration Validation
```csharp
public class AIConfiguration
{
    public string OllamaEndpoint { get; set; }
    public string HF_Token { get; set; }
    
    public void Validate()
    {
        if (string.IsNullOrEmpty(OllamaEndpoint))
            throw new ArgumentException("OllamaEndpoint is required");
        
        if (!Uri.TryCreate(OllamaEndpoint, UriKind.Absolute, out _))
            throw new ArgumentException("OllamaEndpoint must be a valid URL");
    }
}
```

### 4. Monitoring & Metrics
```csharp
public class PerformanceMetrics
{
    private static readonly Counter ProcessedFrames = 
        Metrics.CreateCounter("processed_frames_total", "Total processed frames");
    
    private static readonly Histogram ProcessingDuration = 
        Metrics.CreateHistogram("processing_duration_seconds", "Processing duration");
    
    public static void RecordFrameProcessed()
    {
        ProcessedFrames.Inc();
    }
    
    public static IDisposable TimeOperation()
    {
        return ProcessingDuration.NewTimer();
    }
}
```

## Troubleshooting

**Common Issues:**
1. Ollama not running - `docker run -d -p 11434:11434 ollama/ollama`
2. OpenCV DLL missing - Install via NuGet: `OpenCvSharp4.runtime.win`
3. VOSK model download - Get models from https://alphacephei.com/vosk/models

---

This enhanced version includes:
1. OpenCV for efficient screen capture and processing
2. Multiple free options for each component (offline/online)
3. Ollama integration for local AI processing
4. Hybrid AI routing system
5. Detailed configuration examples
6. Performance optimization techniques

### Optimized Model Selection

**Recommended: Phi3 Mini**
```bash
# Pull the optimized model
ollama pull phi3:mini
```

**Model Comparison:**
| Model | Size | Use Case | Performance |
|-------|------|----------|-------------|
| `phi3:mini` | 2.2 GB | **RECOMMENDED** - Excellent general intelligence, great reasoning | ⭐⭐⭐⭐⭐ |
| `deepseek-coder:1.3b-instruct` | 776 MB | Ultra-light, code-focused analysis | ⭐⭐⭐⭐ |
| `tinyllama:latest` | 637 MB | Ultra-light, basic responses | ⭐⭐⭐ |

**Why Phi3 Mini:**
- ✅ Superior reasoning and comprehension abilities
- ✅ Excellent at following complex instructions
- ✅ Outstanding quality-to-size ratio (2.2 GB)
- ✅ Great for real-time analysis and decision making
- ✅ Better contextual understanding than smaller models

```csharp
// Optimized prompt structure for Phi3
private string OptimizePromptForPhi3(string context)
{
    return $@"You are an intelligent assistant analyzing screen content and voice input in real-time.

Current situation:
{context}

Please provide a brief analysis following this format:
**Analysis:** What do you observe from the screen and voice data?
**Recommendation:** What action should be taken?
**Priority:** High/Medium/Low

Keep your response concise and actionable (under 150 words).";
}
```
