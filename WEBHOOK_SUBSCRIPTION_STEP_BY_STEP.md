# 📧 Step-by-Step: Create Webhook Subscription via Microsoft Graph Explorer

**Guide**: How to create webhook subscription manually when webhook URL is not publicly accessible

---

## 🎯 Goal

Create a Microsoft Graph webhook subscription so your app receives notifications when emails arrive at `info@doganconsult.com`.

---

## 📋 Prerequisites

1. ✅ Admin access to `doganconsult.com` Microsoft 365
2. ✅ Access to Microsoft Graph Explorer
3. ✅ Azure App Registration credentials (already have these)

---

## 🔧 Step-by-Step Instructions

### Step 1: Sign In to Microsoft Graph Explorer

1. Go to: **https://developer.microsoft.com/graph/graph-explorer**
2. Click **"Sign in to Graph Explorer"** (top right)
3. Sign in with an admin account for `doganconsult.com`
   - Use: `info@doganconsult.com` or your admin account

---

### Step 2: Get Access Token

**Option A: Use the token from Graph Explorer** (easiest)
- After signing in, Graph Explorer automatically gets a token
- Click on **"Access token"** tab to view it
- Copy the token (you'll need it for the API call)

**Option B: Get token via API** (if needed)
- Use the credentials you already have

---

### Step 3: Create Webhook Subscription

1. In Graph Explorer, change the **HTTP method** dropdown to: **`POST`**

2. Change the **API endpoint** to:
   ```
   https://graph.microsoft.com/v1.0/subscriptions
   ```

3. Click on **"Request Body"** tab

4. Paste this JSON in the request body:

```json
{
  "changeType": "created",
  "notificationUrl": "https://portal.shahin-ai.com/api/webhooks/email",
  "resource": "/users/info@doganconsult.com/mailFolders/inbox/messages",
  "expirationDateTime": "2026-02-22T00:00:00Z",
  "clientState": "grc-email-webhook-2026"
}
```

**Important**: 
- `notificationUrl`: Your webhook endpoint (must be publicly accessible)
- `resource`: The mailbox to monitor
- `expirationDateTime`: When subscription expires (set to future date)
- `clientState`: A secret string for validation

---

### Step 4: Add Authorization Header

1. Click on **"Request Headers"** tab
2. Click **"Add header"**
3. Add:
   - **Name**: `Authorization`
   - **Value**: `Bearer {YOUR_ACCESS_TOKEN}`
   - Replace `{YOUR_ACCESS_TOKEN}` with the token from Step 2

**OR** if Graph Explorer is already signed in, it may add this automatically.

---

### Step 5: Modify Permissions (If Needed)

1. Click on **"Modify Permissions"** tab
2. Ensure these permissions are granted:
   - ✅ `Mail.Read` (Application permission)
   - ✅ `Mail.ReadBasic` (Application permission)
3. Click **"Consent"** if needed

---

### Step 6: Run the Query

1. Click the blue **"Run query"** button (top right)
2. Wait for the response

---

### Step 7: Check the Response

**Success Response** (200 OK):
```json
{
  "id": "abc123-def456-ghi789",
  "resource": "/users/info@doganconsult.com/mailFolders/inbox/messages",
  "applicationId": "4e2575c6-e269-48eb-b055-ad730a2150a7",
  "changeType": "created",
  "clientState": "grc-email-webhook-2026",
  "notificationUrl": "https://portal.shahin-ai.com/api/webhooks/email",
  "expirationDateTime": "2026-02-22T00:00:00Z",
  "creatorId": "..."
}
```

**Copy the `id` value** - this is your `WebhookSubscriptionId`!

---

### Step 8: Update Database

After getting the subscription ID, update your database:

```sql
UPDATE "EmailMailboxes"
SET 
    "WebhookSubscriptionId" = '{id-from-step-7}',
    "WebhookExpiresAt" = '2026-02-22T00:00:00Z',
    "ModifiedDate" = NOW()
WHERE "EmailAddress" = 'info@doganconsult.com';
```

**Or use this command**:
```bash
docker exec -i a9391f4add8a_grc-db psql -U postgres -d GrcMvcDb -c "
UPDATE \"EmailMailboxes\"
SET 
    \"WebhookSubscriptionId\" = 'YOUR_SUBSCRIPTION_ID_HERE',
    \"WebhookExpiresAt\" = '2026-02-22T00:00:00Z',
    \"ModifiedDate\" = NOW()
WHERE \"EmailAddress\" = 'info@doganconsult.com';
"
```

---

## ⚠️ Common Issues & Solutions

### Issue 1: "ValidationError - Unable to connect to remote server"

**Problem**: Webhook URL is not publicly accessible

**Solutions**:
- ✅ **Option A**: Deploy your application to a public URL
- ✅ **Option B**: Use polling mode instead (see below)
- ✅ **Option C**: Use ngrok or similar tool for testing

### Issue 2: "Insufficient privileges"

**Problem**: App doesn't have required permissions

**Solution**:
1. Go to Azure Portal → App Registrations
2. Select your app (`4e2575c6-e269-48eb-b055-ad730a2150a7`)
3. Go to **API permissions**
4. Add: `Mail.Read`, `Mail.ReadBasic` (Application permissions)
5. Click **"Grant admin consent"**

### Issue 3: "Resource not found"

**Problem**: Invalid mailbox email or user doesn't exist

**Solution**: Verify `info@doganconsult.com` exists in Azure AD

---

## 🔄 Alternative: Polling Mode (If Webhooks Don't Work)

If webhook URL is not publicly accessible, use polling instead:

### Add Recurring Job for Email Polling

Add this to `Program.cs` (in the recurring jobs section):

```csharp
// Email polling - every 5 minutes (if webhooks not available)
RecurringJob.AddOrUpdate<EmailProcessingJob>(
    "email-polling",
    job => job.SyncAllMailboxesAsync(),
    "*/5 * * * *", // Every 5 minutes
    new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });
```

Then add this method to `EmailProcessingJob.cs`:

```csharp
public async Task SyncAllMailboxesAsync()
{
    var mailboxes = await _db.Set<EmailMailbox>()
        .Where(m => m.IsActive && m.GraphUserId != null && m.AutoReplyEnabled)
        .ToListAsync();

    foreach (var mailbox in mailboxes)
    {
        try
        {
            var token = await _graphService.GetAccessTokenAsync(
                mailbox.TenantId!,
                mailbox.ClientId!,
                DecryptSecret(mailbox.EncryptedClientSecret!));

            // Get new messages since last sync
            var messages = await _graphService.GetMessagesAsync(
                token,
                mailbox.GraphUserId!,
                since: mailbox.LastSyncAt ?? DateTime.UtcNow.AddHours(-1),
                top: 50);

            foreach (var message in messages)
            {
                // Check if already processed
                var existing = await _db.Set<EmailMessage>()
                    .FirstOrDefaultAsync(m => m.GraphMessageId == message.Id);

                if (existing == null)
                {
                    // Process new email
                    await ProcessNewEmailAsync(mailbox.GraphUserId!, message.Id, null);
                }
            }

            mailbox.LastSyncAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync mailbox {Mailbox}", mailbox.EmailAddress);
        }
    }
}
```

**This will check for new emails every 5 minutes and process them.**

---

## ✅ Verification Checklist

After creating subscription:

- [ ] Subscription ID received from Graph API
- [ ] Database updated with subscription ID
- [ ] Webhook URL is publicly accessible (or use polling)
- [ ] Application is running
- [ ] Test email sent
- [ ] Check application logs for webhook receipt
- [ ] Verify auto-reply was sent

---

## 📊 Quick Reference

| Item | Value |
|------|-------|
| **API Endpoint** | `POST https://graph.microsoft.com/v1.0/subscriptions` |
| **Mailbox** | `info@doganconsult.com` |
| **Webhook URL** | `https://portal.shahin-ai.com/api/webhooks/email` |
| **Resource** | `/users/info@doganconsult.com/mailFolders/inbox/messages` |
| **Change Type** | `created` |
| **Expiration** | Set to 3 days from now (or longer) |

---

## 🎯 Next Steps After Creating Subscription

1. ✅ Update database with subscription ID
2. ✅ Test by sending email to `info@doganconsult.com`
3. ✅ Check application logs
4. ✅ Verify auto-reply works

**Your email auto-reply will work once the subscription is created!** 📧✅
