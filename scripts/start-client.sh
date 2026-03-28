#!/usr/bin/env bash
# =============================================================================
# start-client.sh — сборка и запуск WinForms-клиента SqlWebServiceClient
#
# Требования:
#   Windows : MSBuild + .NET Framework 4.8
#   Linux   : Mono + mono-complete (WinForms через Wine/Mono)
#
# Использование:
#   chmod +x scripts/start-client.sh
#   ./scripts/start-client.sh [--release] [--no-build]
# =============================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
PROJECT_DIR="$ROOT_DIR/src/SqlWebServiceClient"
CONFIG="Debug"

# ─── Разбор аргументов ───────────────────────────────────────────────────────
for arg in "$@"; do
  case $arg in
    --release)   CONFIG="Release" ;;
    --no-build)  NO_BUILD=1 ;;
  esac
done

echo "================================================================"
echo "  SqlWebServiceClient — Запуск клиента  (конфигурация: $CONFIG)"
echo "================================================================"

# ─── Определяем компилятор ───────────────────────────────────────────────────
if command -v msbuild &>/dev/null; then
  MSBUILD="msbuild"
elif command -v xbuild &>/dev/null; then
  MSBUILD="xbuild"
elif command -v dotnet &>/dev/null; then
  MSBUILD="dotnet build"
else
  echo "❌  Не найден msbuild/xbuild/dotnet."
  exit 1
fi

# ─── Сборка ──────────────────────────────────────────────────────────────────
if [[ -z "${NO_BUILD:-}" ]]; then
  echo ""
  echo "▶  Сборка проекта: SqlWebServiceClient.csproj [$CONFIG]"
  $MSBUILD "$PROJECT_DIR/SqlWebServiceClient.csproj" \
    -p:Configuration="$CONFIG" \
    -verbosity:minimal \
    -nologo
  echo "✓  Сборка завершена"
fi

# ─── Запуск ──────────────────────────────────────────────────────────────────
EXE="$PROJECT_DIR/bin/$CONFIG/SqlWebServiceClient.exe"

if [[ ! -f "$EXE" ]]; then
  echo "❌  Исполняемый файл не найден: $EXE"
  exit 1
fi

echo ""
echo "▶  Запуск: $EXE"
echo "   Убедитесь, что SqlWebService запущен на http://localhost:8080/SqlService"
echo "================================================================"
echo ""

if [[ "$(uname -s)" == "Linux" || "$(uname -s)" == "Darwin" ]]; then
  if command -v mono &>/dev/null; then
    exec mono "$EXE"
  else
    echo "❌  Mono не установлен. Выполните: sudo apt-get install mono-complete"
    exit 1
  fi
else
  exec "$EXE"
fi
