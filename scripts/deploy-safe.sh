#!/bin/bash
# ══════════════════════════════════════════════════════════════
# GRC SaaS Safe Deployment Script
# Always runs quality check before deployment
# ══════════════════════════════════════════════════════════════

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${BLUE}══════════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}   GRC SaaS Safe Deployment${NC}"
echo -e "${BLUE}══════════════════════════════════════════════════════════════${NC}"
echo ""

# ─────────────────────────────────────────────────────────────
# STEP 1: Quality Check
# ─────────────────────────────────────────────────────────────
echo -e "${YELLOW}Step 1: Running Quality Check...${NC}"
echo ""

if ! "$SCRIPT_DIR/quality-check.sh"; then
    echo ""
    echo -e "${RED}❌ Quality check failed! Deployment aborted.${NC}"
    echo -e "${RED}   Fix the issues and try again.${NC}"
    exit 1
fi

echo ""

# ─────────────────────────────────────────────────────────────
# STEP 2: Backup Current Version
# ─────────────────────────────────────────────────────────────
echo -e "${YELLOW}Step 2: Creating Backup...${NC}"

BACKUP_DIR="$PROJECT_ROOT/backups"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
mkdir -p "$BACKUP_DIR"

if [ -d "$PROJECT_ROOT/publish" ]; then
    cp -r "$PROJECT_ROOT/publish" "$BACKUP_DIR/publish_$TIMESTAMP"
    echo -e "${GREEN}   ✅ Backup created: backups/publish_$TIMESTAMP${NC}"
else
    echo -e "${YELLOW}   ⚠️  No previous deployment to backup${NC}"
fi

echo ""

# ─────────────────────────────────────────────────────────────
# STEP 3: Stop Current Application
# ─────────────────────────────────────────────────────────────
echo -e "${YELLOW}Step 3: Stopping Current Application...${NC}"

pkill -f "dotnet.*GrcMvc" 2>/dev/null || true
sleep 3
echo -e "${GREEN}   ✅ Application stopped${NC}"
echo ""

# ─────────────────────────────────────────────────────────────
# STEP 4: Build & Publish
# ─────────────────────────────────────────────────────────────
echo -e "${YELLOW}Step 4: Building & Publishing...${NC}"

cd "$PROJECT_ROOT/src/GrcMvc"
dotnet publish -c Release -o "$PROJECT_ROOT/publish" --no-restore 2>&1 | tail -3

echo -e "${GREEN}   ✅ Build complete${NC}"
echo ""

# ─────────────────────────────────────────────────────────────
# STEP 5: Start Application
# ─────────────────────────────────────────────────────────────
echo -e "${YELLOW}Step 5: Starting Application...${NC}"

cd "$PROJECT_ROOT/publish"
ASPNETCORE_ENVIRONMENT=Production nohup dotnet GrcMvc.dll --urls "http://0.0.0.0:5010" > /tmp/grcmvc.log 2>&1 &
sleep 15

echo -e "${GREEN}   ✅ Application started${NC}"
echo ""

# ─────────────────────────────────────────────────────────────
# STEP 6: Health Check
# ─────────────────────────────────────────────────────────────
echo -e "${YELLOW}Step 6: Health Check...${NC}"

HEALTH_STATUS=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:5010/health 2>/dev/null || echo "000")

if [ "$HEALTH_STATUS" == "200" ]; then
    echo -e "${GREEN}   ✅ Health check passed (HTTP $HEALTH_STATUS)${NC}"
else
    echo -e "${RED}   ❌ Health check failed (HTTP $HEALTH_STATUS)${NC}"
    echo -e "${RED}   Rolling back...${NC}"
    
    # Rollback
    pkill -f "dotnet.*GrcMvc" 2>/dev/null || true
    if [ -d "$BACKUP_DIR/publish_$TIMESTAMP" ]; then
        rm -rf "$PROJECT_ROOT/publish"
        mv "$BACKUP_DIR/publish_$TIMESTAMP" "$PROJECT_ROOT/publish"
        cd "$PROJECT_ROOT/publish"
        ASPNETCORE_ENVIRONMENT=Production nohup dotnet GrcMvc.dll --urls "http://0.0.0.0:5010" > /tmp/grcmvc.log 2>&1 &
        echo -e "${YELLOW}   ⚠️  Rolled back to previous version${NC}"
    fi
    exit 1
fi

echo ""

# ─────────────────────────────────────────────────────────────
# STEP 7: Verify Endpoints
# ─────────────────────────────────────────────────────────────
echo -e "${YELLOW}Step 7: Verifying Endpoints...${NC}"

ENDPOINTS=(
    "http://localhost:5010/health"
    "https://shahin-ai.com/"
    "https://portal.shahin-ai.com/"
    "https://login.shahin-ai.com/admin/login"
)

ALL_OK=true
for URL in "${ENDPOINTS[@]}"; do
    STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$URL" 2>/dev/null || echo "000")
    if [ "$STATUS" == "200" ]; then
        echo -e "${GREEN}   ✅ $URL: $STATUS${NC}"
    else
        echo -e "${RED}   ❌ $URL: $STATUS${NC}"
        ALL_OK=false
    fi
done

echo ""

# ─────────────────────────────────────────────────────────────
# COMPLETE
# ─────────────────────────────────────────────────────────────
if [ "$ALL_OK" = true ]; then
    echo -e "${GREEN}══════════════════════════════════════════════════════════════${NC}"
    echo -e "${GREEN}   ✅ DEPLOYMENT SUCCESSFUL${NC}"
    echo -e "${GREEN}══════════════════════════════════════════════════════════════${NC}"
    echo ""
    echo "   Version: 1.0.$(date +%Y%m%d%H%M)"
    echo "   Time: $(date)"
    echo ""
else
    echo -e "${YELLOW}══════════════════════════════════════════════════════════════${NC}"
    echo -e "${YELLOW}   ⚠️  DEPLOYMENT COMPLETE WITH WARNINGS${NC}"
    echo -e "${YELLOW}══════════════════════════════════════════════════════════════${NC}"
fi
