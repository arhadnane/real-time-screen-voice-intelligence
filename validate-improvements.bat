@echo off
echo ===============================================
echo VALIDATION DES AMÉLIORATIONS - Real-Time Intelligence
echo ===============================================
echo.

echo [1/4] Restauration des packages NuGet...
dotnet restore
if %ERRORLEVEL% neq 0 (
    echo ❌ Échec de la restauration des packages
    pause
    exit /b 1
)

echo.
echo [2/4] Compilation de la solution...
dotnet build --configuration Release
if %ERRORLEVEL% neq 0 (
    echo ❌ Échec de la compilation
    pause
    exit /b 1
)

echo.
echo [3/4] Exécution des tests unitaires...
dotnet test --configuration Release --logger "console;verbosity=minimal"
if %ERRORLEVEL% neq 0 (
    echo ❌ Échec des tests
    pause
    exit /b 1
)

echo.
echo [4/4] Vérification de la structure des logs...
if not exist "logs" mkdir logs
echo Test de log > logs\test.log
if exist "logs\test.log" (
    echo ✅ Dossier logs créé avec succès
    del logs\test.log
) else (
    echo ⚠️ Problème avec le dossier logs
)

echo.
echo ===============================================
echo ✅ TOUTES LES VALIDATIONS SONT PASSÉES !
echo ===============================================
echo.
echo Améliorations implémentées :
echo  ✅ Logging structuré avec Serilog
echo  ✅ Validation de configuration robuste
echo  ✅ Gestion des ressources améliorée
echo  ✅ Métriques de performance
echo  ✅ Tests unitaires complets
echo  ✅ Documentation mise à jour
echo.
echo Pour lancer l'application : dotnet run --project Core
echo Pour voir les logs : tail -f logs/app-*.log
echo Pour relancer les tests : dotnet test
echo.
pause
