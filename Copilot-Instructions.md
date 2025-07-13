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
Cv2.Cuda.SetDevice(0);
```

2. **Memory Management**:
```csharp
// Proper disposal of OpenCV objects
using (var mat = new Mat())
{
    // Processing code
}
```

3. **Adaptive Capture**:
```csharp
// Only analyze changed regions
var diff = new Mat();
Cv2.Absdiff(lastFrame, currentFrame, diff);
if (Cv2.CountNonZero(diff) > threshold)
{
    ProcessFrame(currentFrame);
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
