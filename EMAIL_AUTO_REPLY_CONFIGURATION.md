# 📧 Email Auto-Reply Configuration Guide

**Date**: 2026-01-22  
**Purpose**: Configure automatic email replies for Forward, Reminders, Claimed, and Administrative emails

---

## 🎯 Overview

The system supports automatic email replies based on:
- **Email Classifications** (AI-detected)
- **Subject Patterns** (Regex)
- **From Email Patterns** (Regex)
- **Body Patterns** (Regex)

**Actions Available**:
- `CreateDraft` - Create draft for human review (default)
- `SendImmediately` - Send reply automatically
- `CreateTask` - Create a task for follow-up
- `MarkAsHandled` - Mark as handled/closed
- `Ignore` - Mark as spam

---

## ✅ Step 1: Enable Auto-Reply Globally

### Update `appsettings.Production.json`:

```json
{
  "EmailOperations": {
    "Enabled": true,
    "AutoReplyEnabled": true,  // ✅ Enable auto-reply
    "DraftModeDefault": false,  // Set to true if you want drafts instead of immediate sending
    "WebhookBaseUrl": "https://shahin-ai.com/api/webhooks/email"
  }
}
```

### Update Environment Variable (if using):

```bash
# In .env.production.secure
EmailOperations__AutoReplyEnabled=true
EmailOperations__DraftModeDefault=false
```

---

## 📋 Step 2: Enable Auto-Reply for Each Mailbox

### Via UI:
1. Go to: **Email Operations** → **Mailboxes**
2. Select your mailbox (e.g., `info@doganconsult.com`)
3. Check **"تفعيل الرد الآلي"** (Enable Auto-Reply)
4. Click **Save**

### Via API/Database:

```sql
UPDATE "EmailMailboxes"
SET "AutoReplyEnabled" = true
WHERE "EmailAddress" = 'info@doganconsult.com';
```

---

## 🔧 Step 3: Create Auto-Reply Rules

### Rule 1: Administrative Emails (Compliance, Contracts, Audit)

**Classification**: `ComplianceQuery`, `ContractQuestion`, `AuditRequest`

```sql
INSERT INTO "EmailAutoReplyRules" (
    "Id", "MailboxId", "Name", "Description",
    "TriggerClassifications", "SubjectPattern", "FromPattern",
    "Action", "ReplyContent", "UseAiGeneration",
    "Priority", "IsActive", "MaxAutoRepliesPerThread",
    "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy"
)
SELECT 
    gen_random_uuid(),
    m."Id",
    'رد آلي للاستفسارات الإدارية',
    'يرد تلقائياً على الاستفسارات الإدارية والقانونية والامتثال',
    ARRAY[31, 30, 32]::integer[], -- ComplianceQuery, ContractQuestion, AuditRequest
    NULL,
    NULL,
    0, -- CreateDraft (for review before sending)
    '<html>
    <body dir="rtl">
    <p>عزيزي/عزيزتي <strong>{SenderName}</strong>،</p>
    <p>تم استلام استفسارك وسيتم الرد عليك في أقرب وقت ممكن من فريق الإدارة.</p>
    <p>رقم المتابعة: {ThreadId}</p>
    <p>مع أطيب التحيات،<br>فريق شاهين للأنظمة</p>
    </body>
    </html>',
    false, -- Use static template
    10, -- High priority (evaluated first)
    true,
    1,
    NOW(),
    NOW(),
    'System',
    'System'
FROM "EmailMailboxes" m
WHERE m."EmailAddress" = 'info@doganconsult.com'
LIMIT 1;
```

### Rule 2: Forwarded Emails (Subject contains "Fwd:" or "FW:")

```sql
INSERT INTO "EmailAutoReplyRules" (
    "Id", "MailboxId", "Name", "Description",
    "TriggerClassifications", "SubjectPattern", "FromPattern",
    "Action", "ReplyContent", "UseAiGeneration",
    "Priority", "IsActive", "MaxAutoRepliesPerThread",
    "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy"
)
SELECT 
    gen_random_uuid(),
    m."Id",
    'رد آلي للرسائل المُعاد توجيهها',
    'يرد تلقائياً على الرسائل المُعاد توجيهها (Forward)',
    NULL,
    '(?i)^(Fwd?|FW?|Forwarded|Re:.*Fwd?):', -- Matches "Fwd:", "FW:", "Forwarded:", etc.
    NULL,
    1, -- SendImmediately (routine acknowledgment)
    '<html>
    <body dir="rtl">
    <p>شكراً لإعادة توجيه هذه الرسالة.</p>
    <p>تم استلامها وستتم مراجعتها في أقرب وقت.</p>
    <p>مع أطيب التحيات،<br>فريق شاهين</p>
    </body>
    </html>',
    false,
    20,
    true,
    1,
    NOW(),
    NOW(),
    'System',
    'System'
FROM "EmailMailboxes" m
WHERE m."EmailAddress" = 'info@doganconsult.com'
LIMIT 1;
```

