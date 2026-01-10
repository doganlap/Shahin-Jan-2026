#!/bin/bash

# Script to create Microsoft Graph Webhook Subscription for Email Auto-Reply
# This must be run after the application is deployed and accessible

echo "🔧 Creating Microsoft Graph Webhook Subscription"
echo "================================================"
echo ""

# Configuration
TENANT_ID="c8847e8a-33a0-4b6c-8e01-2e0e6b4aaef5"
CLIENT_ID="4e2575c6-e269-48eb-b055-ad730a2150a7"
CLIENT_SECRET="Wx38Q~5VWvTmcizGb5qXNZREQyNp3yyzCUot.b5x"
MAILBOX_EMAIL="info@doganconsult.com"
WEBHOOK_URL="https://portal.shahin-ai.com/api/webhooks/email"

# Get access token
echo "📡 Getting access token..."
TOKEN_RESPONSE=$(curl -s -X POST "https://login.microsoftonline.com/$TENANT_ID/oauth2/v2.0/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=$CLIENT_ID" \
  -d "client_secret=$CLIENT_SECRET" \
  -d "scope=https://graph.microsoft.com/.default" \
  -d "grant_type=client_credentials")

ACCESS_TOKEN=$(echo "$TOKEN_RESPONSE" | grep -o '"access_token":"[^"]*' | cut -d'"' -f4)

if [ -z "$ACCESS_TOKEN" ]; then
    echo "❌ Failed to get access token"
    echo "Response: $TOKEN_RESPONSE"
    exit 1
fi

echo "✅ Access token obtained"
echo ""

# Calculate expiration (3 days from now)
EXPIRATION_DATE=$(date -u -d "+3 days" +"%Y-%m-%dT%H:%M:%SZ" 2>/dev/null || date -u -v+3d +"%Y-%m-%dT%H:%M:%SZ" 2>/dev/null || echo "2026-01-25T00:00:00Z")

# Create webhook subscription
echo "📧 Creating webhook subscription..."
echo "   Mailbox: $MAILBOX_EMAIL"
echo "   Webhook URL: $WEBHOOK_URL"
echo "   Expires: $EXPIRATION_DATE"
echo ""

SUBSCRIPTION_RESPONSE=$(curl -s -X POST "https://graph.microsoft.com/v1.0/subscriptions" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d "{
    \"changeType\": \"created\",
    \"notificationUrl\": \"$WEBHOOK_URL\",
    \"resource\": \"/users/$MAILBOX_EMAIL/mailFolders/inbox/messages\",
    \"expirationDateTime\": \"$EXPIRATION_DATE\",
    \"clientState\": \"grc-email-webhook-$(date +%s)\"
  }")

echo "Response:"
echo "$SUBSCRIPTION_RESPONSE" | python3 -m json.tool 2>/dev/null || echo "$SUBSCRIPTION_RESPONSE"
echo ""

# Extract subscription ID
SUBSCRIPTION_ID=$(echo "$SUBSCRIPTION_RESPONSE" | grep -o '"id":"[^"]*' | cut -d'"' -f4)

if [ -z "$SUBSCRIPTION_ID" ]; then
    echo "❌ Failed to create subscription"
    echo ""
    echo "Possible issues:"
    echo "- Webhook URL not publicly accessible"
    echo "- Insufficient permissions (need Mail.Read, Mail.ReadBasic)"
    echo "- Invalid mailbox email"
    exit 1
fi

echo "✅ Webhook subscription created!"
echo "   Subscription ID: $SUBSCRIPTION_ID"
echo ""

# Update database
echo "💾 Updating database..."
docker exec -i a9391f4add8a_grc-db psql -U postgres -d GrcMvcDb << SQL
UPDATE "EmailMailboxes"
SET 
    "WebhookSubscriptionId" = '$SUBSCRIPTION_ID',
    "WebhookExpiresAt" = '$EXPIRATION_DATE',
    "ModifiedDate" = NOW()
WHERE "EmailAddress" = '$MAILBOX_EMAIL';

SELECT 
    "EmailAddress",
    "GraphUserId",
    "WebhookSubscriptionId",
    "WebhookExpiresAt",
    "IsActive",
    "AutoReplyEnabled"
FROM "EmailMailboxes"
WHERE "EmailAddress" = '$MAILBOX_EMAIL';
SQL

echo ""
echo "✅ Setup complete!"
echo ""
echo "📋 Next steps:"
echo "1. Test by sending email to: $MAILBOX_EMAIL"
echo "2. Check application logs for webhook receipt"
echo "3. Verify auto-reply is sent"
echo ""
echo "⚠️  Note: Webhook subscription expires on: $EXPIRATION_DATE"
echo "   The system should auto-renew, but monitor expiration date."
