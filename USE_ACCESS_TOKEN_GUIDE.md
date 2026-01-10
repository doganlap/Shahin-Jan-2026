# 🔑 Using Access Token to Create Webhook Subscription

You have an access token from Microsoft Graph Explorer. Here's how to use it:

---

## 🚀 Quick Method: Use the Script

```bash
cd /home/Shahin-ai/Shahin-Jan-2026
./create_webhook_with_token.sh "YOUR_ACCESS_TOKEN_HERE"
```

**Example**:
```bash
./create_webhook_with_token.sh "eyJ0eXAiOiJKV1QiLCJub25jZSI6IjRldUliM1pkUU9fREdzX1JaRm9jbjVUZ3UxVXBUVWd5dnhTM3JZRUlWT0EiLCJhbGciOiJSUzI1NiIsIng1dCI6IlBjWDk4R1g0MjBUMVg2c0JEa3poUW1xZ3dNVSIsImtpZCI6IlBjWDk4R1g0MjBUMVg2c0JEa3poUW1xZ3dNVSJ9..."
```

---

## 📋 Method 2: Direct API Call

### Using curl:

```bash
ACCESS_TOKEN="YOUR_TOKEN_HERE"
MAILBOX="info@doganconsult.com"
WEBHOOK_URL="https://portal.shahin-ai.com/api/webhooks/email"
EXPIRATION="2026-02-22T00:00:00Z"

curl -X POST "https://graph.microsoft.com/v1.0/subscriptions" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d "{
    \"changeType\": \"created\",
    \"notificationUrl\": \"$WEBHOOK_URL\",
    \"resource\": \"/users/$MAILBOX/mailFolders/inbox/messages\",
    \"expirationDateTime\": \"$EXPIRATION\",
    \"clientState\": \"grc-email-webhook-2026\"
  }"
```

---

## 📋 Method 3: Using Microsoft Graph Explorer

1. **Copy your token** (it's in the "Access token" tab)
2. Go to Graph Explorer
3. Set method to **POST**
4. Set endpoint to: `https://graph.microsoft.com/v1.0/subscriptions`
5. In **Request Body** tab, paste:
```json
{
  "changeType": "created",
  "notificationUrl": "https://portal.shahin-ai.com/api/webhooks/email",
  "resource": "/users/info@doganconsult.com/mailFolders/inbox/messages",
  "expirationDateTime": "2026-02-22T00:00:00Z",
  "clientState": "grc-email-webhook-2026"
}
```
6. Click **Run query**
7. Copy the `id` from response
8. Update database (see below)

---

## 💾 Update Database with Subscription ID

After getting the subscription ID, update your database:

```bash
docker exec -i a9391f4add8a_grc-db psql -U postgres -d GrcMvcDb -c "
UPDATE \"EmailMailboxes\"
SET 
    \"WebhookSubscriptionId\" = 'YOUR_SUBSCRIPTION_ID',
    \"WebhookExpiresAt\" = '2026-02-22T00:00:00Z',
    \"ModifiedDate\" = NOW()
WHERE \"EmailAddress\" = 'info@doganconsult.com';
"
```

---

## ⚠️ Important Notes

### Token Expiration

- **User tokens** (like from Graph Explorer) expire in ~1 hour
- **Application tokens** (from your app) can last longer
- Webhook subscriptions need to be renewed before expiration

### Permissions Required

Your token needs:
- ✅ `Mail.Read` or `Mail.ReadBasic` (Delegated)
- ✅ Or `Mail.Read.All` (Application)

### Webhook URL Requirements

- Must be **HTTPS**
- Must be **publicly accessible**
- Must return **202 Accepted** for validation requests

---

## 🔄 Token Refresh (For Production)

For production, use **Application credentials** (Client Credentials flow):

```csharp
var credential = new ClientSecretCredential(
    tenantId,
    clientId,
    clientSecret);

var token = await credential.GetTokenAsync(
    new TokenRequestContext(new[] { "https://graph.microsoft.com/.default" }));
```

---

## ✅ Verification

After creating subscription:

1. **Check database**:
```sql
SELECT "EmailAddress", "WebhookSubscriptionId", "WebhookExpiresAt"
FROM "EmailMailboxes"
WHERE "EmailAddress" = 'info@doganconsult.com';
```

2. **Test**: Send email to `info@doganconsult.com`
3. **Check logs**: Application should receive webhook notification
4. **Verify**: Auto-reply should be sent

---

## 🎯 Next Steps

1. ✅ Create subscription with your token
2. ✅ Update database with subscription ID
3. ✅ Test with sample email
4. ✅ Monitor webhook expiration (renew before expiry)

---

**Ready to run? Execute:**
```bash
./create_webhook_with_token.sh "YOUR_TOKEN"
```
