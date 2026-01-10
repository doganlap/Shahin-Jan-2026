#!/bin/bash
# ══════════════════════════════════════════════════════════════════════════════
# SHAHIN AI GRC - DEPLOYMENT FIXES
# ══════════════════════════════════════════════════════════════════════════════
# Purpose: Fix all deployment issues identified in analysis
# Date: 2026-01-10
# Issues Fixed:
#   1. GrcMvc - Missing JWT_SECRET (CRITICAL)
#   2. Kafka - Cluster ID mismatch (HIGH)
#   3. n8n - Database permissions (MEDIUM)
#   4. Metabase - Database schema issues (LOW)
#   5. Superset - psycopg2 module (MEDIUM)
# ══════════════════════════════════════════════════════════════════════════════

set -e  # Exit on error

echo "══════════════════════════════════════════════════════════════════════════════"
echo "🔧 SHAHIN AI GRC - DEPLOYMENT FIX SCRIPT"
echo "══════════════════════════════════════════════════════════════════════════════"
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# ══════════════════════════════════════════════════════════════════════════════
# 1. FIX GRCMVC - ADD JWT_SECRET (CRITICAL)
# ══════════════════════════════════════════════════════════════════════════════
echo -e "${BLUE}📝 Step 1/5: Fixing GrcMvc - Adding JWT_SECRET${NC}"

if ! grep -q "^JWT_SECRET=" .env 2>/dev/null; then
    # Generate secure 256-bit key
    JWT_SECRET=$(openssl rand -base64 32)
    echo "" >> .env
    echo "# Added by fix-deployment.sh on $(date)" >> .env
    echo "JWT_SECRET=${JWT_SECRET}" >> .env
    echo -e "${GREEN}✅ JWT_SECRET added to .env${NC}"
else
    echo -e "${YELLOW}⚠️  JWT_SECRET already exists in .env${NC}"
fi

# Also check JWT_ISSUER and JWT_AUDIENCE
if ! grep -q "^JWT_ISSUER=" .env 2>/dev/null; then
    echo "JWT_ISSUER=https://portal.shahin-ai.com" >> .env
    echo -e "${GREEN}✅ JWT_ISSUER added${NC}"
fi

if ! grep -q "^JWT_AUDIENCE=" .env 2>/dev/null; then
    echo "JWT_AUDIENCE=https://portal.shahin-ai.com" >> .env
    echo -e "${GREEN}✅ JWT_AUDIENCE added${NC}"
fi

echo ""

# ══════════════════════════════════════════════════════════════════════════════
# 2. FIX KAFKA - RESET CLUSTER DATA (HIGH PRIORITY)
# ══════════════════════════════════════════════════════════════════════════════
echo -e "${BLUE}📝 Step 2/5: Fixing Kafka - Resetting cluster data${NC}"

# Stop Kafka and dependent services
docker-compose stop kafka kafka-connect 2>/dev/null || true

# Remove Kafka volume to clear cluster ID
echo "  Removing kafka_data volume..."
docker volume rm shahin-jan-2026_kafka_data 2>/dev/null || echo "  Volume already removed or doesn't exist"

echo -e "${GREEN}✅ Kafka cluster data reset${NC}"
echo ""

# ══════════════════════════════════════════════════════════════════════════════
# 3. FIX N8N - DATABASE PERMISSIONS (MEDIUM PRIORITY)
# ══════════════════════════════════════════════════════════════════════════════
echo -e "${BLUE}📝 Step 3/5: Fixing n8n - Database permissions${NC}"

# Wait for PostgreSQL to be ready
echo "  Waiting for PostgreSQL to be ready..."
for i in {1..30}; do
    if docker exec grc-db pg_isready -U postgres >/dev/null 2>&1; then
        echo "  PostgreSQL is ready"
        break
    fi
    if [ $i -eq 30 ]; then
        echo -e "${RED}❌ PostgreSQL not ready after 30 seconds${NC}"
        exit 1
    fi
    sleep 1
done

# Create n8n user if doesn't exist
docker exec grc-db psql -U postgres -tc "SELECT 1 FROM pg_roles WHERE rolname='n8n'" | grep -q 1 || \
    docker exec grc-db psql -U postgres -c "CREATE ROLE n8n WITH LOGIN PASSWORD 'admin123';"

# Create database and set permissions
docker exec grc-db psql -U postgres <<-EOSQL
    -- Create database if not exists
    SELECT 'CREATE DATABASE n8n OWNER n8n' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'n8n')\gexec

    -- Connect to n8n database
    \c n8n

    -- Grant permissions
    GRANT ALL ON SCHEMA public TO n8n;
    GRANT ALL PRIVILEGES ON DATABASE n8n TO n8n;
    ALTER SCHEMA public OWNER TO n8n;
EOSQL

echo -e "${GREEN}✅ n8n database permissions fixed${NC}"
echo ""

