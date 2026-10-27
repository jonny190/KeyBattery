#!/usr/bin/env bash
# Builds KeyBattery.exe for Windows x64 using the .NET SDK Docker image.
# The result is a single self-contained exe in app/dist.
#
# On Windows with the .NET 10 SDK installed you can instead run:
#   dotnet publish KeyBattery -c Release -o dist

set -euo pipefail

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SDK_IMAGE="mcr.microsoft.com/dotnet/sdk:10.0"

mkdir -p "$HERE/.build/home" "$HERE/dist"

docker run --rm \
    --user "$(id -u):$(id -g)" \
    -e HOME=/src/.build/home \
    -e DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    -e DOTNET_NOLOGO=1 \
    -v "$HERE":/src \
    -w /src \
    "$SDK_IMAGE" \
    dotnet publish KeyBattery -c Release -o dist

echo "Built $HERE/dist/KeyBattery.exe"
