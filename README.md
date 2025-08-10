# Real-Time Screen & Voice Intelligence System

An intelligent C# application that combines real-time screen capture, voice transcription, and AI analysis to provide contextual assistance.

## Features

- **Real-time Screen Capture**: Using OpenCV for efficient screen analysis
- **Voice Transcription**: Offline speech-to-text with VOSK
- **OCR Processing**: Text extraction from screen content using Tesseract
- **AI Integration**: Multi-engine AI support (Ollama + Hugging Face APIs)
- **Hybrid Analysis**: Combined visual and audio context processing

## Architecture

### Components
- **Vision Module**: Screen capture and OCR processing
- **Audio Module**: Speech recognition and transcription
- **AI Module**: AI routing and response generation
- **Core Module**: Main application loop and configuration

### Technology Stack
- .NET 7.0 (Windows)
- OpenCvSharp4 for computer vision
- Tesseract for OCR
- VOSK for speech recognition
- NAudio for audio processing

## Prerequisites

1. .NET 7.0 SDK or later  
2. Visual Studio 2022 or VS Code
3. Windows 10/11 (required for Windows Forms screen capture)
4. [Ollama](https://ollama.ai/download) - Local AI inference server

## Setup Instructions

### 1. Clone the Repository
```bash
git clone <repository-url>
cd "Real-Time Screen and Voice Intelligence"
```

### 2. AI Model Setup (Enhanced Performance!)
We use **Phi3 Mini** - an outstanding model with superior reasoning abilities:

```bash
# Option 1: Automatic setup (Recommended)
./setup-model.bat
# or for PowerShell users
./setup-model.ps1

# Option 2: Manual setup
ollama pull phi3:mini
```

**Why Phi3 Mini?**
- ✅ Superior reasoning and comprehension: Best-in-class intelligence
- ✅ Excellent instruction following and contextual understanding
- ✅ Outstanding quality-to-size ratio at 2.2 GB
- ✅ Perfect for real-time analysis and complex decision making
- ✅ Much better performance than ultra-light models
- ✅ Developed by Microsoft with cutting-edge architecture

### 3. Download Required Data Files

**Tesseract Language Data:**
- Download `eng.traineddata` and `fra.traineddata` from [tesseract-ocr/tessdata](https://github.com/tesseract-ocr/tessdata)
- Create a `tessdata` folder in the project root
- Place the `.traineddata` files in the `tessdata` folder

**VOSK Speech Model:**
- Download a VOSK model (e.g., `vosk-model-en-us-0.22`) from [alphacephei.com/vosk/models](https://alphacephei.com/vosk/models)
- Extract the model to the project root directory

### 3. Configuration

Edit `Core/appsettings.json` to configure:
- Ollama endpoint (if using local AI)
- Hugging Face API token (if using HF API)
- Audio and vision processing settings

### 4. Build and Run

```bash
dotnet build .\Core\Core.csproj
dotnet run --project .\Core
```

## Configuration Options

### AI Settings
- `PrimaryEngine`: Main AI provider (Ollama/HuggingFace)
- `FallbackEngine`: Backup AI provider
- `OllamaEndpoint`: Local Ollama server URL
- `HF_Token`: Hugging Face API token

### Vision Settings
- `UseOpenCV`: Enable OpenCV processing
- `CaptureFPS`: Screen capture frequency
- `ROIDetection`: Region of interest detection

### Audio Settings
- `Engine`: Speech recognition engine (VOSK)
- `SampleRate`: Audio sampling rate

## Usage

1. Launch the application
2. The system will start capturing screen content and listening for voice input
3. AI responses will be displayed in the console based on the combined context
4. The application runs continuously with a 2-second processing interval

## Troubleshooting

### Common Issues

1. **Missing Tesseract Data**: Ensure `tessdata` folder contains required language files
2. **VOSK Model Error**: Verify the VOSK model path and extraction
3. **Ollama Connection**: Check if Ollama server is running on the specified endpoint
4. **OpenCV Runtime**: Ensure OpenCvSharp4.runtime.win is properly installed

### Performance Optimization

- Enable GPU acceleration for OpenCV if available
- Adjust capture FPS based on system performance
- Use adaptive frame processing to reduce computational load

## API Integration

### Ollama (Local AI)
```csharp
var ollama = new OllamaProvider("http://localhost:11434");
var response = await ollama.QueryOllama("your prompt");
```

### Hugging Face (Remote AI)
```csharp
var hf = new HuggingFaceProvider("your-token");
var response = await hf.CallHuggingFace("your input");
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- OpenCvSharp for computer vision capabilities
- Tesseract OCR for text recognition
- VOSK for offline speech recognition
- Ollama for local AI processing
