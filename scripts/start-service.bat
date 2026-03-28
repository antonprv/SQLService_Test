@echo off
:: =============================================================================
:: start-service.bat — сборка и запуск WCF-сервиса SqlWebService
::
:: Требования:
::   .NET Framework 4.8 SDK
::   MSBuild (из Visual Studio 2019/2022 или Build Tools)
::
:: Использование:
::   start-service.bat [release] [no-build]
:: =============================================================================
setlocal EnableDelayedExpansion

set CONFIG=Debug
set NO_BUILD=0

:: ─── Разбор аргументов ───────────────────────────────────────────────────────
for %%A in (%*) do (
    if /I "%%A"=="release"   set CONFIG=Release
    if /I "%%A"=="no-build"  set NO_BUILD=1
)

set SCRIPT_DIR=%~dp0
set ROOT_DIR=%SCRIPT_DIR%..
set PROJECT_DIR=%ROOT_DIR%\src\SqlWebService

echo ================================================================
echo   SqlWebService — Запуск сервиса  (конфигурация: %CONFIG%)
echo ================================================================

:: ─── Поиск MSBuild ───────────────────────────────────────────────────────────
call :FindMSBuild
if "%MSBUILD%"=="" (
    echo [ERROR] MSBuild не найден.
    echo         Установите Visual Studio 2019/2022 или Build Tools for Visual Studio.
    echo         https://visualstudio.microsoft.com/downloads/
    pause
    exit /b 1
)

:: ─── Регистрация URL-префикса (если не зарегистрирован) ──────────────────────
if %NO_BUILD%==0 (
    echo.
    echo [INFO] Проверка регистрации URL-префикса...
    netsh http show urlacl url=http://+:8080/SqlService/ >nul 2>&1
    if errorlevel 1 (
        echo [INFO] Регистрируем http://+:8080/SqlService/ для текущего пользователя...
        netsh http add urlacl url=http://+:8080/SqlService/ user="%USERNAME%" >nul 2>&1
        if errorlevel 1 (
            echo [WARN] Не удалось зарегистрировать URL. Запустите от имени администратора
            echo        или выполните вручную:
            echo        netsh http add urlacl url=http://+:8080/SqlService/ user=Everyone
        ) else (
            echo [OK]   URL зарегистрирован.
        )
    ) else (
        echo [OK]   URL уже зарегистрирован.
    )
)

:: ─── Сборка ──────────────────────────────────────────────────────────────────
if %NO_BUILD%==0 (
    echo.
    echo [BUILD] Сборка SqlWebService.csproj [%CONFIG%]...
    "%MSBUILD%" "%PROJECT_DIR%\SqlWebService.csproj" ^
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
set EXE=%PROJECT_DIR%\bin\%CONFIG%\SqlWebService.exe

if not exist "%EXE%" (
    echo [ERROR] Файл не найден: %EXE%
    echo         Запустите без аргумента no-build
    pause
    exit /b 1
)

echo.
echo [RUN]  Запуск: %EXE%
echo        Endpoint : http://localhost:8080/SqlService
echo        WSDL/MEX : http://localhost:8080/SqlService/mex
echo.
echo        Для остановки нажмите ENTER в консоли сервиса.
echo ================================================================
echo.
"%EXE%"
goto :EOF

:: ─── Функция поиска MSBuild ──────────────────────────────────────────────────
:FindMSBuild
set MSBUILD=

:: Пробуем vswhere (рекомендуемый способ)
set VSWHERE="%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
if exist %VSWHERE% (
    for /f "usebackq tokens=*" %%i in (`%VSWHERE% -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do (
        set MSBUILD=%%i
        goto :MSBuildFound
    )
)

:: Фиксированные пути VS 2022 / 2019 / Build Tools
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

:: dotnet как запасной вариант
where dotnet >nul 2>&1 && (
    set MSBUILD=dotnet build
    goto :MSBuildFound
)

:MSBuildFound
goto :EOF
