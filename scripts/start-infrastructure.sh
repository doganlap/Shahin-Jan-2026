#!/bin/bash
# Start GRC Infrastructure Services
# Camunda + Kafka + Zookeeper + Redis

set -e

echo "==========================================="
echo "🚀 Starting GRC Infrastructure Services"
echo "==========================================="

# Colors
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

cd /home/dogan/grc-system

echo -e "${BLUE}Starting Zookeeper...${NC}"
docker-compose up -d zookeeper
sleep 5

echo -e "${BLUE}Starting Kafka...${NC}"
docker-compose up -d kafka
sleep 10

echo -e "${BLUE}Starting Kafka UI...${NC}"
docker-compose up -d kafka-ui
sleep 3

echo -e "${BLUE}Creating Camunda database...${NC}"
PGPASSWORD=postgres psql -h localhost -U postgres -c "CREATE DATABASE camunda;" 2>/dev/null || echo "Database already exists"

echo -e "${BLUE}Starting Camunda...${NC}"
docker-compose up -d camunda
sleep 15

echo -e "${BLUE}Starting Redis...${NC}"
docker-compose up -d redis

echo ""
echo "==========================================="
echo -e "${GREEN}✅ Infrastructure Services Started!${NC}"
echo "==========================================="
echo ""
echo "📊 Service URLs:"
echo "  • Camunda:   http://localhost:8080/camunda"
echo "  • Kafka UI:  http://localhost:9080"
echo "  • Kafka:     localhost:9092"
echo "  • Zookeeper: localhost:2181"
echo "  • Redis:     localhost:6379"
echo ""
echo "📝 Camunda Default Credentials:"
echo "  • Username: demo"
echo "  • Password: demo"
echo ""
echo "==========================================="

# Check health
echo ""
echo "🔍 Checking service health..."
echo ""

# Check Camunda
if curl -s http://localhost:8080/camunda/ > /dev/null 2>&1; then
    echo -e "  ✅ Camunda: ${GREEN}Running${NC}"
else
    echo "  ⏳ Camunda: Starting... (may take 30-60 seconds)"
fi

# Check Kafka
if docker exec grc-kafka kafka-broker-api-versions --bootstrap-server localhost:9092 > /dev/null 2>&1; then
    echo -e "  ✅ Kafka: ${GREEN}Running${NC}"
else
    echo "  ⏳ Kafka: Starting..."
fi

# Check Redis
if docker exec grc-redis redis-cli ping > /dev/null 2>&1; then
    echo -e "  ✅ Redis: ${GREEN}Running${NC}"
else
    echo "  ⏳ Redis: Starting..."
fi

echo ""
echo "==========================================="
