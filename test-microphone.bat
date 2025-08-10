@echo off
echo === TEST MICROPHONE ===
echo Ce script va tester votre microphone en detail
echo.
cd /d "d:\Coding\VSCodeProject\Real-Time Screen and Voice Intelligence"
dotnet run --project Core --test-mic
echo.
echo Test termine!
pause
