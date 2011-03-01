@echo off

echo.
echo Making sure that NuGet.exe is up to date...
echo.
NuSpecs\Tools\nuget.exe update

if NOT EXIST "NuGet-Packages" md "NuGet-Packages"

for /f %%F in ('dir /a-d /b NuSpecs\*.nuspec') DO CALL :PACKAGE NuSpecs\%%F
GOTO :EOF

:PACKAGE
echo -------------------------------------------------------------------------------
echo Packaging %1
NuSpecs\Tools\nuget.exe pack "%1" -v -o "NuGet-Packages"
GOTO :EOF
