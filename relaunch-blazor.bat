@echo off
echo =============================================
echo RELANCEMENT DE L'APPLICATION BLAZOR
echo =============================================

cd /d "d:\Coding\VSCodeProject\Real-Time Screen and Voice Intelligence\WebApp\RealTimeIntelligenceApp"

echo [INFO] Compilation du projet...
dotnet build
if %ERRORLEVEL% neq 0 (
    echo [ERREUR] Échec de la compilation
    pause
    exit /b 1
)

echo [INFO] Ouverture du navigateur...
start http://localhost:5000

echo [INFO] Démarrage du serveur Blazor...
echo URL: http://localhost:5000
echo Appuyez sur Ctrl+C pour arrêter

dotnet run --urls "http://localhost:5000"
