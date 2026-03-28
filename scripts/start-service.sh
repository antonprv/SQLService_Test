#!/usr/bin/env bash
# =============================================================================
# start-service.sh — сборка и запуск WCF-сервиса SqlWebService
#
# Требования:
#   Windows : MSBuild (из VS 2019/2022 или Build Tools) + .NET Framework 4.8
#   Linux   : Mono + xbuild/msbuild (mono-complete)
#
# Использование:
#   chmod +x scripts/start-service.sh
#   ./scripts/start-service.sh [--release] [--no-build]
# =============================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
PROJECT_DIR="$ROOT_DIR/src/SqlWebService"
CONFIG="Debug"

# ─── Разбор аргументов ───────────────────────────────────────────────────────
for arg in "$@"; do
  case $arg in
    --release)   CONFIG="Release" ;;
    --no-build)  NO_BUILD=1 ;;
  esac
done

echo "================================================================"
echo "  SqlWebService — Запуск сервиса  (конфигурация: $CONFIG)"
echo "================================================================"

# ─── Определяем компилятор ───────────────────────────────────────────────────
if command -v msbuild &>/dev/null; then
  MSBUILD="msbuild"
elif command -v xbuild &>/dev/null; then
  MSBUILD="xbuild"
elif command -v dotnet &>/dev/null; then
  # dotnet build поддерживает .NET Framework 4.x на Windows/Mono
  MSBUILD="dotnet build"
else
  echo "❌  Не найден msbuild/xbuild/dotnet. Установите .NET SDK или Mono."
  exit 1
fi

# ─── Сборка ──────────────────────────────────────────────────────────────────
if [[ -z "${NO_BUILD:-}" ]]; then
  echo ""
  echo "▶  Сборка проекта: SqlWebService.csproj [$CONFIG]"
  $MSBUILD "$PROJECT_DIR/SqlWebService.csproj" \
    -p:Configuration="$CONFIG" \
    -verbosity:minimal \
    -nologo
  echo "✓  Сборка завершена"
fi

# ─── Запуск ──────────────────────────────────────────────────────────────────
EXE="$PROJECT_DIR/bin/$CONFIG/SqlWebService.exe"

if [[ ! -f "$EXE" ]]; then
  echo "❌  Исполняемый файл не найден: $EXE"
  echo "    Запустите без флага --no-build"
  exit 1
fi

echo ""
echo "▶  Запуск: $EXE"
echo "   Endpoint : http://localhost:8080/SqlService"
echo "   WSDL/MEX : http://localhost:8080/SqlService/mex"
echo ""
echo "   Для остановки нажмите Ctrl+C или ENTER в консоли сервиса"
echo "================================================================"
echo ""

# На Linux запускаем через Mono; на Windows exe запускается напрямую
if [[ "$(uname -s)" == "Linux" || "$(uname -s)" == "Darwin" ]]; then
  if command -v mono &>/dev/null; then
    exec mono "$EXE"
  else
    echo "❌  Mono не установлен. На Linux выполните:"
    echo "    sudo apt-get install mono-complete"
    exit 1
  fi
else
  exec "$EXE"
fi
