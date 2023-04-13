@echo off
setlocal enabledelayedexpansion

set platform=x64
set rid=win10-%platform%

dotnet publish ".\src\ActionRepeater.UI\ActionRepeater.UI.csproj" -o ".\bin\Release" --verbosity n -c Release --nologo -r %rid% --self-contained false -p:Platform=%platform% -p:Version=0.3.0-alpha -p:PublishTrimmed=false -p:PublishReadyToRun=false -p:SatelliteResourceLanguages=en-US -p:DebuggerSupport=false -p:HttpActivityPropagationSupport=false -p:MetadataUpdaterSupport=false -p:EnableUnsafeBinaryFormatterSerialization=false -p:DebugType=None -p:DebugSymbols=false

pause
