#!/bin/bash
# Deploy GRC System to Chel Server

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

# Configuration
CHEL_HOST="${CHEL_HOST:-chel}"
CHEL_USER="${CHEL_USER:-$USER}"
CHEL_DEPLOY_PATH="${CHEL_DEPLOY_PATH:-/opt/grc-system}"
REMOTE_PORT="${REMOTE_PORT:-22}"

echo "🚀 Deploying GRC System to Chel..."
echo "📍 Target: ${CHEL_USER}@${CHEL_HOST}:${CHEL_DEPLOY_PATH}"

# Build first
echo "🔨 Building solution..."
bash "$SCRIPT_DIR/build.sh"

# Find the Blazor app project (the one we'll deploy)
BLAZOR_PROJECT="$PROJECT_ROOT/src/Grc.Blazor/Grc.Blazor.csproj"

if [ ! -f "$BLAZOR_PROJECT" ]; then
    echo "❌ Blazor project not found: $BLAZOR_PROJECT"
    exit 1
fi

# Publish the application
echo "📦 Publishing application..."
dotnet publish "$BLAZOR_PROJECT" \
    --configuration Release \
    --output "$PROJECT_ROOT/publish" \
    --self-contained false

# Create deployment package
echo "📦 Creating deployment package..."
DEPLOY_DIR="$PROJECT_ROOT/deploy-temp"
rm -rf "$DEPLOY_DIR"
mkdir -p "$DEPLOY_DIR"

# Copy published files
cp -r "$PROJECT_ROOT/publish" "$DEPLOY_DIR/app"

# Copy configuration files
mkdir -p "$DEPLOY_DIR/config"
cp "$PROJECT_ROOT/appsettings.json" "$DEPLOY_DIR/config/"
cp -r "$PROJECT_ROOT/etc" "$DEPLOY_DIR/config/" 2>/dev/null || true

# Create deployment script for remote
cat > "$DEPLOY_DIR/deploy-remote.sh" << 'EOF'
#!/bin/bash
set -e

DEPLOY_PATH="$1"
APP_DIR="$DEPLOY_PATH/app"
BACKUP_DIR="$DEPLOY_PATH/backup-$(date +%Y%m%d-%H%M%S)"

echo "🔄 Deploying to $DEPLOY_PATH..."

# Backup existing deployment
if [ -d "$APP_DIR" ]; then
    echo "💾 Backing up existing deployment..."
    mkdir -p "$(dirname "$BACKUP_DIR")"
    mv "$APP_DIR" "$BACKUP_DIR"
fi

# Create deployment directory
mkdir -p "$(dirname "$APP_DIR")"

# Extract new deployment
echo "📦 Extracting new deployment..."
# Files will be copied here

# Set permissions
chmod +x "$APP_DIR/Grc.Blazor" 2>/dev/null || true

echo "✅ Deployment complete!"
echo "📍 Application directory: $APP_DIR"
echo "💾 Backup location: $BACKUP_DIR"
EOF

chmod +x "$DEPLOY_DIR/deploy-remote.sh"

# Create systemd service file (optional)
cat > "$DEPLOY_DIR/grc-system.service" << EOF
[Unit]
Description=GRC System Blazor Application
After=network.target

[Service]
Type=notify
ExecStart=/usr/bin/dotnet $DEPLOY_PATH/app/Grc.Blazor.dll
Restart=always
RestartSec=10
User=$CHEL_USER
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_ROOT=/usr/share/dotnet

[Install]
WantedBy=multi-user.target
EOF

# Copy to remote server
echo "📤 Copying files to ${CHEL_HOST}..."
rsync -avz --progress \
    -e "ssh -p $REMOTE_PORT" \
    "$DEPLOY_DIR/" \
    "${CHEL_USER}@${CHEL_HOST}:${CHEL_DEPLOY_PATH}/"

# Execute remote deployment
echo "🔄 Executing remote deployment..."
ssh -p "$REMOTE_PORT" "${CHEL_USER}@${CHEL_HOST}" << EOF
    cd $CHEL_DEPLOY_PATH
    bash deploy-remote.sh $CHEL_DEPLOY_PATH
EOF

# Cleanup
rm -rf "$DEPLOY_DIR"

echo "✅ Deployment complete!"
echo "🌐 Application should be running on ${CHEL_HOST}"
echo ""
echo "📝 Next steps:"
echo "   1. SSH to ${CHEL_HOST}: ssh -p $REMOTE_PORT ${CHEL_USER}@${CHEL_HOST}"
echo "   2. Check application status"
echo "   3. Configure database connection"
echo "   4. Run database migrations"
echo "   5. Start service: sudo systemctl start grc-system"
