#!/bin/bash
# ══════════════════════════════════════════════════════════════════════════════
# SHAHIN AI GRC - PRODUCTION DEPLOYMENT SCRIPT
# ══════════════════════════════════════════════════════════════════════════════
# Usage: ./scripts/deploy-production.sh [command]
# Commands: setup | build | deploy | status | logs | stop | restart
# ══════════════════════════════════════════════════════════════════════════════

set -e

PROJECT_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
COMPOSE_FILE="$PROJECT_ROOT/docker-compose.production.yml"
ENV_FILE="$PROJECT_ROOT/.env.production.secure"

cd "$PROJECT_ROOT"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

log_info() { echo -e "${GREEN}[INFO]${NC} $1"; }
log_warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }

# ════════════════════════════════════════════════════════════════════════════
# SETUP: Create required directories and files
# ════════════════════════════════════════════════════════════════════════════
setup() {
    log_info "Setting up production environment..."
    
    # Create host directories for volumes
    sudo mkdir -p /var/lib/shahin-ai/{postgres,redis}
    sudo mkdir -p /var/www/shahin-ai/storage
    sudo mkdir -p /var/log/shahin-ai
    sudo chown -R $USER:$USER /var/lib/shahin-ai /var/www/shahin-ai /var/log/shahin-ai
    
    # Create SSL directories
    mkdir -p nginx/ssl src/GrcMvc/certificates
    
    # Check for env file
    if [ ! -f "$ENV_FILE" ]; then
        if [ -f "$PROJECT_ROOT/.env.production.secure.template" ]; then
            cp "$PROJECT_ROOT/.env.production.secure.template" "$ENV_FILE"
            log_warn "Created $ENV_FILE from template - EDIT THIS FILE!"
        else
            log_error ".env.production.secure not found! Copy from template first."
            exit 1
        fi
    fi
    
    # Check for SSL certificates
    if [ ! -f "nginx/ssl/fullchain.pem" ]; then
        log_warn "SSL certificates not found in nginx/ssl/"
        log_info "Run: ./scripts/setup-ssl.sh to generate certificates"
    fi
    
    log_info "Setup complete!"
}

# ════════════════════════════════════════════════════════════════════════════
# BUILD: Build Docker images
# ════════════════════════════════════════════════════════════════════════════
build() {
    log_info "Building production images..."
    
    # Build .NET application first
    log_info "Building .NET application..."
    dotnet build src/GrcMvc/GrcMvc.csproj -c Release
    
    # Build Docker images
    docker-compose -f "$COMPOSE_FILE" build --no-cache
    
    log_info "Build complete!"
}

# ════════════════════════════════════════════════════════════════════════════
# DEPLOY: Start production containers
# ════════════════════════════════════════════════════════════════════════════
deploy() {
    log_info "Deploying to production..."
    
    # Verify prerequisites
    if [ ! -f "$ENV_FILE" ]; then
        log_error "Environment file not found: $ENV_FILE"
        log_info "Run: ./scripts/deploy-production.sh setup"
        exit 1
    fi
    
    # Pull latest images and start
    docker-compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" up -d
    
    # Wait for health checks
    log_info "Waiting for services to be healthy..."
    sleep 10
    
    # Show status
    status
    
    log_info "Deployment complete!"
    log_info "Access application at: https://app.shahin-ai.com"
}

# ════════════════════════════════════════════════════════════════════════════
# STATUS: Show container status
# ════════════════════════════════════════════════════════════════════════════
status() {
    log_info "Container status:"
    docker-compose -f "$COMPOSE_FILE" ps
    
    echo ""
    log_info "Health check:"
    curl -s -k https://localhost/health 2>/dev/null || curl -s http://localhost:8888/health 2>/dev/null || echo "Health endpoint not responding"
}

# ════════════════════════════════════════════════════════════════════════════
# LOGS: View container logs
# ════════════════════════════════════════════════════════════════════════════
logs() {
    docker-compose -f "$COMPOSE_FILE" logs -f --tail=100 ${2:-}
}

# ════════════════════════════════════════════════════════════════════════════
# STOP: Stop all containers
# ════════════════════════════════════════════════════════════════════════════
stop() {
    log_info "Stopping production containers..."
    docker-compose -f "$COMPOSE_FILE" down
    log_info "Containers stopped."
}

# ════════════════════════════════════════════════════════════════════════════
# RESTART: Restart containers
# ════════════════════════════════════════════════════════════════════════════
restart() {
    log_info "Restarting production containers..."
    docker-compose -f "$COMPOSE_FILE" restart
    status
}

# ════════════════════════════════════════════════════════════════════════════
# MAIN
# ════════════════════════════════════════════════════════════════════════════
case "${1:-help}" in
    setup)   setup ;;
    build)   build ;;
    deploy)  deploy ;;
    status)  status ;;
    logs)    logs "$@" ;;
    stop)    stop ;;
    restart) restart ;;
    *)
        echo "Usage: $0 {setup|build|deploy|status|logs|stop|restart}"
        echo ""
        echo "Commands:"
        echo "  setup   - Create directories and prepare environment"
        echo "  build   - Build Docker images"
        echo "  deploy  - Start production containers"
        echo "  status  - Show container status"
        echo "  logs    - View container logs (optional: service name)"
        echo "  stop    - Stop all containers"
        echo "  restart - Restart all containers"
        ;;
esac
