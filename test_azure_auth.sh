#!/bin/bash

TENANT_ID="c8847e8a-33a0-4b6c-8e01-2e0e6b4aaef5"
CLIENT_ID="4e2575c6-e269-48eb-b055-ad730a2150a7"
CLIENT_SECRET="Wx38Q~5VWvTmcizGb5qXNZREQyNp3yyzCUot.b5x"

echo "🔍 Testing Azure AD App Registration Authentication..."
echo ""

RESPONSE=$(curl -s -X POST "https://login.microsoftonline.com/$TENANT_ID/oauth2/v2.0/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=$CLIENT_ID" \
  -d "client_secret=$CLIENT_SECRET" \
  -d "scope=https://graph.microsoft.com/.default" \
  -d "grant_type=client_credentials")

if echo "$RESPONSE" | grep -q "access_token"; then
    echo "✅ SUCCESS: Authentication successful!"
    TOKEN=$(echo "$RESPONSE" | grep -o '"access_token":"[^"]*' | cut -d'"' -f4)
    echo "Token obtained (first 50 chars): ${TOKEN:0:50}..."
    echo ""
    echo "✅ Your Azure App Registration is correctly configured!"
else
    echo "❌ FAILED: Authentication failed"
    echo "Response: $RESPONSE"
    echo ""
    echo "Possible issues:"
    echo "- Client Secret is incorrect or expired"
    echo "- Client ID is incorrect"
    echo "- App registration is disabled"
    echo "- API permissions not granted"
fi
