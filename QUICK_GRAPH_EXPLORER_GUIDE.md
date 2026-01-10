# ⚡ Quick Guide: Create Webhook in Graph Explorer

**For**: Creating Microsoft Graph webhook subscription to enable email auto-reply

---

## 🚀 Quick Steps (5 minutes)

### 1. Open Graph Explorer
Go to: **https://developer.microsoft.com/graph/graph-explorer**
- Click **"Sign in"** (top right)
- Sign in with `info@doganconsult.com` or admin account

---

### 2. Set Up the Request

**Change these in Graph Explorer:**

1. **HTTP Method**: Change dropdown to **`POST`**

2. **API Endpoint**: Change to:
   ```
   https://graph.microsoft.com/v1.0/subscriptions
   ```

3. **Request Body Tab**: Click **"Request Body"** tab, then paste this:

```json
{
  "changeType": "created",
  "notificationUrl": "https://portal.shahin-ai.com/api/webhooks/email",
  "resource": "/users/info@doganconsult.com/mailFolders/inbox/messages",
  "expirationDateTime": "2026-02-22T00:00:00Z",
  "clientState": "grc-email-webhook-2026"
}
```

---

### 3. Run the Query

Click the blue **"Run query"** button (top right corner)

---

### 4. Get the Subscription ID

**If Success** (200 OK):
- Look for `"id"` in the response
- Copy that ID (looks like: `abc123-def456-ghi789`)
- This is your `WebhookSubscriptionId`

**Example Response**:
```json
{
  "id": "7f105c7d-71cd-4af6-b036-0138a8b3c45a",
  "resource": "/users/info@doganconsult.com/mailFolders/inbox/messages",
  ...
}
```

**Copy the `id` value!**

---

### 5. Update Database

**Option A: Edit the SQL file**

1. Open: `update_subscription_id.sql`
2. Replace `YOUR_SUBSCRIPTION_ID_HERE` with the ID you copied
3. Run:
```bash
docker exec -i a9391f4add8a_grc-db psql -U postgres -d GrcMvcDb < update_subscription_id.sql
```

**Option B: Direct command**

```bash
docker exec -i a9391f4add8a_grc-db psql -U postgres -d GrcMvcDb -c "
UPDATE \"EmailMailboxes\"
SET 
    \"WebhookSubscriptionId\" = 'PASTE_YOUR_ID_HERE',
    \"WebhookExpiresAt\" = '2026-02-22T00:00:00Z',
    \"ModifiedDate\" = NOW()
WHERE \"EmailAddress\" = 'info@doganconsult.com';
"
```

---

## ⚠️ Important Notes

### If You Get "ValidationError - Unable to connect":

**This means**: Your webhook URL (`https://portal.shahin-ai.com/api/webhooks/email`) is not publicly accessible.

**Solutions**:
1. **Deploy app** to a public URL first
2. **Use polling mode** instead (check alternative below)
3. **Use ngrok** for testing: `ngrok http 8888` → use the ngrok URL

---

## 🔄 Alternative: Polling Mode (If Webhook URL Not Accessible)

If webhooks don't work, use email polling instead:

The system already has a sync method - you just need to call it periodically via API:

```bash
# Sync mailbox manually (can be called via cron or scheduled job)
curl -X POST "https://your-app/api/email/mailboxes/sync-all" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

This will check for new emails and process them with auto-reply rules.

---

## ✅ After Setup

1. ✅ Subscription created in Graph Explorer
2. ✅ Database updated with subscription ID
3. 🧪 **Test**: Send email to `info@doganconsult.com`
4. ✅ **Verify**: Check if auto-reply was sent

---

## 📋 Summary

**What you're doing**:
- Creating a subscription in Microsoft Graph
- This tells Microsoft: "When new email arrives at info@doganconsult.com, notify my app"
- Your app processes the email and sends auto-reply

**Current Status**:
- ✅ Auto-reply rules: Configured (6 rules)
- ✅ Mailbox: Configured
- ⏳ Webhook subscription: **Need to create this**
- ⏳ Email processing: Will work after subscription created

---

**Follow the steps above to complete the setup!** 🚀
