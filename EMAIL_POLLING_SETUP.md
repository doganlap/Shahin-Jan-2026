# 🔄 Email Polling Mode Setup (Alternative to Webhooks)

**Problem**: Webhook URL is not publicly accessible  
**Solution**: Use polling mode to check for new emails periodically

---

## ✅ What is Polling Mode?

Instead of receiving real-time notifications via webhooks, the system will:
- Check for new emails every X minutes
- Process any new emails found
- Apply auto-reply rules
- Send replies automatically

**Advantages**:
- ✅ Works without public URL
- ✅ Works behind firewall
- ✅ Simpler setup
- ✅ More reliable (no webhook expiration)

**Disadvantages**:
- ⚠️ Slight delay (depends on polling interval)
- ⚠️ More API calls (but Graph API handles this well)

---

## 🔧 Setup: Add Recurring Polling Job

### Step 1: Add Sync Method to EmailProcessingJob

Already exists! Check `EmailProcessingJob.cs` - there should be a method to sync mailboxes.

### Step 2: Add Recurring Job in Program.cs

Add this to your recurring jobs section (around line 1450):

```csharp
// Email polling - check for new emails every 5 minutes
RecurringJob.AddOrUpdate<EmailProcessingJob>(
    "email-polling-sync",
    job => job.SyncAllMailboxesAsync(),
    "*/5 * * * *", // Every 5 minutes
    new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });
```

### Step 3: Add SyncAllMailboxesAsync Method

Add this method to `EmailProcessingJob.cs`:

```csharp
/// <summary>
/// Sync all active mailboxes - check for new emails and process them
/// Called by Hangfire recurring job every 5 minutes
/// </summary>
[Hangfire.AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 30, 120, 300 })]
public async Task SyncAllMailboxesAsync()
{
    _logger.LogInformation("Starting email polling sync for all mailboxes");

    var mailboxes = await _db.Set<EmailMailbox>()
        .Where(m => m.IsActive && m.GraphUserId != null && m.AutoReplyEnabled)
        .ToListAsync();

    foreach (var mailbox in mailboxes)
    {
        try
        {
            _logger.LogInformation("Syncing mailbox: {Email}", mailbox.EmailAddress);

            // Get access token
            var token = await _graphService.GetAccessTokenAsync(
                mailbox.TenantId!,
                mailbox.ClientId!,
                DecryptSecret(mailbox.EncryptedClientSecret!));

            // Get messages since last sync (or last hour if never synced)
            var since = mailbox.LastSyncAt ?? DateTime.UtcNow.AddHours(-1);
            var messages = await _graphService.GetMessagesAsync(
                token,
                mailbox.GraphUserId!,
                since: since,
                top: 50);

            _logger.LogInformation("Found {Count} new messages in {Mailbox}", 
                messages.Count, mailbox.EmailAddress);

            // Process each new message
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

            // Update last sync time
            mailbox.LastSyncAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            _logger.LogInformation("Completed sync for mailbox: {Email}", mailbox.EmailAddress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync mailbox {Mailbox}", mailbox.EmailAddress);
            // Continue with next mailbox
        }
    }

    _logger.LogInformation("Email polling sync completed");
}
```

---

## ⚙️ Configuration

### Polling Interval

**Recommended**: Every 5 minutes (`*/5 * * * *`)

**Options**:
- Every 1 minute: `*/1 * * * *` (more real-time, more API calls)
- Every 5 minutes: `*/5 * * * *` (balanced) ⭐ Recommended
- Every 10 minutes: `*/10 * * * *` (less frequent)
- Every 15 minutes: `*/15 * * * *` (least API calls)

### Adjust Based on Volume

- **High volume**: Use longer intervals (10-15 min)
- **Low volume**: Can use shorter intervals (1-5 min)
- **Urgent responses needed**: Use 1-2 minutes

---

## ✅ Verification

After setup:

1. **Check Hangfire Dashboard**: Go to `/hangfire` → Recurring Jobs
2. **Verify job exists**: Should see "email-polling-sync"
3. **Wait for next run**: Job runs every 5 minutes
4. **Check logs**: Should see sync activity
5. **Test**: Send email to `info@doganconsult.com`
6. **Wait up to 5 minutes**: Email should be processed
7. **Verify**: Auto-reply should be sent

---

## 📊 Monitoring

### Check Last Sync Time

```sql
SELECT 
    "EmailAddress",
    "LastSyncAt",
    "AutoReplyEnabled",
    NOW() - "LastSyncAt" as "TimeSinceLastSync"
FROM "EmailMailboxes"
WHERE "EmailAddress" = 'info@doganconsult.com';
```

### Check Processed Messages

```sql
SELECT 
    COUNT(*) as "TotalMessages",
    COUNT(CASE WHEN "Direction" = 'Inbound' THEN 1 END) as "Received",
    COUNT(CASE WHEN "Direction" = 'Outbound' THEN 1 END) as "Sent",
    MAX("ReceivedAt") as "LastEmailReceived"
FROM "EmailMessages"
WHERE "ReceivedAt" > NOW() - INTERVAL '1 day';
```

---

## 🔄 Transition from Webhooks to Polling

If webhooks become available later:

1. Create webhook subscription (when URL is public)
2. Keep polling as backup (with longer interval)
3. Or disable polling once webhooks are confirmed working

---

## ✅ Summary

**Current Setup**:
- ❌ Webhooks: Not available (URL not public)
- ✅ Polling: Will be set up (recommended)

**Result**:
- ✅ Email auto-reply will work
- ✅ Checks every 5 minutes
- ✅ Processes new emails automatically
- ✅ Applies auto-reply rules

---

**Would you like me to add the polling job now?** 🚀
