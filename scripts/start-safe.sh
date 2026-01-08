#!/bin/bash
# Safe Container Startup Script
# Prevents conflicts and validates configuration before starting

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

cd "$PROJECT_ROOT"

echo "=== Pre-Startup Validation ==="
echo

# Check if .env exists
if [ ! -f .env ]; then
    echo -e "${RED}ERROR:${NC} .env file not found"
    echo "  Create .env from .env.example"
    exit 1
fi

# Check for existing containers
if docker ps -a | grep -q "grc-db"; then
    echo -e "${YELLOW}WARNING:${NC} grc-db container already exists"
    CONTAINER_STATUS=$(docker ps -a --format '{{.Status}}' --filter name=grc-db)
    echo "  Status: $CONTAINER_STATUS"
    
    read -p "Remove existing container and start fresh? (y/N) " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        echo "Stopping and removing existing containers..."
        docker compose down
    else
        echo "Using existing container..."
    fi
fi

# Check port availability
if netstat -tuln 2>/dev/null | grep -q ":5433 " || ss -tuln 2>/dev/null | grep -q ":5433 "; then
    if ! docker ps | grep -q "grc-db"; then
        echo -e "${RED}ERROR:${NC} Port 5433 is already in use"
        echo "  Stop the service using port 5433 or change DB_PORT in .env"
        exit 1
    fi
fi

# Validate docker-compose.yml
if ! docker compose config > /dev/null 2>&1; then
    echo -e "${RED}ERROR:${NC} docker-compose.yml is invalid"
    docker compose config
    exit 1
fi

echo -e "${GREEN}✓${NC} All pre-startup checks passed"
echo
echo "Starting services..."
docker compose up -d

echo
echo "Waiting for services to be healthy..."
sleep 10

# Run health check
if [ -f "$SCRIPT_DIR/monitor-db.sh" ]; then
    echo
    "$SCRIPT_DIR/monitor-db.sh"
fi
