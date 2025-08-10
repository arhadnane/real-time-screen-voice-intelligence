@echo off
echo ==========================================
echo LANCEMENT DU PROJET WEB BLAZOR
echo ==========================================
echo.

echo [1] Compilation du projet web...
dotnet build RealTimeIntelligence.Web
if %ERRORLEVEL% neq 0 (
    echo ❌ Échec de la compilation
    pause
    exit /b 1
)

echo.
echo [2] Démarrage du serveur web...
echo 🌐 Ouverture de l'application web sur :
echo    - HTTP:  http://localhost:5000
echo    - HTTPS: https://localhost:5001
echo.
echo 📝 Appuyez sur Ctrl+C pour arrêter le serveur
echo.

cd RealTimeIntelligence.Web
dotnet run --urls "http://localhost:5000;https://localhost:5001"
