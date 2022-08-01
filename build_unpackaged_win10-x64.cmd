@echo off
setlocal enabledelayedexpansion

for /f "usebackq tokens=*" %%i in (`vswhere.exe -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do (
  set msbuildPath="%%i"
  goto afterLoop
)

:afterLoop
%msbuildPath% ActionRepeater.sln -t:restore,rebuild,publish -p:Configuration=Release -p:Platform=x64 -p:Version=0.0.1-alpha -p:SelfContained=False -p:PublishReadyToRun=False -p:SatelliteResourceLanguages=en
echo:
pause
