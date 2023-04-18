@echo off
setlocal enabledelayedexpansion

for /f "usebackq tokens=*" %%i in (`vswhere.exe -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do (
    set msbuildPath="%%i"
    goto afterLoop
)

:afterLoop

set config=Release
set platform=x64
set rid=win10-%platform%

REM build the c++ project using vs build tools' msbuils, because .net's msbuild cant build c++ projects
%msbuildPath% ".\src\PathWindows\PathWindows.vcxproj" -t:rebuild -p:Configuration=%config% -p:Platform=%platform% -p:RuntimeIdentifier=%rid%

dotnet clean ".\src\ActionRepeater.UI\ActionRepeater.UI.csproj" -c %config% --verbosity m
dotnet publish ".\src\ActionRepeater.UI\ActionRepeater.UI.csproj" -o ".\bin\%config%" --verbosity n -c %config% --nologo -r %rid% --self-contained false -p:Platform=%platform% -p:Version=0.3.0-alpha -p:PublishTrimmed=false -p:PublishReadyToRun=false -p:SatelliteResourceLanguages=en-US -p:DebuggerSupport=false -p:HttpActivityPropagationSupport=false -p:MetadataUpdaterSupport=false -p:EnableUnsafeBinaryFormatterSerialization=false -p:DebugType=None -p:DebugSymbols=false

xcopy ".\src\PathWindows\bin\%platform%\%config%\PathWindows.dll" ".\bin\%config%\" /v /k /r /y /f

pause