### Rule 3: Reminder Emails (Subject/Body contains reminder keywords)

```sql
INSERT INTO "EmailAutoReplyRules" (
    "Id", "MailboxId", "Name", "Description",
    "TriggerClassifications", "SubjectPattern", "FromPattern",
    "BodyPattern",
    "Action", "ReplyContent", "UseAiGeneration",
    "Priority", "IsActive", "MaxAutoRepliesPerThread",
    "FollowUpAfterHours",
    "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy"
)
SELECT 
    gen_random_uuid(),
    m."Id",
    'رد آلي للتذكيرات',
    'يرد تلقائياً على رسائل التذكير ويجدول متابعة',
    NULL,
    '(?i)(reminder|تذكير|ذكر|follow.?up|متابعة)', -- Subject pattern
    NULL,
    '(?i)(reminder|تذكير|follow.?up|متابعة)', -- Body pattern
    1, -- SendImmediately
    '<html>
    <body dir="rtl">
    <p>شكراً لتذكيرك.</p>
    <p>تم استلام تذكيرك وسيتم المتابعة.</p>
    <p>رقم المرجع: {ThreadId}</p>
    <p>مع أطيب التحيات،<br>فريق شاهين</p>
    </body>
    </html>',
    false,
    30,
    true,
    1,
    48, -- Schedule follow-up in 48 hours
    NOW(),
    NOW(),
    'System',
    'System'
FROM "EmailMailboxes" m
WHERE m."EmailAddress" = 'info@doganconsult.com'
LIMIT 1;
```

### Rule 4: Claimed/Account Issue Emails

**Classification**: `AccountIssue`

```sql
INSERT INTO "EmailAutoReplyRules" (
    "Id", "MailboxId", "Name", "Description",
    "TriggerClassifications", "SubjectPattern", "FromPattern",
    "Action", "ReplyContent", "UseAiGeneration",
    "Priority", "IsActive", "MaxAutoRepliesPerThread",
    "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy"
)
SELECT 
    gen_random_uuid(),
    m."Id",
    'رد آلي لمشاكل الحساب',
    'يرد تلقائياً على استفسارات ومشاكل الحساب',
    ARRAY[12]::integer[], -- AccountIssue
    NULL,
    NULL,
    0, -- CreateDraft (needs human review for account issues)
    '<html>
    <body dir="rtl">
    <p>عزيزي/عزيزتي <strong>{SenderName}</strong>،</p>
    <p>تم استلام استفسارك المتعلق بحسابك.</p>
    <p>سيقوم فريق الدعم الفني بمراجعة طلبك والرد عليك خلال 24 ساعة.</p>
    <p>رقم المرجع: {ThreadId}</p>
    <p>مع أطيب التحيات،<br>فريق الدعم الفني - شاهين</p>
    </body>
    </html>',
    false,
    15,
    true,
    1,
    NOW(),
    NOW(),
    'System',
    'System'
FROM "EmailMailboxes" m
WHERE m."EmailAddress" = 'info@doganconsult.com'
LIMIT 1;
```

### Rule 5: All Administrative Emails (Catch-all)

**For all admin-related classifications:**

```sql
INSERT INTO "EmailAutoReplyRules" (
    "Id", "MailboxId", "Name", "Description",
    "TriggerClassifications", "SubjectPattern", "FromPattern",
    "Action", "ReplyContent", "UseAiGeneration",
    "Priority", "IsActive", "MaxAutoRepliesPerThread",
    "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy"
)
SELECT 
    gen_random_uuid(),
    m."Id",
    'رد آلي عام للاستفسارات الإدارية',
    'يرد على جميع الاستفسارات الإدارية بشكل عام',
    ARRAY[30, 31, 32, 33, 40, 41, 42]::integer[], -- ContractQuestion, ComplianceQuery, AuditRequest, DocumentRequest, JobApplication, VendorInquiry, MediaInquiry
    NULL,
    NULL,
    1, -- SendImmediately
    '<html>
    <body dir="rtl">
    <p>عزيزي/عزيزتي <strong>{SenderName}</strong>،</p>
    <p>تم استلام استفسارك بنجاح.</p>
    <p>سيتم الرد عليك من قبل الفريق المختص في أقرب وقت ممكن.</p>
    <p>رقم المتابعة: {ThreadId}</p>
    <p>مع أطيب التحيات،<br>فريق شاهين للأنظمة</p>
    </body>
    </html>',
    false,
    100, -- Lower priority (applied if no other rule matches)
    true,
    2, -- Allow up to 2 auto-replies per thread
    NOW(),
    NOW(),
    'System',
    'System'
FROM "EmailMailboxes" m
WHERE m."EmailAddress" = 'info@doganconsult.com'
LIMIT 1;
```

