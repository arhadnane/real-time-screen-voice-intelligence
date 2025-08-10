# Script PowerShell pour lancer le projet web
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "LANCEMENT DU PROJET WEB BLAZOR" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "[1] Compilation du projet web..." -ForegroundColor Yellow
$buildResult = & dotnet build RealTimeIntelligence.Web
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Échec de la compilation" -ForegroundColor Red
    Read-Host "Appuyez sur Entrée pour quitter"
    exit 1
}

Write-Host ""
Write-Host "[2] Démarrage du serveur web..." -ForegroundColor Yellow
Write-Host "🌐 Ouverture de l'application web sur :" -ForegroundColor Green
Write-Host "   - HTTP:  http://localhost:5000" -ForegroundColor White
Write-Host "   - HTTPS: https://localhost:5001" -ForegroundColor White
Write-Host ""
Write-Host "📝 Appuyez sur Ctrl+C pour arrêter le serveur" -ForegroundColor Gray
Write-Host ""

# Lancer le navigateur après un délai
Start-Job -ScriptBlock {
    Start-Sleep 3
    Start-Process "http://localhost:5000"
} | Out-Null

Set-Location RealTimeIntelligence.Web
& dotnet run --urls "http://localhost:5000;https://localhost:5001"
