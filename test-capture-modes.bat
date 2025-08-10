@echo off
echo 🎯 === TEST SYSTÈME DE CAPTURE D'ÉCRAN ===
echo.

echo 📋 Test 1: Mode plein écran...
powershell -Command "& { (Get-Content 'Core\appsettings.json') -replace '\"CaptureMode\": \".*\"', '\"CaptureMode\": \"FullScreen\"' | Set-Content 'Core\appsettings.json' }"
timeout /t 2 >nul

echo 📋 Test 2: Mode région personnalisée...
powershell -Command "& { (Get-Content 'Core\appsettings.json') -replace '\"CaptureMode\": \".*\"', '\"CaptureMode\": \"CustomRegion\"' | Set-Content 'Core\appsettings.json' }"
timeout /t 2 >nul

echo 📋 Test 3: Mode multi-écrans...
powershell -Command "& { (Get-Content 'Core\appsettings.json') -replace '\"CaptureMode\": \".*\"', '\"CaptureMode\": \"AllScreens\"' | Set-Content 'Core\appsettings.json' }"
timeout /t 2 >nul

echo.
echo ✅ Tests de configuration terminés
echo 🚀 Vous pouvez maintenant lancer le programme avec les différents modes
echo.
echo 💡 Commandes disponibles:
echo    .\Core.exe                 # Mode normal avec configuration JSON
echo    .\Core.exe --select-capture # Interface de sélection graphique
echo    .\Core.exe --test-ai        # Test des composants IA
echo.
pause
