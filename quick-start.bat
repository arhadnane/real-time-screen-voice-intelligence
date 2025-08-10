@echo off
echo Demarrage rapide de l'application Blazor...

cd /d "d:\Coding\VSCodeProject\Real-Time Screen and Voice Intelligence\WebApp\RealTimeIntelligenceApp"

echo Building...
dotnet build --no-restore

echo Starting server...
start http://localhost:5000
dotnet run --urls "http://localhost:5000" --no-build
