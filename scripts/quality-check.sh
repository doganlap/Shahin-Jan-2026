#!/bin/bash
# ══════════════════════════════════════════════════════════════
# GRC SaaS Quality Check Script
# Run before every deployment
# ══════════════════════════════════════════════════════════════

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
REPORT_DIR="$PROJECT_ROOT/quality-reports"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${BLUE}══════════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}   GRC SaaS Quality Gate - Pre-Deployment Check${NC}"
echo -e "${BLUE}══════════════════════════════════════════════════════════════${NC}"
echo ""

# Initialize scores
BUILD_SCORE=0
TEST_SCORE=15
SECURITY_SCORE=25
CODE_SCORE=25
TOTAL_SCORE=0

mkdir -p "$REPORT_DIR"

# ─────────────────────────────────────────────────────────────
# 1. BUILD CHECK (25 points)
# ─────────────────────────────────────────────────────────────
echo -e "${YELLOW}[1/4] 🏗️  Build Check...${NC}"
cd "$PROJECT_ROOT/src/GrcMvc"

if dotnet build --configuration Release --no-restore > /tmp/build_output.txt 2>&1; then
    BUILD_SCORE=25
    echo -e "${GREEN}   ✅ Build: PASSED${NC}"
else
    BUILD_SCORE=0
    echo -e "${RED}   ❌ Build: FAILED${NC}"
    cat /tmp/build_output.txt | grep -i "error" | head -5
fi

BUILD_WARNINGS=$(grep -c "warning" /tmp/build_output.txt 2>/dev/null || echo 0)
if [ "$BUILD_WARNINGS" -gt 0 ]; then
    echo -e "${YELLOW}   ⚠️  Warnings: $BUILD_WARNINGS${NC}"
fi

# ─────────────────────────────────────────────────────────────
# 2. TEST CHECK (25 points) - Skip if no tests
# ─────────────────────────────────────────────────────────────
echo -e "${YELLOW}[2/4] 🧪 Test Check...${NC}"

if [ -d "$PROJECT_ROOT/tests" ] && [ "$(ls -A $PROJECT_ROOT/tests 2>/dev/null)" ]; then
    if dotnet test "$PROJECT_ROOT/tests/" --no-build > /tmp/test_output.txt 2>&1; then
        TEST_SCORE=25
        echo -e "${GREEN}   ✅ Tests: PASSED${NC}"
    else
        TEST_SCORE=10
        echo -e "${YELLOW}   ⚠️  Some tests failed${NC}"
    fi
else
    TEST_SCORE=15
    echo -e "${YELLOW}   ⚠️  No tests found (partial credit)${NC}"
fi

# ─────────────────────────────────────────────────────────────
# 3. SECURITY CHECK (25 points)
# ─────────────────────────────────────────────────────────────
echo -e "${YELLOW}[3/4] 🔒 Security Check...${NC}"

SECURITY_ISSUES=0

# Check for hardcoded connection strings with passwords
if grep -rn "Password=" "$PROJECT_ROOT/src/" --include="*.cs" 2>/dev/null | grep -v "Configuration\[" | grep -v "string.Empty" | grep -v "// " | head -1 > /dev/null 2>&1; then
    echo -e "${YELLOW}   ⚠️  Check connection string handling${NC}"
    SECURITY_ISSUES=$((SECURITY_ISSUES + 1))
fi

# Check for exposed API keys
if grep -rn "sk-ant-api" "$PROJECT_ROOT/src/" --include="*.cs" 2>/dev/null | head -1 > /dev/null 2>&1; then
    echo -e "${RED}   ❌ API keys in source code!${NC}"
    SECURITY_ISSUES=$((SECURITY_ISSUES + 2))
fi

SECURITY_SCORE=$((25 - SECURITY_ISSUES * 5))
if [ $SECURITY_SCORE -lt 0 ]; then SECURITY_SCORE=0; fi

if [ "$SECURITY_ISSUES" -eq 0 ]; then
    echo -e "${GREEN}   ✅ Security: PASSED${NC}"
fi

# ─────────────────────────────────────────────────────────────
# 4. CODE QUALITY CHECK (25 points)
# ─────────────────────────────────────────────────────────────
echo -e "${YELLOW}[4/4] 📊 Code Quality Check...${NC}"

CODE_ISSUES=0

# Check for TODO count
TODO_COUNT=$(grep -r "TODO\|FIXME" "$PROJECT_ROOT/src/" --include="*.cs" 2>/dev/null | wc -l)
if [ "$TODO_COUNT" -gt 20 ]; then
    echo -e "${YELLOW}   ⚠️  High TODO count: $TODO_COUNT${NC}"
    CODE_ISSUES=$((CODE_ISSUES + 1))
fi

CODE_SCORE=$((25 - CODE_ISSUES * 5))
if [ $CODE_SCORE -lt 0 ]; then CODE_SCORE=0; fi

if [ "$CODE_ISSUES" -eq 0 ]; then
    echo -e "${GREEN}   ✅ Code Quality: PASSED${NC}"
fi

# ─────────────────────────────────────────────────────────────
# FINAL SCORE
# ─────────────────────────────────────────────────────────────
TOTAL_SCORE=$((BUILD_SCORE + TEST_SCORE + SECURITY_SCORE + CODE_SCORE))

echo ""
echo -e "${BLUE}══════════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}   QUALITY REPORT${NC}"
echo -e "${BLUE}══════════════════════════════════════════════════════════════${NC}"
echo ""
echo "   Build Score:     $BUILD_SCORE/25"
echo "   Test Score:      $TEST_SCORE/25"
echo "   Security Score:  $SECURITY_SCORE/25"
echo "   Code Score:      $CODE_SCORE/25"
echo "   ─────────────────────────"
echo -e "   ${BLUE}TOTAL SCORE:    $TOTAL_SCORE/100${NC}"
echo ""

# Generate JSON report
cat > "$REPORT_DIR/quality-report-$TIMESTAMP.json" << EOF
{
  "timestamp": "$(date -u +%Y-%m-%dT%H:%M:%SZ)",
  "scores": {
    "build": $BUILD_SCORE,
    "test": $TEST_SCORE,
    "security": $SECURITY_SCORE,
    "codeQuality": $CODE_SCORE,
    "total": $TOTAL_SCORE
  },
  "deploymentAllowed": $([ $TOTAL_SCORE -ge 70 ] && echo "true" || echo "false")
}
EOF

echo "   Report: quality-reports/quality-report-$TIMESTAMP.json"
echo ""

# Deployment decision
if [ $TOTAL_SCORE -ge 70 ]; then
    echo -e "${GREEN}══════════════════════════════════════════════════════════════${NC}"
    echo -e "${GREEN}   ✅ DEPLOYMENT ALLOWED (Score: $TOTAL_SCORE/100)${NC}"
    echo -e "${GREEN}══════════════════════════════════════════════════════════════${NC}"
    exit 0
else
    echo -e "${RED}══════════════════════════════════════════════════════════════${NC}"
    echo -e "${RED}   ❌ DEPLOYMENT BLOCKED (Score: $TOTAL_SCORE/100)${NC}"
    echo -e "${RED}   Minimum required: 70/100${NC}"
    echo -e "${RED}══════════════════════════════════════════════════════════════${NC}"
    exit 1
fi
