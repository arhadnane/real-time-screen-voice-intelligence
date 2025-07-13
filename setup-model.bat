@echo off
echo 🚀 Setting up DeepSeek Coder model for Real-Time Screen & Voice Intelligence...
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

echo 📦 Pulling DeepSeek Coder 1.3B Instruct model (776 MB)...
echo    This may take a few minutes depending on your internet connection...
ollama pull deepseek-coder:1.3b-instruct

if %errorlevel% equ 0 (
    echo.
    echo ✅ Model downloaded successfully!
    echo.
    echo 🧪 Testing the model...
    echo.
    echo | ollama run deepseek-coder:1.3b-instruct "Hello, can you help me analyze screen content?"
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
