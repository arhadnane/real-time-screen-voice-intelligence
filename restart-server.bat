@echo off
echo ===========================================
echo REDEMARRAGE ET DIAGNOSTIC DU SERVEUR WEB
echo ===========================================

cd /d "d:\Coding\VSCodeProject\Real-Time Screen and Voice Intelligence\RealTimeIntelligence.Web"

echo [1] Nettoyage des artefacts...
dotnet clean

echo [2] Reconstruction...
dotnet build
if %ERRORLEVEL% neq 0 (
    echo ❌ Échec de la construction
    pause
    exit /b 1
)

echo [3] Vérification des pages disponibles...
echo    - http://localhost:5000/ (Page principale - HomeBasic)
echo    - http://localhost:5000/minimal (Test minimal)
echo    - http://localhost:5000/test (Test HTML)
echo    - http://localhost:5000/home-advanced (Version avancée)

echo.
echo [4] Démarrage du serveur...
echo 🚀 Serveur démarré sur http://localhost:5000
echo.

start http://localhost:5000

dotnet run --urls "http://localhost:5000"
