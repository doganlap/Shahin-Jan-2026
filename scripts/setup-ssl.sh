#!/bin/bash
# ══════════════════════════════════════════════════════════════════════════════
# SHAHIN AI GRC - SSL CERTIFICATE SETUP SCRIPT
# ══════════════════════════════════════════════════════════════════════════════
# Usage: ./scripts/setup-ssl.sh [domain]
# Example: ./scripts/setup-ssl.sh shahin-ai.com
# ══════════════════════════════════════════════════════════════════════════════

set -e

DOMAIN=${1:-"shahin-ai.com"}
PROJECT_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
NGINX_SSL_DIR="$PROJECT_ROOT/nginx/ssl"
APP_CERT_DIR="$PROJECT_ROOT/src/GrcMvc/certificates"

echo "══════════════════════════════════════════════════════════════════════════════"
echo "SHAHIN AI GRC - SSL Certificate Setup"
echo "══════════════════════════════════════════════════════════════════════════════"
echo "Domain: $DOMAIN"
echo "Project Root: $PROJECT_ROOT"
echo ""

# Create directories
mkdir -p "$NGINX_SSL_DIR"
mkdir -p "$APP_CERT_DIR"

# ════════════════════════════════════════════════════════════════════════════
# OPTION 1: Self-signed certificates (Development/Testing)
# ════════════════════════════════════════════════════════════════════════════
generate_self_signed() {
    echo "Generating self-signed certificates for development..."
    
    openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
        -keyout "$NGINX_SSL_DIR/privkey.pem" \
        -out "$NGINX_SSL_DIR/fullchain.pem" \
        -subj "/C=SA/ST=Riyadh/L=Riyadh/O=Shahin AI/CN=$DOMAIN" \
        -addext "subjectAltName=DNS:$DOMAIN,DNS:www.$DOMAIN,DNS:app.$DOMAIN,DNS:portal.$DOMAIN"
    
    echo "✅ Self-signed certificates created in $NGINX_SSL_DIR"
}

# ════════════════════════════════════════════════════════════════════════════
# OPTION 2: Let's Encrypt certificates (Production)
# ════════════════════════════════════════════════════════════════════════════
generate_letsencrypt() {
    echo "Setting up Let's Encrypt certificates..."
    
    # Check if certbot is installed
    if ! command -v certbot &> /dev/null; then
        echo "Installing certbot..."
        sudo apt-get update
        sudo apt-get install -y certbot
    fi
    
    # Stop nginx if running
    sudo systemctl stop nginx 2>/dev/null || true
    
    # Generate certificate
    sudo certbot certonly --standalone \
        -d "$DOMAIN" \
        -d "www.$DOMAIN" \
        -d "app.$DOMAIN" \
        -d "portal.$DOMAIN" \
        --agree-tos \
        --email "admin@$DOMAIN" \
        --non-interactive
    
    # Copy certificates to nginx/ssl
    sudo cp "/etc/letsencrypt/live/$DOMAIN/fullchain.pem" "$NGINX_SSL_DIR/"
    sudo cp "/etc/letsencrypt/live/$DOMAIN/privkey.pem" "$NGINX_SSL_DIR/"
    sudo chown $(whoami):$(whoami) "$NGINX_SSL_DIR"/*.pem
    
    echo "✅ Let's Encrypt certificates created in $NGINX_SSL_DIR"
}

# ════════════════════════════════════════════════════════════════════════════
# OPTION 3: ASP.NET Core development certificate
# ════════════════════════════════════════════════════════════════════════════
generate_dotnet_cert() {
    echo "Generating ASP.NET Core HTTPS certificate..."
    
    CERT_PASSWORD=${CERT_PASSWORD:-"ShahinAI2026!"}
    
    dotnet dev-certs https -ep "$APP_CERT_DIR/aspnetapp.pfx" -p "$CERT_PASSWORD"
    dotnet dev-certs https --trust 2>/dev/null || true
    
    echo "✅ ASP.NET Core certificate created: $APP_CERT_DIR/aspnetapp.pfx"
    echo "   Password: $CERT_PASSWORD (store securely!)"
}

# ════════════════════════════════════════════════════════════════════════════
# MAIN
# ════════════════════════════════════════════════════════════════════════════
echo ""
echo "Select certificate type:"
echo "  1) Self-signed (Development/Testing)"
echo "  2) Let's Encrypt (Production - requires domain pointing to this server)"
echo "  3) ASP.NET Core dev cert only"
echo "  4) All (Self-signed + ASP.NET Core)"
echo ""
read -p "Enter choice [1-4]: " choice

case $choice in
    1)
        generate_self_signed
        ;;
    2)
        generate_letsencrypt
        ;;
    3)
        generate_dotnet_cert
        ;;
    4)
        generate_self_signed
        generate_dotnet_cert
        ;;
    *)
        echo "Invalid choice. Generating self-signed certificates..."
        generate_self_signed
        ;;
esac

echo ""
echo "══════════════════════════════════════════════════════════════════════════════"
echo "SSL Setup Complete!"
echo "══════════════════════════════════════════════════════════════════════════════"
echo ""
echo "Files created:"
ls -la "$NGINX_SSL_DIR/" 2>/dev/null || echo "  (nginx/ssl empty)"
ls -la "$APP_CERT_DIR/" 2>/dev/null || echo "  (certificates empty)"
echo ""
echo "Next steps:"
echo "  1. Copy .env.production.secure.template to .env.production.secure"
echo "  2. Fill in all environment variables"
echo "  3. Run: docker-compose -f docker-compose.production.yml up -d"
echo ""
