#!/bin/bash
# Verify PostgreSQL Database Connection
# This script helps diagnose connection issues by testing direct DB access

set -e

echo "========================================="
echo "GRC System - Database Connection Check"
echo "========================================="
echo ""

# Default values (can be overridden via environment variables)
DB_HOST="${DB_HOST:-localhost}"
DB_PORT="${DB_PORT:-5433}"
DB_USER="${DB_USER:-postgres}"
DB_PASSWORD="${DB_PASSWORD:-postgres}"
DB_NAME="${DB_NAME:-GrcMvcDb}"

echo "Configuration:"
echo "  Host: $DB_HOST"
echo "  Port: $DB_PORT"
echo "  User: $DB_USER"
echo "  Database: $DB_NAME"
echo ""

# Check if docker container is running
echo "1. Checking Docker containers..."
if docker ps --filter "name=db" --format "{{.Names}}" | grep -q "grc-db"; then
    echo "   ✅ grc-db container is running"
    docker ps --filter "name=grc-db" --format "   Container: {{.Names}} | Ports: {{.Ports}} | Status: {{.Status}}"
else
    echo "   ❌ grc-db container is NOT running"
    echo "   Run: docker-compose up -d db"
    exit 1
fi
echo ""

# Check if psql is available
if command -v psql > /dev/null 2>&1; then
    echo "2. Testing connection with psql..."
    export PGPASSWORD="$DB_PASSWORD"
    if psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -c "SELECT version();" > /dev/null 2>&1; then
        echo "   ✅ Connection successful"
        echo ""
        echo "   Database version:"
        psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -c "SELECT version();" | head -1
        echo ""
        echo "   Database exists check:"
        psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -c "SELECT current_database();" 2>&1 | grep -v "could not connect" || echo "   ❌ Connection failed"
    else
        echo "   ❌ Connection failed"
        echo ""
        echo "   Trying to diagnose..."
        echo "   Attempting connection (will show error):"
        psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -c "SELECT 1;" 2>&1 | head -5
    fi
    unset PGPASSWORD
else
    echo "2. psql not found (skipping direct connection test)"
    echo "   Install PostgreSQL client tools to test directly"
fi
echo ""

# Test via docker exec (if container is running)
echo "3. Testing connection from inside container..."
if docker exec grc-db psql -U "$DB_USER" -d "$DB_NAME" -c "SELECT current_database(), current_user;" > /dev/null 2>&1; then
    echo "   ✅ Internal container connection successful"
    docker exec grc-db psql -U "$DB_USER" -d "$DB_NAME" -c "SELECT current_database() as database, current_user as user, version();" 2>&1 | grep -E "(database|user|PostgreSQL)" | head -3
else
    echo "   ❌ Internal container connection failed"
    echo "   This suggests DB credentials or initialization issue"
fi
echo ""

# Check environment variables
echo "4. Checking environment variable format (if .env exists)..."
if [ -f ".env" ]; then
    echo "   ✅ .env file found"
    if grep -q "CONNECTION_STRING" .env; then
        echo "   CONNECTION_STRING is set in .env"
        # Show first part (without password if visible)
        grep "CONNECTION_STRING" .env | sed 's/Password=[^;]*/Password=***/g' | head -1
    else
        echo "   ⚠️  CONNECTION_STRING not found in .env"
    fi
    if grep -q "DB_USER\|DB_PASSWORD\|DB_PORT" .env; then
        echo "   DB_* variables found in .env"
    fi
else
    echo "   ⚠️  .env file not found (this is normal if using defaults)"
fi
echo ""

# Expected connection string format
echo "5. Expected Connection String Format:"
echo "   Host=$DB_HOST;Database=$DB_NAME;Username=$DB_USER;Password=$DB_PASSWORD;Port=$DB_PORT"
echo "   OR (Npgsql format):"
echo "   Host=$DB_HOST;Database=$DB_NAME;Username=$DB_USER;Password=$DB_PASSWORD;Port=$DB_PORT"
echo ""

# Docker compose override check
echo "6. Docker Compose Environment Override:"
echo "   docker-compose.yml sets: ConnectionStrings__DefaultConnection=\${CONNECTION_STRING}"
echo "   This OVERRIDES appsettings.json if CONNECTION_STRING is set in .env"
echo ""

# Recommendations
echo "========================================="
echo "Recommendations:"
echo "========================================="
echo ""
echo "If connection fails:"
echo "  1. Verify .env file has correct CONNECTION_STRING:"
echo "     CONNECTION_STRING=\"Host=localhost;Database=$DB_NAME;Username=$DB_USER;Password=$DB_PASSWORD;Port=$DB_PORT\""
echo ""
echo "  2. OR set individual DB variables in .env:"
echo "     DB_USER=$DB_USER"
echo "     DB_PASSWORD=$DB_PASSWORD"
echo "     DB_PORT=$DB_PORT"
echo "     DB_NAME=$DB_NAME"
echo ""
echo "  3. Restart application after changing .env:"
echo "     docker-compose restart grcmvc"
echo ""
echo "  4. Check application logs:"
echo "     docker-compose logs grcmvc | grep -i connection"
echo ""