---

## 🤖 Step 4: AI-Generated Replies (Optional)

If you want AI to generate personalized replies:

### Rule with AI Generation:

```sql
INSERT INTO "EmailAutoReplyRules" (
    "Id", "MailboxId", "Name", "Description",
    "TriggerClassifications", "SubjectPattern",
    "Action", "UseAiGeneration", "AiPromptTemplate",
    "Priority", "IsActive",
    "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy"
)
SELECT 
    gen_random_uuid(),
    m."Id",
    'رد آلي ذكي بالذكاء الاصطناعي',
    'يرد تلقائياً باستخدام الذكاء الاصطناعي للردود المخصصة',
    ARRAY[10, 11, 13]::integer[], -- TechnicalSupport, BillingInquiry, FeatureRequest
    NULL,
    0, -- CreateDraft (AI replies should be reviewed)
    true, -- Enable AI generation
    'أنت مساعد ذكي لشركة {Brand}. ارد على الاستفسار التالي بطريقة مهنية ومفيدة باللغة العربية. كن مختصراً وواضحاً.',
    50,
    true,
    NOW(),
    NOW(),
    'System',
    'System'
FROM "EmailMailboxes" m
WHERE m."EmailAddress" = 'info@doganconsult.com'
LIMIT 1;
```

---

## 📊 Email Classifications Reference

| Classification | ID | Description | Arabic |
|----------------|-----|-------------|---------|
| `TechnicalSupport` | 10 | Technical support requests | دعم فني |
| `BillingInquiry` | 11 | Billing questions | استفسار عن الفواتير |
| `AccountIssue` | 12 | Account problems | مشكلة في الحساب |
| `FeatureRequest` | 13 | Feature requests | طلب ميزة |
| `BugReport` | 14 | Bug reports | بلاغ خطأ |
| `QuoteRequest` | 20 | Price quotes | عرض سعر |
| `DemoRequest` | 21 | Demo requests | عرض تجريبي |
| `ContractQuestion` | 30 | Contract questions | سؤال عقد |
| `ComplianceQuery` | 31 | Compliance questions | استفسار امتثال |
| `AuditRequest` | 32 | Audit requests | طلب مراجعة |
| `DocumentRequest` | 33 | Document requests | طلب مستند |
| `JobApplication` | 40 | Job applications | طلب توظيف |
| `VendorInquiry` | 41 | Vendor inquiries | استفسار مورد |
| `MediaInquiry` | 42 | Media inquiries | استفسار إعلامي |

---

## 🔄 Action Types Reference

| Action | Value | Description |
|--------|-------|-------------|
| `CreateDraft` | 0 | Create draft for human review (SAFEST) |
| `SendImmediately` | 1 | Send reply immediately (for routine messages) |
| `CreateTask` | 2 | Create a task for follow-up |
| `Forward` | 3 | Forward to specific team |
| `MarkAsHandled` | 4 | Mark as handled/closed |
| `Escalate` | 5 | Escalate to manager |
| `Ignore` | 6 | Ignore (spam) |

---

## 🧪 Testing Auto-Reply

### 1. Enable Auto-Reply for Mailbox:

```sql
UPDATE "EmailMailboxes"
SET "AutoReplyEnabled" = true
WHERE "EmailAddress" = 'info@doganconsult.com';
```

### 2. Send Test Emails:

- **Forward Test**: Send email with subject "Fwd: Test Message"
- **Reminder Test**: Send email with subject containing "Reminder"
- **Admin Test**: Send email that will be classified as `ComplianceQuery`
- **Account Issue Test**: Send email about account problems

### 3. Check Results:

- View drafts in **Email Operations** → **Threads** → Filter by `DraftPending`
- View sent emails in **Email Operations** → **Threads** → Filter by `AwaitingCustomerReply`

---

## 📝 Complete SQL Script

Save this as `setup_auto_reply_rules.sql`:

