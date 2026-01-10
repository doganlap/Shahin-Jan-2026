# 🧪 Manual Email Sync Test

## Current Status

✅ **Configuration**: All correct
- Mailbox: `info@doganconsult.com` configured
- Auto-Reply: Enabled
- Rules: 6 active rules
- Last Sync: Never (will sync on first run)

---

## 🧪 Testing Methods

### Method 1: Wait for Automatic Polling (Recommended)

1. **Start/Restart your application**
   ```bash
   # If using docker-compose
   docker-compose restart grcmvc
   
   # Or if running directly
   dotnet run --project src/GrcMvc
   ```

2. **Wait for first poll** (up to 5 minutes)
   - Polling runs every 5 minutes
   - First poll will fetch emails from last hour

3. **Check Hangfire Dashboard**
   - Go to: `http://localhost:5000/hangfire` (or your app URL)
   - Look for: `email-polling-sync` in Recurring Jobs
   - Check execution history

4. **Send test email**
   - Send to: `info@doganconsult.com`
   - Wait up to 5 minutes
   - Check if processed

---

### Method 2: Manual Sync via API (If Available)

If you have an API endpoint to trigger sync:

```bash
# Trigger manual sync
curl -X POST "http://your-app/api/email/mailboxes/sync-all" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

### Method 3: Trigger via Hangfire Dashboard

1. Go to Hangfire Dashboard: `/hangfire`
2. Click on "Recurring Jobs"
3. Find `email-polling-sync`
4. Click "Trigger now" to run immediately

---

## 📊 Monitoring Results

### Check Last Sync Time

```sql
SELECT 
    "EmailAddress",
    "LastSyncAt",
    CASE 
        WHEN "LastSyncAt" IS NULL THEN 'Never synced'
        WHEN NOW() - "LastSyncAt" < INTERVAL '10 minutes' THEN 'Recently synced'
        ELSE 'Synced ' || EXTRACT(EPOCH FROM (NOW() - "LastSyncAt"))::int / 60 || ' minutes ago'
    END as "SyncStatus"
FROM "EmailMailboxes"
WHERE "EmailAddress" = 'info@doganconsult.com';
```

### Check Processed Messages

```sql
SELECT 
    "Subject",
    "FromEmail",
    "ReceivedAt",
    "Direction",
    "Status",
    CASE 
        WHEN "ReceivedAt" > NOW() - INTERVAL '10 minutes' THEN 'Recent'
        ELSE 'Older'
    END as "Age"
FROM "EmailMessages"
ORDER BY "ReceivedAt" DESC
LIMIT 20;
```

### Check Email Threads

```sql
SELECT 
    t."Subject",
    t."Status",
    t."Classification",
    t."Priority",
    COUNT(m."Id") as "MessageCount",
    MAX(m."ReceivedAt") as "LastMessage"
FROM "EmailThreads" t
LEFT JOIN "EmailMessages" m ON t."Id" = m."ThreadId"
GROUP BY t."Id", t."Subject", t."Status", t."Classification", t."Priority"
ORDER BY MAX(m."ReceivedAt") DESC
LIMIT 10;
```

### Check Auto-Replies Sent

```sql
SELECT 
    m."Subject",
    m."FromEmail",
    m."ToRecipients",
    m."ReceivedAt",
    CASE 
        WHEN EXISTS (
            SELECT 1 FROM "EmailMessages" m2 
            WHERE m2."ThreadId" = (SELECT "ThreadId" FROM "EmailMessages" WHERE "Id" = m."Id")
            AND m2."Direction" = 'Outbound'
            AND m2."ReceivedAt" > m."ReceivedAt"
        ) THEN 'Replied'
        ELSE 'Not replied'
    END as "ReplyStatus"
FROM "EmailMessages" m
WHERE m."Direction" = 'Inbound'
AND m."ReceivedAt" > NOW() - INTERVAL '1 hour'
ORDER BY m."ReceivedAt" DESC;
```

---

## ✅ Expected Results

### After First Sync:

1. **LastSyncAt** updated in database
2. **Recent emails** processed (from last hour)
3. **Threads created** for new conversations
4. **Auto-replies sent** if rules matched
5. **Tasks created** for unmatched emails (if catch-all rule active)

### After Sending Test Email:

1. **Email appears** in `EmailMessages` table
2. **Thread created** in `EmailThreads` table
3. **Auto-reply sent** (if rule matches)
4. **Reply message** in `EmailMessages` (Direction = 'Outbound')

---

## 🔍 Troubleshooting

### Polling Not Running?

1. Check if application is running
2. Check Hangfire is enabled in configuration
3. Check Hangfire dashboard for errors
4. Check application logs for exceptions

### Emails Not Processed?

1. Check `LastSyncAt` is updating
2. Check Graph API credentials are correct
3. Check mailbox `GraphUserId` is set
4. Check application logs for errors

### Auto-Replies Not Sent?

1. Verify auto-reply rules are active
2. Check rule priority and matching criteria
3. Check if `DraftModeDefault` is false
4. Verify SMTP/Graph API sending works
5. Check application logs for sending errors

---

## 📝 Test Checklist

- [ ] Application running
- [ ] Hangfire job scheduled
- [ ] Database configuration correct
- [ ] Auto-reply rules active
- [ ] Graph API credentials valid
- [ ] Test email sent
- [ ] Email processed (checked database)
- [ ] Auto-reply sent (if rule matched)
- [ ] LastSyncAt updated

---

**Ready to test!** 🚀
