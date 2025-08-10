@echo off
echo === TEST OCR DIRECT ===
echo.

echo Verification des fichiers tessdata:
dir tessdata

echo.
echo Test d'execution avec gestion d'erreurs specifique...
echo.

rem Essayer de lancer le système avec mode OCR
dotnet run --project Core --test-ocr

echo.
echo Si le test OCR ne fonctionne pas, essayons le systeme normal:
dotnet run --project Core

pause
