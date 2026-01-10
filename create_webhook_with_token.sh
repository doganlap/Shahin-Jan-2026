#!/bin/bash

# Create Webhook Subscription using Access Token from Graph Explorer
# Usage: ./create_webhook_with_token.sh YOUR_ACCESS_TOKEN

ACCESS_TOKEN="$1"

if [ -z "$ACCESS_TOKEN" ]; then
    echo "❌ Error: Access token required"
    echo "Usage: ./create_webhook_with_token.sh YOUR_ACCESS_TOKEN"
    exit 1
fi

MAILBOX_EMAIL="info@doganconsult.com"
WEBHOOK_URL="https://portal.shahin-ai.com/api/webhooks/email"

# Calculate expiration (3 days from now)
EXPIRATION_DATE=$(date -u -d "+3 days" +"%Y-%m-%dT%H:%M:%SZ" 2>/dev/null || date -u -v+3d +"%Y-%m-%dT%H:%M:%SZ" 2>/dev/null || echo "2026-02-22T00:00:00Z")

echo "🔧 Creating Microsoft Graph Webhook Subscription"
echo "================================================"
echo ""
echo "📧 Mailbox: $MAILBOX_EMAIL"
echo "🔗 Webhook URL: $WEBHOOK_URL"
echo "⏰ Expires: $EXPIRATION_DATE"
echo ""

# Create webhook subscription
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
SUBSCRIPTION_ID=$(echo "$SUBSCRIPTION_RESPONSE" | grep -o '"id":"[^"]*' | head -1 | cut -d'"' -f4)

if [ -z "$SUBSCRIPTION_ID" ]; then
    echo "❌ Failed to create subscription"
    echo ""
    echo "Possible issues:"
    echo "- Token expired or invalid"
    echo "- Webhook URL not publicly accessible"
    echo "- Insufficient permissions (need Mail.Read, Mail.ReadBasic)"
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
echo "⚠️  Note: Token will expire. Use app credentials for production."
