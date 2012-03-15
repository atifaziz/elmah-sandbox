@echo off

setlocal
pushd "%~dp0"

set hr=---------------------------------------------------------------------------
REM set binzip=ELMAH-1.2-sp1-bin-x86.zip
set nuget=pkg\Tools\nuget.exe

:main
call :clean ^
 && call :buildsandbox ^
 && call :autoupdate ^
 && call :packall pkg\*.nuspec
goto :EOF


:buildsandbox
call build
goto :EOF


:clean
call :rd bin && call :rd base && call :rd tmp
goto :EOF

:rd
if exist %1 rd %1 /s /q
if exist %1 exit /b 1
goto :EOF

:md
if not exist %1 md %1
goto :EOF

:download
setlocal
echo %hr%
echo Downloading %1...
call tools\wgets http://elmah.googlecode.com/files/%1 %2 %3 %4 %5 %6 %7 %8 %9
goto :EOF

:unzip
setlocal
echo %hr%
tools\7za x %*
goto :EOF

:autoupdate
echo %hr%
echo Making sure that NuGet.exe is up to date...
"%nuget%" update -self
goto :EOF

:packall
for /f %%F in ('dir /a-d /b "%1"') DO (
CALL :pack pkg\%%F
IF ERRORLEVEL 1 GOTO :EOF
)
GOTO :EOF

:pack
echo %hr%
echo Packaging %1
call :md bin && "%nuget%" pack "%1" -verbose -output bin
GOTO :EOF
