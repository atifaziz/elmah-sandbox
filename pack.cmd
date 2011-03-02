@echo off

setlocal
pushd "%~dp0"

echo.
echo Building ELMAH-Sandbox
echo.

call build

IF ERRORLEVEL 1 GOTO :EOF

echo.
echo Making sure that NuGet.exe is up to date...
echo.

pkg\Tools\nuget.exe update

IF ERRORLEVEL 1 GOTO :EOF

if NOT EXIST "NuGet-Packages" md "NuGet-Packages"

for /f %%F in ('dir /a-d /b pkg\*.nuspec') DO CALL :PACKAGE pkg\%%F
GOTO :EOF

:PACKAGE
echo -------------------------------------------------------------------------------
echo Packaging %1
pkg\Tools\nuget.exe pack "%1" -v -o "NuGet-Packages"
GOTO :EOF
