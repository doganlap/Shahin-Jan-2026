#!/bin/bash
# Build GRC System Solution

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

cd "$PROJECT_ROOT"

echo "🔨 Building GRC System..."
echo "📁 Project root: $PROJECT_ROOT"

# Check if dotnet is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK not found. Installing..."
    bash "$SCRIPT_DIR/install-dotnet.sh"
fi

echo "📦 Restoring NuGet packages..."
dotnet restore

echo "🔨 Building solution..."
dotnet build --configuration Release --no-restore

if [ $? -eq 0 ]; then
    echo "✅ Build successful!"
    echo ""
    echo "📊 Build artifacts location:"
    find . -name "*.dll" -path "*/bin/Release/*" | head -5
else
    echo "❌ Build failed!"
    exit 1
fi
