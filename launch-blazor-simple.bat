@echo off
echo =================================================
echo  Real-Time Intelligence - Application Blazor
echo =================================================
echo.

cd "RealTimeIntelligence.Web" 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo Erreur: Le dossier RealTimeIntelligence.Web est introuvable.
    pause
    exit /b 1
)

echo Restoration des packages...
dotnet restore >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo Erreur lors de la restauration des packages.
    pause
    exit /b 1
)

echo Construction du projet...
dotnet build >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo Erreur lors de la construction du projet.
    pause
    exit /b 1
)

echo.
echo ✅ Projet compilé avec succès !
echo 🌐 Démarrage de l'application...
echo.
echo 📍 URL: https://localhost:7001
echo 🔧 Appuyez sur Ctrl+C pour arrêter l'application
echo.

start https://localhost:7001
dotnet run --urls "https://localhost:7001"
