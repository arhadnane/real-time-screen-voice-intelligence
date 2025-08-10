@echo off
echo 🚀 Setting up Phi3 Mini model for Real-Time Screen & Voice Intelligence...
echo.

echo 📥 Checking if Ollama is running...
curl -s http://localhost:11434/api/tags >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ Ollama is not running! Please start Ollama first.
    echo    Download from: https://ollama.ai/download
    echo    Then run: ollama serve
    pause
    exit /b 1
)

echo ✅ Ollama is running!
echo.

echo 📦 Pulling Phi3 Mini model (2.2 GB)...
echo    This model offers superior reasoning and comprehension abilities.
echo    Download may take a few minutes depending on your internet connection...
ollama pull phi3:mini

if %errorlevel% equ 0 (
    echo.
    echo ✅ Model downloaded successfully!
    echo.
    echo 🧪 Testing the model...
    echo.
    echo | ollama run phi3:mini "Hello, can you help me analyze screen content and voice data?"
    echo.
    echo 🎉 Setup complete! Your Real-Time Screen & Voice Intelligence system is ready to use.
    echo.
    echo 📋 Next steps:
    echo    1. Make sure you have Tesseract data files in ./tessdata/
    echo    2. Download VOSK model if you want voice recognition
    echo    3. Run the application: dotnet run --project Core
    echo.
) else (
    echo ❌ Failed to download the model. Please check your internet connection.
)

pause
