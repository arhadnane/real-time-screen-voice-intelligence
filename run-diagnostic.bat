@echo off
echo === DIAGNOSTIC DU SYSTÈME ===
echo.
echo Vérification des fichiers tessdata:
if exist "tessdata\eng.traineddata" (
    echo ✅ eng.traineddata présent
) else (
    echo ❌ eng.traineddata manquant
)

if exist "tessdata\fra.traineddata" (
    echo ✅ fra.traineddata présent  
) else (
    echo ❌ fra.traineddata manquant
)

echo.
echo Vérification du modèle Vosk:
if exist "vosk-models\vosk-model-small-fr-0.22" (
    echo ✅ Modèle Vosk présent
) else (
    echo ❌ Modèle Vosk manquant
)

echo.
echo Variables d'environnement:
echo TESSDATA_PREFIX=%TESSDATA_PREFIX%
echo.

echo Exécution du système avec gestion d'erreurs améliorée...
dotnet run --project Core
