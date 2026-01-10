# 📧 Email Service Status & Setup Guide

**Current Status**: ⚠️ **PARTIALLY CONFIGURED**

---

## 🔍 Current Status Check

### Mailbox Configuration:

```
Email: info@doganconsult.com
GraphUserId: NULL ❌ (REQUIRED)
WebhookSubscriptionId: NULL ❌ (REQUIRED)
WebhookExpiresAt: NULL ❌
AutoReplyEnabled: ✅ true
IsActive: ✅ true
```

**Problem**: The mailbox is missing critical configuration needed to receive emails!

---

## ❌ Why Your Test Email Wasn't Processed

1. **No GraphUserId**: The system can't identify which mailbox in Microsoft Graph
2. **No Webhook Subscription**: Microsoft Graph doesn't know where to send notifications when emails arrive
3. **No Email Processing**: Without webhooks, emails aren't processed automatically

---

## ✅ Required Setup Steps

### Step 1: Get Graph User ID for the Mailbox

The `GraphUserId` should be the email address itself (`info@doganconsult.com`) or the user's Object ID from Azure AD.

**To find it**:
1. Go to Azure Portal → Azure Active Directory → Users
2. Search for `info@doganconsult.com`
3. Copy the **Object ID** (GUID format)

**OR** just use the email address directly (most common).

### Step 2: Update Mailbox with Graph User ID

```sql
UPDATE "EmailMailboxes"
SET 
    "GraphUserId" = 'info@doganconsult.com',  -- Or use Object ID from Azure AD
    "ModifiedDate" = NOW()
WHERE "EmailAddress" = 'info@doganconsult.com';
```

### Step 3: Create Webhook Subscription

**This must be done via API or manually** - Microsoft Graph needs to register the webhook.

**Option A: Use API Endpoint** (if application is running):
```
POST https://your-app-url/api/email/subscriptions/create/{mailboxId}
```

**Option B: Use Microsoft Graph Explorer**:
1. Go to: https://developer.microsoft.com/graph/graph-explorer
2. Sign in with admin account
3. POST to: `https://graph.microsoft.com/v1.0/subscriptions`
4. Body:
```json
{
  "changeType": "created",
  "notificationUrl": "https://portal.shahin-ai.com/api/webhooks/email",
  "resource": "/users/info@doganconsult.com/mailFolders/inbox/messages",
  "expirationDateTime": "2026-02-22T00:00:00Z",
  "clientState": "secret-state-value"
}
```

---

## 🔧 How Email Processing Works

### Flow:

```
1. Email arrives at info@doganconsult.com (Microsoft 365)
   ↓
2. Microsoft Graph detects new email
   ↓
3. Graph sends webhook notification to: https://portal.shahin-ai.com/api/webhooks/email
   ↓
4. EmailWebhookController receives notification
   ↓
5. Enqueues EmailProcessingJob (Hangfire)
   ↓
6. EmailProcessingJob:
   - Fetches email from Graph API
   - Creates thread/message in database
   - Classifies email (AI)
   - Processes auto-reply rules
   - Sends auto-reply if rule matches
```

**Current Status**: Step 3 is blocked (no webhook subscription)!

---

## 🚀 Quick Fix Script

Here's a complete setup script:

```sql
-- Step 1: Update mailbox with Graph User ID
UPDATE "EmailMailboxes"
SET 
    "GraphUserId" = 'info@doganconsult.com',
    "ModifiedDate" = NOW()
WHERE "EmailAddress" = 'info@doganconsult.com';

-- Verify
SELECT "EmailAddress", "GraphUserId", "WebhookSubscriptionId", "IsActive", "AutoReplyEnabled"
FROM "EmailMailboxes"
WHERE "EmailAddress" = 'info@doganconsult.com';
```

Then create webhook subscription via API or Graph Explorer.

---

## 📋 Manual Setup Instructions

### Method 1: Via Application API (Recommended)

If your application is running:

1. **Get Mailbox ID**:
```sql
SELECT "Id" FROM "EmailMailboxes" WHERE "EmailAddress" = 'info@doganconsult.com';
```

2. **Call API** (replace `{mailboxId}` with the ID from step 1):
```bash
curl -X POST "https://portal.shahin-ai.com/api/email/subscriptions/create/{mailboxId}" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json"
```

### Method 2: Via Microsoft Graph Explorer

1. Go to: https://developer.microsoft.com/graph/graph-explorer
2. Sign in with admin credentials for `doganconsult.com`
3. Run this query:

```http
POST https://graph.microsoft.com/v1.0/subscriptions
Content-Type: application/json

{
  "changeType": "created",
  "notificationUrl": "https://portal.shahin-ai.com/api/webhooks/email",
  "resource": "/users/info@doganconsult.com/mailFolders/inbox/messages",
  "expirationDateTime": "2026-02-22T00:00:00Z",
  "clientState": "grc-email-webhook-2026"
}
```

4. Copy the `id` from response → This is your `WebhookSubscriptionId`
5. Update database:
```sql
UPDATE "EmailMailboxes"
SET 
    "WebhookSubscriptionId" = '{subscription-id-from-step-4}',
    "WebhookExpiresAt" = '2026-02-22T00:00:00Z',
    "ModifiedDate" = NOW()
WHERE "EmailAddress" = 'info@doganconsult.com';
```

---

## ✅ Verification Checklist

After setup, verify:

- [ ] `GraphUserId` is set in database
- [ ] `WebhookSubscriptionId` is set in database
- [ ] `WebhookExpiresAt` is set (future date)
- [ ] Webhook URL is publicly accessible (https://portal.shahin-ai.com/api/webhooks/email)
- [ ] Application is running
- [ ] Microsoft Graph API permissions granted (`Mail.Read`, `Mail.Send`)

---

## 🧪 Testing After Setup

1. **Send Test Email**: Send email to `info@doganconsult.com`
2. **Check Webhook Logs**: Check application logs for webhook receipt
3. **Check Database**: Verify email appears in `EmailMessages` table
4. **Check Auto-Reply**: Verify auto-reply was sent (if rule matched)

---

## 🔄 Alternative: Polling Mode (If Webhooks Don't Work)

If webhooks aren't possible, you can set up polling:

1. Create recurring Hangfire job to check for new emails
2. Runs every X minutes
3. Fetches new emails from Graph API
4. Processes them normally

**This is less efficient but works if webhooks aren't available.**

---

## 📞 Next Steps

1. ✅ **Update GraphUserId** in database
2. ✅ **Create webhook subscription** (via API or Graph Explorer)
3. ✅ **Update database** with subscription ID
4. ✅ **Test** by sending email
5. ✅ **Verify** auto-reply works

**Would you like me to:**
- Update the GraphUserId now?
- Help create the webhook subscription?
- Set up polling mode as backup?
