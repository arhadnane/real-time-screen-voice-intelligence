@echo off
cd /d "d:\Coding\VSCodeProject\Real-Time Screen and Voice Intelligence\RealTimeIntelligence.Web"
echo ===========================================
echo DIAGNOSTIC DE L'APPLICATION WEB
echo ===========================================
echo.

echo [1] Verification des fichiers...
if exist "Program.cs" (echo ✅ Program.cs trouvé) else (echo ❌ Program.cs manquant)
if exist "Components\Pages\Home.razor" (echo ✅ Home.razor trouvé) else (echo ❌ Home.razor manquant)
if exist "Pages\_Host.cshtml" (echo ✅ _Host.cshtml trouvé) else (echo ❌ _Host.cshtml manquant)

echo.
echo [2] Compilation du projet...
dotnet build
if %ERRORLEVEL% neq 0 (
    echo ❌ Échec de la compilation
    pause
    exit /b 1
)

echo.
echo [3] Lancement du serveur web...
echo 🚀 URL: http://localhost:5000
echo 🔗 URL HTTPS: https://localhost:5001
echo.
echo ⚠️  Appuyez sur Ctrl+C pour arrêter le serveur
echo.

start http://localhost:5000/test

dotnet run --urls "http://localhost:5000;https://localhost:5001"
