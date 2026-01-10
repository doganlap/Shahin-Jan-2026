# 📧 Email Service: Current Status & How It Works

## 🔍 Answer to Your Question

**Q**: Is the service running now or will it run automatically after production?

**A**: ⚠️ **The service is CONFIGURED but NOT ACTIVELY PROCESSING emails yet** because:
1. ✅ Auto-reply rules are configured
2. ✅ Mailbox is set up
3. ❌ **Webhook subscription is missing** - Microsoft Graph doesn't know to notify your app when emails arrive

---

## 🔄 How Email Processing Works

### Current Architecture:

```
┌─────────────────────────────────────────────────────────────┐
│  Email arrives at info@doganconsult.com (Microsoft 365)    │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│  Microsoft Graph detects new email                          │
│  └─ Sends webhook notification (REQUIRES SUBSCRIPTION)      │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│  Your App receives webhook at:                              │
│  https://portal.shahin-ai.com/api/webhooks/email            │
│  └─ EmailWebhookController processes it                     │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│  Hangfire Background Job (EmailProcessingJob)               │
│  └─ Fetches email from Graph API                            │
│  └─ Creates thread/message in database                      │
│  └─ Classifies email (AI)                                   │
│  └─ Processes auto-reply rules                              │
│  └─ Sends auto-reply if rule matches                        │
└─────────────────────────────────────────────────────────────┘
```

**Problem**: Step 2-3 is broken (no webhook subscription)

---

## ✅ What's Already Configured

| Component | Status | Notes |
|-----------|--------|-------|
| **Auto-Reply Rules** | ✅ Configured | 6 rules active |
| **Mailbox** | ✅ Configured | info@doganconsult.com |
| **Auto-Reply Enabled** | ✅ Enabled | true |
| **GraphUserId** | ✅ Set | info@doganconsult.com |
| **Webhook Subscription** | ❌ **MISSING** | **This is blocking!** |
| **Application** | ⚠️ Unknown | Need to check if running |

---

## 🚨 Why Your Test Email Wasn't Processed

**You sent a test email but received no reply because:**

1. ✅ Email arrived at `info@doganconsult.com`
2. ❌ Microsoft Graph doesn't have a webhook subscription for your app
3. ❌ Your app was never notified about the new email
4. ❌ Email was never processed
5. ❌ Auto-reply rules were never evaluated
6. ❌ No reply was sent

---

## 🔧 Solution: Create Webhook Subscription

### Option 1: Use the Script (Automated)

```bash
cd /home/Shahin-ai/Shahin-Jan-2026
./create_webhook_subscription.sh
```

This script will:
- Get access token from Azure
- Create webhook subscription in Microsoft Graph
- Update database with subscription ID
- Verify setup

### Option 2: Manual Setup via Microsoft Graph Explorer

1. Go to: https://developer.microsoft.com/graph/graph-explorer
2. Sign in with admin account for `doganconsult.com`
3. Run this:

```http
POST https://graph.microsoft.com/v1.0/subscriptions
Content-Type: application/json
Authorization: Bearer {YOUR_TOKEN}

{
  "changeType": "created",
  "notificationUrl": "https://portal.shahin-ai.com/api/webhooks/email",
  "resource": "/users/info@doganconsult.com/mailFolders/inbox/messages",
  "expirationDateTime": "2026-02-22T00:00:00Z",
  "clientState": "grc-email-webhook-2026"
}
```

4. Copy the `id` from response
5. Update database:
```sql
UPDATE "EmailMailboxes"
SET 
    "WebhookSubscriptionId" = '{id-from-step-4}',
    "WebhookExpiresAt" = '2026-02-22T00:00:00Z'
WHERE "EmailAddress" = 'info@doganconsult.com';
```

### Option 3: Use Application API (If Running)

```bash
# Get mailbox ID first
curl https://your-app/api/email/mailboxes

# Create subscription
curl -X POST "https://your-app/api/email/subscriptions/create/{mailboxId}" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

## ⚠️ Important Requirements

### 1. Webhook URL Must Be Publicly Accessible

Your webhook URL `https://portal.shahin-ai.com/api/webhooks/email` must:
- ✅ Be accessible from the internet (HTTPS)
- ✅ Accept POST requests
- ✅ Return 202 Accepted for validation requests
- ✅ Not require authentication (or Microsoft won't be able to call it)

### 2. Application Must Be Running

The application must be:
- ✅ Running and accessible
- ✅ Webhook endpoint active: `/api/webhooks/email`
- ✅ Hangfire background jobs enabled

### 3. Microsoft Graph Permissions

Your Azure App must have:
- ✅ `Mail.Read` (Application permission)
- ✅ `Mail.ReadBasic` (Application permission)
- ✅ Admin consent granted

---

## 🧪 Testing After Setup

1. **Create webhook subscription** (using one of the methods above)
2. **Wait 1-2 minutes** (give Microsoft Graph time to register)
3. **Send test email** to `info@doganconsult.com`
4. **Check application logs** for:
   - Webhook received
   - Email processed
   - Auto-reply sent
5. **Check your inbox** for the auto-reply

---

## 📊 Current Database Status

```
Email: info@doganconsult.com
GraphUserId: ✅ info@doganconsult.com (JUST UPDATED)
WebhookSubscriptionId: ❌ NULL (NEEDS TO BE SET)
WebhookExpiresAt: ❌ NULL (NEEDS TO BE SET)
AutoReplyEnabled: ✅ true
IsActive: ✅ true
```

---

## 🎯 Next Steps

1. ✅ **DONE**: GraphUserId updated
2. ⏳ **TODO**: Create webhook subscription
3. ⏳ **TODO**: Update database with subscription ID
4. ⏳ **TODO**: Verify webhook URL is publicly accessible
5. ⏳ **TODO**: Test with email

---

## 💡 Alternative: Polling Mode

If webhooks don't work (e.g., webhook URL not publicly accessible), you can use polling:

1. Create recurring Hangfire job
2. Runs every 5-10 minutes
3. Fetches new emails from Graph API
4. Processes them normally

**Less efficient but works without webhooks.**

Would you like me to:
- ✅ Run the webhook subscription script now?
- ✅ Set up polling mode as backup?
- ✅ Check if your application is running?
