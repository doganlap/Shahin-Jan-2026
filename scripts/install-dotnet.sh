#!/bin/bash
# Install .NET SDK 8.0 for GRC System

set -e

echo "🔧 Installing .NET SDK 8.0..."

# Check if dotnet is already installed
if command -v dotnet &> /dev/null; then
    echo "✅ .NET SDK already installed:"
    dotnet --version
    exit 0
fi

# Detect OS
if [ -f /etc/os-release ]; then
    . /etc/os-release
    OS=$ID
    VERSION=$VERSION_ID
else
    echo "❌ Cannot detect OS. Please install .NET SDK manually."
    exit 1
fi

echo "📦 Detected OS: $OS $VERSION"

# Install based on OS
if [ "$OS" = "ubuntu" ] || [ "$OS" = "debian" ]; then
    echo "📥 Installing via apt (Ubuntu/Debian)..."
    
    # Add Microsoft package repository
    wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O /tmp/packages-microsoft-prod.deb
    sudo dpkg -i /tmp/packages-microsoft-prod.deb
    rm /tmp/packages-microsoft-prod.deb
    
    sudo apt-get update
    sudo apt-get install -y dotnet-sdk-8.0
    
elif [ "$OS" = "rhel" ] || [ "$OS" = "centos" ] || [ "$OS" = "fedora" ]; then
    echo "📥 Installing via yum/dnf (RHEL/CentOS/Fedora)..."
    
    if command -v dnf &> /dev/null; then
        sudo dnf install -y dotnet-sdk-8.0
    else
        sudo yum install -y dotnet-sdk-8.0
    fi
    
else
    echo "⚠️  Unsupported OS. Please install .NET SDK 8.0 manually:"
    echo "   Visit: https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
fi

# Verify installation
if command -v dotnet &> /dev/null; then
    echo "✅ .NET SDK installed successfully!"
    dotnet --version
else
    echo "❌ Installation failed. Please install manually."
    exit 1
fi
