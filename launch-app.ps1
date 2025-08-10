# Script PowerShell pour lancer l'application Real-Time Intelligence
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "LANCEMENT DE L'APPLICATION BLAZOR" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

# Navigation vers le dossier du projet
Set-Location "d:\Coding\VSCodeProject\Real-Time Screen and Voice Intelligence\WebApp\RealTimeIntelligenceApp"

Write-Host "[1] Vérification du projet..." -ForegroundColor Yellow
if (Test-Path "RealTimeIntelligenceApp.csproj") {
    Write-Host "✅ Projet trouvé" -ForegroundColor Green
} else {
    Write-Host "❌ Projet non trouvé" -ForegroundColor Red
    Read-Host "Appuyez sur Entrée pour continuer..."
    exit 1
}

Write-Host ""
Write-Host "[2] Compilation..." -ForegroundColor Yellow
dotnet build
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Échec de la compilation" -ForegroundColor Red
    Read-Host "Appuyez sur Entrée pour continuer..."
    exit 1
}

Write-Host ""
Write-Host "[3] Démarrage du serveur..." -ForegroundColor Yellow
Write-Host "🚀 Application disponible sur: http://localhost:5000" -ForegroundColor Green
Write-Host "📱 Interface: Real-Time Intelligence System" -ForegroundColor Green
Write-Host ""
Write-Host "⚠️  Appuyez sur Ctrl+C pour arrêter" -ForegroundColor Yellow
Write-Host ""

# Ouverture du navigateur
Start-Process "http://localhost:5000"

# Démarrage du serveur
dotnet run --urls "http://localhost:5000"
