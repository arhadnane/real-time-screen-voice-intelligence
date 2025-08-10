@echo off
echo ========================================
echo    MIGRATION VERS BLAZOR SERVER
echo ========================================
echo.

echo 1. Compilation du nouveau projet Blazor...
cd "RealTimeIntelligence.Web"
dotnet restore
if %ERRORLEVEL% neq 0 (
    echo Erreur lors de la restauration des packages
    pause
    exit /b 1
)

dotnet build
if %ERRORLEVEL% neq 0 (
    echo Erreur lors de la compilation
    pause
    exit /b 1
)

echo.
echo 2. Copie des ressources necessaires...

:: Copier tessdata
if exist "..\tessdata" (
    xcopy "..\tessdata" "tessdata" /E /I /Y
    echo - tessdata copie
) else (
    echo - tessdata non trouve, creez le dossier et ajoutez eng.traineddata et fra.traineddata
)

:: Copier vosk-models
if exist "..\vosk-models" (
    xcopy "..\vosk-models" "vosk-models" /E /I /Y
    echo - vosk-models copie
) else (
    echo - vosk-models non trouve, telechargez le modele francais
)

echo.
echo 3. Lancement de l'application Blazor...
echo.
echo L'application sera disponible sur: https://localhost:7001
echo Appuyez sur Ctrl+C pour arreter
echo.
dotnet run

pause
