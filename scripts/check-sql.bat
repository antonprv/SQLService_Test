@echo off
echo === Проверка доступности SQL Server ===
echo.

REM Проверка службы SQL Server
echo [1] Проверка служб SQL Server...
sc query | findstr /i "SQL Server (MSSQLSERVER) SQL Server (SQLEXPRESS)"
echo.

REM Проверка порта 1433
echo [2] Проверка порта 1433...
netstat -an | findstr ":1433"
if %errorlevel% neq 0 echo Порт 1433 не найден
echo.

REM Ping localhost
echo [3] Ping localhost...
ping -n 1 localhost | findstr "TTL"
echo.

REM Проверка подключения через telnet (если доступен)
echo [4] Проверка подключения к localhost:1433...
powershell -Command "Test-NetConnection -ComputerName localhost -Port 1433 -InformationLevel Quiet" 2>nul
if %errorlevel% neq 0 echo Test-NetConnection недоступен
echo.

pause
