@echo off
echo =============================================
echo LANCEMENT DE LA NOUVELLE APPLICATION BLAZOR
echo =============================================
echo.

cd /d "d:\Coding\VSCodeProject\Real-Time Screen and Voice Intelligence\WebApp\RealTimeIntelligenceApp"

echo [1] Vérification du projet...
if exist "RealTimeIntelligenceApp.csproj" (
    echo ✅ Projet trouvé
) else (
    echo ❌ Projet non trouvé
    pause
    exit /b 1
)

echo.
echo [2] Compilation...
dotnet build
if %ERRORLEVEL% neq 0 (
    echo ❌ Échec de la compilation
    pause
    exit /b 1
)

echo.
echo [3] Démarrage du serveur...
echo 🚀 Application disponible sur: http://localhost:5000
echo 📱 Interface: Real-Time Intelligence System
echo.
echo ⚠️  Appuyez sur Ctrl+C pour arrêter
echo.

start http://localhost:5000

dotnet run --urls "http://localhost:5000"
