#!/bin/bash
# Database Monitoring Script
# Checks database container, connectivity, and application health

set -euo pipefail

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

PASSED=0
FAILED=0

check() {
    local name=$1
    shift
    if "$@"; then
        echo -e "${GREEN}✓${NC} $name"
        ((PASSED++))
        return 0
    else
        echo -e "${RED}✗${NC} $name"
        ((FAILED++))
        return 1
    fi
}

echo "=== Database Health Check ==="
echo

# Check container status
if ! check "Container grc-db is running" docker ps | grep -q "grc-db"; then
    echo "  Container status:"
    docker ps -a | grep grc-db || echo "  Container not found"
    exit 1
fi

# Check database connectivity
check "Database is ready" docker exec grc-db pg_isready -U postgres > /dev/null 2>&1

# Check both databases exist
check "GrcMvcDb exists" docker exec grc-db psql -U postgres -lqt | cut -d \| -f 1 | grep -qw GrcMvcDb
check "GrcAuthDb exists" docker exec grc-db psql -U postgres -lqt | cut -d \| -f 1 | grep -qw GrcAuthDb

# Check database sizes
echo
echo "=== Database Sizes ==="
docker exec grc-db psql -U postgres -c "SELECT datname, pg_size_pretty(pg_database_size(datname)) as size FROM pg_database WHERE datistemplate = false ORDER BY datname;" 2>/dev/null || true

# Check application health (if running)
if docker ps | grep -q "grcmvc\|grc-system-grcmvc"; then
    echo
    echo "=== Application Health ==="
    if check "Application health endpoint" curl -sf http://localhost:8888/health > /dev/null 2>&1; then
        echo "  Health check response:"
        curl -s http://localhost:8888/health | jq '.' 2>/dev/null || curl -s http://localhost:8888/health
    fi
fi

# Check network connectivity
echo
echo "=== Network Status ==="
if docker network inspect grc-system_grc-network > /dev/null 2>&1; then
    echo -e "${GREEN}✓${NC} Network grc-system_grc-network exists"
    echo "  Containers on network:"
    docker network inspect grc-system_grc-network --format '  - {{.Name}}: {{.IPv4Address}}' 2>/dev/null || true
else
    echo -e "${YELLOW}⚠${NC} Network grc-system_grc-network not found"
fi

# Summary
echo
echo "=== Summary ==="
echo "Passed: $PASSED"
if [ $FAILED -gt 0 ]; then
    echo -e "Failed: ${RED}$FAILED${NC}"
    exit 1
else
    echo -e "Failed: ${GREEN}$FAILED${NC}"
    exit 0
fi
