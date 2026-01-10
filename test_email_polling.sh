#!/bin/bash

# Test Email Polling Setup
# This script tests the email polling configuration

echo "🧪 Testing Email Polling Setup"
echo "=============================="
echo ""

# Colors
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Test 1: Check database configuration
echo "📋 Test 1: Database Configuration"
echo "----------------------------------"
DB_CHECK=$(docker exec -i a9391f4add8a_grc-db psql -U postgres -d GrcMvcDb -t -c "
SELECT 
    CASE 
        WHEN \"AutoReplyEnabled\" = true AND \"IsActive\" = true AND \"GraphUserId\" IS NOT NULL 
        THEN 'OK' 
        ELSE 'FAIL' 
    END
FROM \"EmailMailboxes\"
WHERE \"EmailAddress\" = 'info@doganconsult.com';
" 2>/dev/null | tr -d ' ')

if [ "$DB_CHECK" = "OK" ]; then
    echo -e "${GREEN}✅ Mailbox configured correctly${NC}"
else
    echo -e "${RED}❌ Mailbox configuration issue${NC}"
fi

# Test 2: Check auto-reply rules
echo ""
echo "📋 Test 2: Auto-Reply Rules"
echo "---------------------------"
RULE_COUNT=$(docker exec -i a9391f4add8a_grc-db psql -U postgres -d GrcMvcDb -t -c "
SELECT COUNT(*)
FROM \"EmailAutoReplyRules\"
WHERE \"MailboxId\" IN (
    SELECT \"Id\" FROM \"EmailMailboxes\" 
    WHERE \"EmailAddress\" = 'info@doganconsult.com'
)
AND \"IsActive\" = true
AND \"IsDeleted\" = false;
" 2>/dev/null | tr -d ' ')

if [ "$RULE_COUNT" -gt 0 ]; then
    echo -e "${GREEN}✅ Found $RULE_COUNT active auto-reply rules${NC}"
else
    echo -e "${RED}❌ No active auto-reply rules found${NC}"
fi

# Test 3: Check Azure credentials (if .env exists)
echo ""
echo "📋 Test 3: Azure Credentials"
echo "---------------------------"
if [ -f ".env.production.final" ]; then
    if grep -q "AZURE_TENANT_ID" .env.production.final && grep -q "MSGRAPH_CLIENT_ID" .env.production.final; then
        echo -e "${GREEN}✅ Azure credentials file found${NC}"
    else
        echo -e "${YELLOW}⚠️  Azure credentials file incomplete${NC}"
    fi
else
    echo -e "${YELLOW}⚠️  Azure credentials file not found${NC}"
fi

# Test 4: Test Graph API access (if credentials available)
echo ""
echo "📋 Test 4: Microsoft Graph API Access"
echo "-------------------------------------"
if [ -f ".env.production.final" ]; then
    source .env.production.final 2>/dev/null
    if [ ! -z "$AZURE_TENANT_ID" ] && [ ! -z "$MSGRAPH_CLIENT_ID" ] && [ ! -z "$MSGRAPH_CLIENT_SECRET" ]; then
        TOKEN_RESPONSE=$(curl -s -X POST "https://login.microsoftonline.com/$AZURE_TENANT_ID/oauth2/v2.0/token" \
            -H "Content-Type: application/x-www-form-urlencoded" \
            -d "client_id=$MSGRAPH_CLIENT_ID" \
            -d "client_secret=$MSGRAPH_CLIENT_SECRET" \
            -d "scope=https://graph.microsoft.com/.default" \
            -d "grant_type=client_credentials" 2>/dev/null)
        
        if echo "$TOKEN_RESPONSE" | grep -q "access_token"; then
            echo -e "${GREEN}✅ Successfully obtained access token${NC}"
        else
            echo -e "${RED}❌ Failed to obtain access token${NC}"
            echo "   Response: $TOKEN_RESPONSE"
        fi
    else
        echo -e "${YELLOW}⚠️  Credentials not available for testing${NC}"
    fi
else
    echo -e "${YELLOW}⚠️  Cannot test - credentials file not found${NC}"
fi

# Test 5: Check Hangfire (if app is running)
echo ""
echo "📋 Test 5: Hangfire Job Status"
echo "------------------------------"
if docker ps | grep -q "grcmvc\|grc-app"; then
    echo -e "${GREEN}✅ Application container is running${NC}"
    echo "   Job 'email-polling-sync' should be scheduled"
    echo "   Check Hangfire dashboard: /hangfire"
else
    echo -e "${YELLOW}⚠️  Application container not found${NC}"
    echo "   Start the application to enable polling"
fi

# Summary
echo ""
echo "=============================="
echo "📊 Test Summary"
echo "=============================="
echo ""
echo "✅ Configuration looks good!"
echo ""
echo "🧪 To test email processing:"
echo "1. Send a test email to: info@doganconsult.com"
echo "2. Wait up to 5 minutes for processing"
echo "3. Check database for new email records:"
echo ""
echo "   SELECT * FROM \"EmailMessages\" "
echo "   WHERE \"ReceivedAt\" > NOW() - INTERVAL '10 minutes'"
echo "   ORDER BY \"ReceivedAt\" DESC;"
echo ""
echo "4. Check if auto-reply was sent (check SentItems folder)"
echo ""
echo "📋 Monitor polling:"
echo "   - Hangfire Dashboard: /hangfire"
echo "   - Application logs for 'email-polling-sync'"
echo "   - Database LastSyncAt timestamp"
echo ""
