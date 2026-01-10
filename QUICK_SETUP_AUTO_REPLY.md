# ⚡ Quick Setup: Email Auto-Reply Configuration

**Quick Start Guide** - Configure auto-reply for Forward, Reminders, Claimed, and Administrative emails

---

## 🚀 Quick Steps (5 minutes)

### Step 1: Enable Auto-Reply in Configuration

**File**: `src/GrcMvc/appsettings.Production.json`

Find the `EmailOperations` section and update:

```json
{
  "EmailOperations": {
    "Enabled": true,
    "WebhookBaseUrl": "https://shahin-ai.com/api/webhooks/email",
    "DefaultSlaFirstResponseHours": 24,
    "DefaultSlaResolutionHours": 72,
    "AutoReplyEnabled": true,  // ✅ Change from false to true
    "DraftModeDefault": false,  // Set to false for immediate sending, true for drafts
    "MicrosoftGraph": {
      "TenantId": "c8847e8a-33a0-4b6c-8e01-2e0e6b4aaef5",
      "ClientId": "4e2575c6-e269-48eb-b055-ad730a2150a7",
      "ClientSecret": "${MSGRAPH_CLIENT_SECRET}",
      "ApplicationIdUri": "api://4e2575c6-e269-48eb-b055-ad730a2150a7"
    }
  }
}
```

---

### Step 2: Run SQL Script to Create Rules

```bash
# Connect to your PostgreSQL database
psql -h localhost -U postgres -d GrcMvcDb

# Run the setup script
\i setup_auto_reply_rules.sql
```

**Or manually via SQL**:

```sql
-- Enable auto-reply for mailbox
UPDATE "EmailMailboxes"
SET "AutoReplyEnabled" = true
WHERE "EmailAddress" = 'info@doganconsult.com';
```

Then run the complete SQL script from `setup_auto_reply_rules.sql`.

---

### Step 3: Verify Configuration

```sql
-- Check if auto-reply is enabled
SELECT "EmailAddress", "AutoReplyEnabled", "DisplayName"
FROM "EmailMailboxes"
WHERE "EmailAddress" = 'info@doganconsult.com';

-- Check created rules
SELECT "Name", "Priority", "Action", "IsActive"
FROM "EmailAutoReplyRules" r
JOIN "EmailMailboxes" m ON r."MailboxId" = m."Id"
WHERE m."EmailAddress" = 'info@doganconsult.com'
ORDER BY "Priority";
```

---

## 📋 Created Rules Summary

| Rule | Priority | Action | Matches |
|------|----------|--------|---------|
| **Administrative** | 10 | CreateDraft | Compliance, Contract, Audit queries |
| **Account Issues** | 15 | CreateDraft | Account problems |
| **Forwarded** | 20 | SendImmediately | Emails with "Fwd:", "FW:", etc. |
| **Reminders** | 30 | SendImmediately | Emails with "reminder", "تذكير", etc. |
| **General Admin** | 100 | SendImmediately | All admin classifications (catch-all) |

---

## ✅ Testing

### Test 1: Forward Email
Send email with subject: **"Fwd: Test Message"**
- Expected: Auto-reply sent immediately

### Test 2: Reminder Email
Send email with subject: **"Reminder: Follow up"**
- Expected: Auto-reply sent + follow-up scheduled in 48 hours

### Test 3: Administrative Email
Send email about compliance/contract
- Expected: Draft created for review (if DraftModeDefault=true)

### Test 4: Account Issue
Send email about account problem
- Expected: Draft created for review

---

## 🔧 Configuration Options

### Immediate Sending vs Drafts

- **`DraftModeDefault: false`** → Auto-replies sent immediately
- **`DraftModeDefault: true`** → Auto-replies created as drafts (safer)

**Recommendation**: Start with `DraftModeDefault: true` for safety, then change to `false` after testing.

---

## 📧 Action Types

- **`SendImmediately` (1)**: Sends reply automatically - Use for routine messages
- **`CreateDraft` (0)**: Creates draft for review - Use for sensitive topics

---

## 🔄 Restart Application

After making changes:

```bash
# Restart the application
sudo systemctl restart grcmvc

# Or if using Docker
docker-compose restart grcmvc
```

---

## 📝 Next Steps

1. ✅ Enable auto-reply in configuration
2. ✅ Run SQL script to create rules
3. ✅ Test with sample emails
4. ✅ Monitor drafts/sent emails in UI
5. ✅ Adjust rules as needed

**Done! Your email auto-reply is now configured!** 🎉

For detailed documentation, see: `EMAIL_AUTO_REPLY_CONFIGURATION.md`