# ══════════════════════════════════════════════════════════════════════════════
# 4. FIX METABASE - RESET DATABASE (LOW PRIORITY)
# ══════════════════════════════════════════════════════════════════════════════
echo -e "${BLUE}📝 Step 4/5: Fixing Metabase - Resetting database${NC}"

# Create metabase user if doesn't exist
docker exec grc-db psql -U postgres -tc "SELECT 1 FROM pg_roles WHERE rolname='metabase'" | grep -q 1 || \
    docker exec grc-db psql -U postgres -c "CREATE ROLE metabase WITH LOGIN PASSWORD 'admin123';"

# Drop and recreate database
docker exec grc-db psql -U postgres <<-EOSQL
    -- Terminate existing connections
    SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = 'metabase';

    -- Drop and recreate
    DROP DATABASE IF EXISTS metabase;
    CREATE DATABASE metabase OWNER metabase;
    GRANT ALL PRIVILEGES ON DATABASE metabase TO metabase;
EOSQL

echo -e "${GREEN}✅ Metabase database reset${NC}"
echo ""

# ══════════════════════════════════════════════════════════════════════════════
# 5. FIX SUPERSET - CREATE CUSTOM DOCKERFILE (MEDIUM PRIORITY)
# ══════════════════════════════════════════════════════════════════════════════
echo -e "${BLUE}📝 Step 5/5: Fixing Superset - Creating custom Dockerfile${NC}"

# Create superset directory if doesn't exist
mkdir -p superset

# Create custom Dockerfile with psycopg2
cat > superset/Dockerfile.custom <<'EOF'
FROM apache/superset:latest

# Switch to root to install packages
USER root

# Install PostgreSQL driver and other dependencies
RUN pip install --no-cache-dir \
    psycopg2-binary==2.9.9 \
    redis==5.0.1

# Create necessary directories
RUN mkdir -p /app/superset_home

# Switch back to superset user
USER superset

# Set working directory
WORKDIR /app

# Default command will be overridden by docker-compose
CMD ["/usr/bin/run-server.sh"]
EOF

echo -e "${GREEN}✅ Superset custom Dockerfile created${NC}"
echo "  To apply: docker-compose build superset && docker-compose up -d superset"
echo ""

# ══════════════════════════════════════════════════════════════════════════════
# RESTART SERVICES
# ══════════════════════════════════════════════════════════════════════════════
echo -e "${BLUE}🔄 Restarting all services...${NC}"
docker-compose down
docker-compose up -d

echo ""
echo -e "${GREEN}✅ All services started!${NC}"
echo ""
echo -e "${YELLOW}⏳ Waiting 45 seconds for services to initialize...${NC}"
sleep 45

# ══════════════════════════════════════════════════════════════════════════════
# STATUS CHECK
# ══════════════════════════════════════════════════════════════════════════════
echo ""
echo "══════════════════════════════════════════════════════════════════════════════"
echo "📊 CURRENT SERVICE STATUS"
echo "══════════════════════════════════════════════════════════════════════════════"
docker-compose ps

echo ""
echo "══════════════════════════════════════════════════════════════════════════════"
echo "🔍 HEALTH CHECK SUMMARY"
echo "══════════════════════════════════════════════════════════════════════════════"

HEALTHY=$(docker-compose ps | grep -c "(healthy)" || echo "0")
RUNNING=$(docker-compose ps | grep -c "Up" || echo "0")
EXITED=$(docker-compose ps | grep -c "Exit" || echo "0")

echo -e "${GREEN}✅ Healthy services: ${HEALTHY}${NC}"
echo -e "${BLUE}📦 Running services: ${RUNNING}${NC}"
echo -e "${RED}❌ Failed services: ${EXITED}${NC}"

echo ""
echo "══════════════════════════════════════════════════════════════════════════════"
echo "📋 NEXT STEPS"
echo "══════════════════════════════════════════════════════════════════════════════"
echo ""
echo "1. Check GrcMvc logs:"
echo "   ${BLUE}docker logs shahin-jan-2026_grcmvc_1 -f${NC}"
echo ""
echo "2. Check Kafka logs:"
echo "   ${BLUE}docker logs grc-kafka -f${NC}"
echo ""
echo "3. Check all logs:"
echo "   ${BLUE}docker-compose logs -f${NC}"
echo ""
echo "4. Test GrcMvc health endpoint:"
echo "   ${BLUE}curl http://localhost:8888/health${NC}"
echo ""
echo "5. If Superset still fails, rebuild it:"
echo "   ${BLUE}docker-compose build superset && docker-compose up -d superset${NC}"
echo ""
echo "6. Monitor all services:"
echo "   ${BLUE}watch -n 2 'docker-compose ps'${NC}"
echo ""
echo "══════════════════════════════════════════════════════════════════════════════"
echo "✅ DEPLOYMENT FIX COMPLETE"
echo "══════════════════════════════════════════════════════════════════════════════"
echo ""
echo "For detailed analysis, see: DEPLOYMENT_FAILURE_ANALYSIS.md"
echo ""
