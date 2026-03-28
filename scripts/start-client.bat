@echo off
:: =============================================================================
:: start-client.bat — сборка и запуск WinForms-клиента SqlWebServiceClient
::
:: Требования:
::   .NET Framework 4.8 SDK
::   MSBuild (из Visual Studio 2019/2022 или Build Tools)
::
:: Использование:
::   start-client.bat [release] [no-build]
:: =============================================================================
setlocal EnableDelayedExpansion

set CONFIG=Debug
set NO_BUILD=0

for %%A in (%*) do (
    if /I "%%A"=="release"   set CONFIG=Release
    if /I "%%A"=="no-build"  set NO_BUILD=1
)

set SCRIPT_DIR=%~dp0
set ROOT_DIR=%SCRIPT_DIR%..
set PROJECT_DIR=%ROOT_DIR%\src\SqlWebServiceClient

echo ================================================================
echo   SqlWebServiceClient — Запуск клиента  (конфигурация: %CONFIG%)
echo ================================================================

call :FindMSBuild
if "%MSBUILD%"=="" (
    echo [ERROR] MSBuild не найден.
    pause
    exit /b 1
)

:: ─── Сборка ──────────────────────────────────────────────────────────────────
if %NO_BUILD%==0 (
    echo.
    echo [BUILD] Сборка SqlWebServiceClient.csproj [%CONFIG%]...
    "%MSBUILD%" "%PROJECT_DIR%\SqlWebServiceClient.csproj" ^
        /p:Configuration=%CONFIG% ^
        /verbosity:minimal ^
        /nologo
    if errorlevel 1 (
        echo [ERROR] Сборка завершилась с ошибкой.
        pause
        exit /b 1
    )
    echo [OK]   Сборка завершена.
)

:: ─── Запуск ──────────────────────────────────────────────────────────────────
set EXE=%PROJECT_DIR%\bin\%CONFIG%\SqlWebServiceClient.exe

if not exist "%EXE%" (
    echo [ERROR] Файл не найден: %EXE%
    pause
    exit /b 1
)

echo.
echo [RUN]  Запуск: %EXE%
echo        Убедитесь, что сервис запущен: http://localhost:8080/SqlService
echo ================================================================
echo.
start "" "%EXE%"
goto :EOF

:FindMSBuild
set MSBUILD=
set VSWHERE="%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
if exist %VSWHERE% (
    for /f "usebackq tokens=*" %%i in (`%VSWHERE% -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do (
        set MSBUILD=%%i
        goto :MSBuildFound
    )
)
for %%P in (
    "%ProgramFiles%\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
    "%ProgramFiles%\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe"
    "%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
    "%ProgramFiles(x86)%\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
    "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
    "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe"
    "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
    "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
) do (
    if exist %%P (
        set MSBUILD=%%~P
        goto :MSBuildFound
    )
)
where dotnet >nul 2>&1 && ( set MSBUILD=dotnet build && goto :MSBuildFound )
:MSBuildFound
goto :EOF
