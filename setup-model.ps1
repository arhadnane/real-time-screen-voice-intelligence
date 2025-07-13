# Setup DeepSeek Coder model for Real-Time Screen & Voice Intelligence
Write-Host "🚀 Setting up DeepSeek Coder model..." -ForegroundColor Green
Write-Host ""

# Check if Ollama is running
Write-Host "📥 Checking if Ollama is running..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:11434/api/tags" -TimeoutSec 5 -ErrorAction Stop
    Write-Host "✅ Ollama is running!" -ForegroundColor Green
} catch {
    Write-Host "❌ Ollama is not running! Please start Ollama first." -ForegroundColor Red
    Write-Host "   Download from: https://ollama.ai/download" -ForegroundColor Yellow
    Write-Host "   Then run: ollama serve" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host ""
Write-Host "📦 Pulling DeepSeek Coder 1.3B Instruct model (776 MB)..." -ForegroundColor Yellow
Write-Host "   This may take a few minutes depending on your internet connection..." -ForegroundColor Gray

# Pull the model
$pullProcess = Start-Process -FilePath "ollama" -ArgumentList "pull", "deepseek-coder:1.3b-instruct" -Wait -PassThru

if ($pullProcess.ExitCode -eq 0) {
    Write-Host ""
    Write-Host "✅ Model downloaded successfully!" -ForegroundColor Green
    Write-Host ""
    
    Write-Host "🧪 Testing the model..." -ForegroundColor Yellow
    Write-Host ""
    
    # Test the model
    $testProcess = Start-Process -FilePath "ollama" -ArgumentList "run", "deepseek-coder:1.3b-instruct", "Hello, can you help me analyze screen content?" -Wait -PassThru
    
    Write-Host ""
    Write-Host "🎉 Setup complete! Your Real-Time Screen & Voice Intelligence system is ready to use." -ForegroundColor Green
    Write-Host ""
    Write-Host "📋 Next steps:" -ForegroundColor Cyan
    Write-Host "   1. Make sure you have Tesseract data files in ./tessdata/" -ForegroundColor White
    Write-Host "   2. Download VOSK model if you want voice recognition" -ForegroundColor White
    Write-Host "   3. Run the application: dotnet run --project Core" -ForegroundColor White
    Write-Host ""
} else {
    Write-Host "❌ Failed to download the model. Please check your internet connection." -ForegroundColor Red
}

Read-Host "Press Enter to exit"
