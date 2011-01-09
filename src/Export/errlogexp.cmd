@echo off
setlocal
set IRONPYTHONPATH=%IRONPYTHONPATH%;%~dp0lib
"%~dp0ipy\ipy" "%~dpn0.py" %*
