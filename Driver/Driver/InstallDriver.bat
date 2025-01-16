@echo off
echo Administrative permissions required. Detecting permissions...
net session >nul 2>&1
if %errorLevel% == 0 (
    cd /D "%~dp0"
	install-interception.exe /install
	pause
) else (
    echo Failure: Current permissions inadequate.
	pause
)