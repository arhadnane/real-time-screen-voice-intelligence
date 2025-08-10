@echo off
echo =================================================
echo  Real-Time Intelligence - Nettoyage du projet
echo =================================================
echo.

echo 🧹 Suppression des fichiers de compilation...

REM Supprimer les dossiers bin et obj
for /d /r . %%d in (bin,obj) do @if exist "%%d" rd /s /q "%%d" && echo   ✅ Supprimé: %%d

REM Supprimer les fichiers temporaires
del /s /q *.tmp >nul 2>&1
del /s /q *.log >nul 2>&1
del /s /q startup_log.txt >nul 2>&1

REM Supprimer les anciens projets de test
if exist "Core" (
    echo   🗑️  Suppression de l'ancien projet Core...
    rd /s /q "Core"
)

if exist "TempTest" (
    echo   🗑️  Suppression de l'ancien projet TempTest...
    rd /s /q "TempTest"
)

if exist "Tests" (
    echo   🗑️  Suppression des anciens tests...
    rd /s /q "Tests"
)

echo.
echo ✅ Nettoyage terminé !
echo 📁 Le projet est maintenant propre et prêt à être utilisé.
echo.
echo 🚀 Pour lancer l'application, exécutez: launch-blazor-simple.bat
echo.

pause
