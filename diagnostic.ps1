Write-Host "=== DIAGNOSTIC DE L'APPLICATION ===" -ForegroundColor Cyan

# Vérifier si le port 5000 est utilisé
Write-Host "`n[1] Vérification du port 5000..." -ForegroundColor Yellow
try {
    $tcpConnection = Get-NetTCPConnection -LocalPort 5000 -State Listen -ErrorAction SilentlyContinue
    if ($tcpConnection) {
        Write-Host "✅ Port 5000 en écoute - Serveur probablement actif" -ForegroundColor Green
        Write-Host "   Process ID: $($tcpConnection.OwningProcess)" -ForegroundColor Cyan
    } else {
        Write-Host "❌ Port 5000 libre - Aucun serveur en cours" -ForegroundColor Red
    }
} catch {
    Write-Host "⚠️  Impossible de vérifier le port" -ForegroundColor Yellow
}

# Vérifier l'existence du projet
Write-Host "`n[2] Vérification du projet..." -ForegroundColor Yellow
$projectPath = "d:\Coding\VSCodeProject\Real-Time Screen and Voice Intelligence\WebApp\RealTimeIntelligenceApp\RealTimeIntelligenceApp.csproj"
if (Test-Path $projectPath) {
    Write-Host "✅ Projet trouvé: $projectPath" -ForegroundColor Green
} else {
    Write-Host "❌ Projet non trouvé: $projectPath" -ForegroundColor Red
}

# Test de connectivité
Write-Host "`n[3] Test de connectivité..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000" -Method Head -TimeoutSec 5 -ErrorAction SilentlyContinue
    if ($response.StatusCode -eq 200) {
        Write-Host "✅ Application accessible sur http://localhost:5000" -ForegroundColor Green
    } else {
        Write-Host "⚠️  Réponse inattendue: $($response.StatusCode)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ Application non accessible sur http://localhost:5000" -ForegroundColor Red
    Write-Host "   Erreur: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== FIN DU DIAGNOSTIC ===" -ForegroundColor Cyan