```sql
-- Enable auto-reply for mailbox
UPDATE "EmailMailboxes"
SET "AutoReplyEnabled" = true
WHERE "EmailAddress" = 'info@doganconsult.com';

-- Get mailbox ID
DO $$
DECLARE
    v_mailbox_id UUID;
BEGIN
    SELECT "Id" INTO v_mailbox_id
    FROM "EmailMailboxes"
    WHERE "EmailAddress" = 'info@doganconsult.com'
    LIMIT 1;

    IF v_mailbox_id IS NULL THEN
        RAISE EXCEPTION 'Mailbox not found: info@doganconsult.com';
    END IF;

    -- Rule 1: Administrative (Compliance, Contracts, Audit)
    INSERT INTO "EmailAutoReplyRules" (
        "Id", "MailboxId", "Name", "Description",
        "TriggerClassifications", "Action", "ReplyContent",
        "Priority", "IsActive", "MaxAutoRepliesPerThread",
        "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy"
    ) VALUES (
        gen_random_uuid(), v_mailbox_id,
        'رد آلي للاستفسارات الإدارية',
        'يرد تلقائياً على الاستفسارات الإدارية والقانونية والامتثال',
        ARRAY[31, 30, 32]::integer[],
        0, -- CreateDraft
        '<html><body dir="rtl"><p>تم استلام استفسارك وسيتم الرد عليك قريباً.</p></body></html>',
        10, true, 1, NOW(), NOW(), 'System', 'System'
    );

    -- Rule 2: Forwarded Emails
    INSERT INTO "EmailAutoReplyRules" (
        "Id", "MailboxId", "Name", "Description",
        "SubjectPattern", "Action", "ReplyContent",
        "Priority", "IsActive",
        "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy"
    ) VALUES (
        gen_random_uuid(), v_mailbox_id,
        'رد آلي للرسائل المُعاد توجيهها',
        'يرد تلقائياً على الرسائل المُعاد توجيهها',
        '(?i)^(Fwd?|FW?|Forwarded):',
        1, -- SendImmediately
        '<html><body dir="rtl"><p>شكراً لإعادة توجيه هذه الرسالة. تم استلامها وستتم مراجعتها.</p></body></html>',
        20, true, NOW(), NOW(), 'System', 'System'
    );

    -- Rule 3: Reminders
    INSERT INTO "EmailAutoReplyRules" (
        "Id", "MailboxId", "Name", "Description",
        "SubjectPattern", "Action", "ReplyContent",
        "Priority", "IsActive", "FollowUpAfterHours",
        "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy"
    ) VALUES (
        gen_random_uuid(), v_mailbox_id,
        'رد آلي للتذكيرات',
        'يرد تلقائياً على رسائل التذكير',
        '(?i)(reminder|تذكير|follow.?up|متابعة)',
        1, -- SendImmediately
        '<html><body dir="rtl"><p>شكراً لتذكيرك. تم استلام تذكيرك وسيتم المتابعة.</p></body></html>',
        30, true, 48, NOW(), NOW(), 'System', 'System'
    );

    -- Rule 4: Account Issues
    INSERT INTO "EmailAutoReplyRules" (
        "Id", "MailboxId", "Name", "Description",
        "TriggerClassifications", "Action", "ReplyContent",
        "Priority", "IsActive",
        "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy"
    ) VALUES (
        gen_random_uuid(), v_mailbox_id,
        'رد آلي لمشاكل الحساب',
        'يرد تلقائياً على استفسارات الحساب',
        ARRAY[12]::integer[], -- AccountIssue
        0, -- CreateDraft
        '<html><body dir="rtl"><p>تم استلام استفسارك المتعلق بحسابك. سيقوم فريق الدعم الفني بالرد خلال 24 ساعة.</p></body></html>',
        15, true, NOW(), NOW(), 'System', 'System'
    );

    -- Rule 5: General Administrative (Catch-all)
    INSERT INTO "EmailAutoReplyRules" (
        "Id", "MailboxId", "Name", "Description",
        "TriggerClassifications", "Action", "ReplyContent",
        "Priority", "IsActive", "MaxAutoRepliesPerThread",
        "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy"
    ) VALUES (
        gen_random_uuid(), v_mailbox_id,
        'رد آلي عام للإداري',
        'يرد على جميع الاستفسارات الإدارية',
        ARRAY[30, 31, 32, 33, 40, 41, 42]::integer[],
        1, -- SendImmediately
        '<html><body dir="rtl"><p>تم استلام استفسارك بنجاح. سيتم الرد عليك من قبل الفريق المختص قريباً.</p></body></html>',
        100, true, 2, NOW(), NOW(), 'System', 'System'
    );

    RAISE NOTICE 'Auto-reply rules created successfully for mailbox: %', v_mailbox_id;
END $$;
```

---

## ✅ Verification Checklist

- [ ] Auto-reply enabled in `appsettings.Production.json`
- [ ] Auto-reply enabled for mailbox in database/UI
- [ ] Auto-reply rules created in database
- [ ] Rules have correct priority order
- [ ] Test emails sent and verified
- [ ] Drafts created or emails sent automatically
- [ ] Follow-up scheduling working (for reminders)

---

## 🚀 Next Steps

1. Run the SQL script to create rules
2. Enable auto-reply for your mailbox
3. Test with sample emails
4. Monitor drafts/sent emails in the UI
5. Adjust rules as needed

**Your email service is now configured for automatic replies!** 📧✅
