-- ============================================================
-- Email Auto-Reply Rules Setup Script
-- ============================================================
-- This script creates auto-reply rules for:
-- 1. Administrative emails (Compliance, Contracts, Audit)
-- 2. Forwarded emails
-- 3. Reminder emails
-- 4. Claimed/Account issue emails
-- 5. General administrative (catch-all)
-- ============================================================

-- Step 1: Enable auto-reply for mailbox
UPDATE "EmailMailboxes"
SET "AutoReplyEnabled" = true
WHERE "EmailAddress" = 'info@doganconsult.com';

-- Step 2: Create auto-reply rules
DO $$
DECLARE
    v_mailbox_id UUID;
BEGIN
    -- Get mailbox ID
    SELECT "Id" INTO v_mailbox_id
    FROM "EmailMailboxes"
    WHERE "EmailAddress" = 'info@doganconsult.com'
    LIMIT 1;

    IF v_mailbox_id IS NULL THEN
        RAISE EXCEPTION 'Mailbox not found: info@doganconsult.com';
    END IF;

    -- Rule 1: Administrative (Compliance, Contracts, Audit) - Priority 10
    INSERT INTO "EmailAutoReplyRules" (
        "Id", "MailboxId", "Name", "Description",
        "TriggerClassifications", "Action", "ReplyContent",
        "Priority", "IsActive", "MaxAutoRepliesPerThread",
        "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy"
    ) VALUES (
        gen_random_uuid(), v_mailbox_id,
        'رد آلي للاستفسارات الإدارية',
        'يرد تلقائياً على الاستفسارات الإدارية والقانونية والامتثال',
        ARRAY[31, 30, 32]::integer[], -- ComplianceQuery, ContractQuestion, AuditRequest
        0, -- CreateDraft
        '<html><body dir="rtl"><p>عزيزي/عزيزتي،</p><p>تم استلام استفسارك وسيتم الرد عليك في أقرب وقت ممكن من فريق الإدارة.</p><p>مع أطيب التحيات،<br>فريق شاهين للأنظمة</p></body></html>',
        10, true, 1, NOW(), NOW(), 'System', 'System'
    );

    -- Rule 2: Forwarded Emails - Priority 20
    INSERT INTO "EmailAutoReplyRules" (
        "Id", "MailboxId", "Name", "Description",
        "SubjectPattern", "Action", "ReplyContent",
        "Priority", "IsActive",
        "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy"
    ) VALUES (
        gen_random_uuid(), v_mailbox_id,
        'رد آلي للرسائل المُعاد توجيهها',
        'يرد تلقائياً على الرسائل المُعاد توجيهها (Forward)',
        '(?i)^(Fwd?|FW?|Forwarded|Re:.*Fwd?):',
        1, -- SendImmediately
        '<html><body dir="rtl"><p>شكراً لإعادة توجيه هذه الرسالة.</p><p>تم استلامها وستتم مراجعتها في أقرب وقت.</p><p>مع أطيب التحيات،<br>فريق شاهين</p></body></html>',
        20, true, NOW(), NOW(), 'System', 'System'
    );

    -- Rule 3: Reminders - Priority 30
    INSERT INTO "EmailAutoReplyRules" (
        "Id", "MailboxId", "Name", "Description",
        "SubjectPattern", "BodyPattern", "Action", "ReplyContent",
        "Priority", "IsActive", "FollowUpAfterHours",
        "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy"
    ) VALUES (
        gen_random_uuid(), v_mailbox_id,
        'رد آلي للتذكيرات',
        'يرد تلقائياً على رسائل التذكير ويجدول متابعة',
        '(?i)(reminder|تذكير|ذكر|follow.?up|متابعة)',
        '(?i)(reminder|تذكير|follow.?up|متابعة)',
        1, -- SendImmediately
        '<html><body dir="rtl"><p>شكراً لتذكيرك.</p><p>تم استلام تذكيرك وسيتم المتابعة.</p><p>مع أطيب التحيات،<br>فريق شاهين</p></body></html>',
        30, true, 48, -- Schedule follow-up in 48 hours
        NOW(), NOW(), 'System', 'System'
    );

    -- Rule 4: Account Issues - Priority 15
    INSERT INTO "EmailAutoReplyRules" (
        "Id", "MailboxId", "Name", "Description",
        "TriggerClassifications", "Action", "ReplyContent",
        "Priority", "IsActive",
        "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy"
    ) VALUES (
        gen_random_uuid(), v_mailbox_id,
        'رد آلي لمشاكل الحساب',
        'يرد تلقائياً على استفسارات ومشاكل الحساب',
        ARRAY[12]::integer[], -- AccountIssue
        0, -- CreateDraft
        '<html><body dir="rtl"><p>عزيزي/عزيزتي،</p><p>تم استلام استفسارك المتعلق بحسابك.</p><p>سيقوم فريق الدعم الفني بمراجعة طلبك والرد عليك خلال 24 ساعة.</p><p>مع أطيب التحيات،<br>فريق الدعم الفني - شاهين</p></body></html>',
        15, true, NOW(), NOW(), 'System', 'System'
    );

    -- Rule 5: General Administrative (Catch-all) - Priority 100
    INSERT INTO "EmailAutoReplyRules" (
        "Id", "MailboxId", "Name", "Description",
        "TriggerClassifications", "Action", "ReplyContent",
        "Priority", "IsActive", "MaxAutoRepliesPerThread",
        "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy"
    ) VALUES (
        gen_random_uuid(), v_mailbox_id,
        'رد آلي عام للإداري',
        'يرد على جميع الاستفسارات الإدارية بشكل عام',
        ARRAY[30, 31, 32, 33, 40, 41, 42]::integer[], -- ContractQuestion, ComplianceQuery, AuditRequest, DocumentRequest, JobApplication, VendorInquiry, MediaInquiry
        1, -- SendImmediately
        '<html><body dir="rtl"><p>عزيزي/عزيزتي،</p><p>تم استلام استفسارك بنجاح.</p><p>سيتم الرد عليك من قبل الفريق المختص في أقرب وقت ممكن.</p><p>مع أطيب التحيات،<br>فريق شاهين للأنظمة</p></body></html>',
        100, true, 2, -- Allow up to 2 auto-replies per thread
        NOW(), NOW(), 'System', 'System'
    );

    RAISE NOTICE '✅ Auto-reply rules created successfully for mailbox: %', v_mailbox_id;
    RAISE NOTICE '📧 Total rules created: 5';
    RAISE NOTICE '1. Administrative (Priority 10) - CreateDraft';
    RAISE NOTICE '2. Forwarded (Priority 20) - SendImmediately';
    RAISE NOTICE '3. Reminders (Priority 30) - SendImmediately + Follow-up';
    RAISE NOTICE '4. Account Issues (Priority 15) - CreateDraft';
    RAISE NOTICE '5. General Admin (Priority 100) - SendImmediately';
END $$;
