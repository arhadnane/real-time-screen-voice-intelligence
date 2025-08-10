@echo off
echo Test de la fonction historique
echo s > temp_input.txt
echo h >> temp_input.txt
echo q >> temp_input.txt

echo Commandes envoyées: s (capture), h (historique), q (quitter)
dotnet run --project Core < temp_input.txt

del temp_input.txt
echo Test terminé
pause
