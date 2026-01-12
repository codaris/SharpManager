@echo off

rem Check if arduino-cli is available in PATH
where arduino-cli >nul 2>&1
if errorlevel 1 (
    echo ERROR: 'arduino-cli' is not found in PATH. Please install it or add it to your PATH.
    pause
    exit /b 1
)

echo Building all Arduino firmwares

set SKETCH_DIR=%~dp0Arduino
set OUTPUT_DIR=%~dp0build
set DEPLOY_DIR=%~dp0Desktop\SharpManager.Common\Firmware

if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%"

rem Compile each board
call :compile "arduino:avr:leonardo"        Leonardo.hex
call :compile "arduino:avr:mega"            Mega2560.hex
call :compile "arduino:avr:micro"           Micro.hex
call :compile "arduino:avr:nano"			Nano.hex
call :compile "arduino:avr:uno"             Uno.hex

echo Done.
pause
exit /b

:compile
echo Compiling for %~1 to %~2
echo arduino-cli compile --fqbn %1 --build-path "%OUTPUT_DIR%" "%SKETCH_DIR%"
arduino-cli compile --fqbn %1 --build-path "%OUTPUT_DIR%" "%SKETCH_DIR%"
if %errorlevel% neq 0 (
    echo Failed to compile %1
) else (
    copy /Y "%OUTPUT_DIR%\Arduino.ino.hex" "%DEPLOY_DIR%\%2" >nul
    echo Success: %2
)
echo.
exit /b