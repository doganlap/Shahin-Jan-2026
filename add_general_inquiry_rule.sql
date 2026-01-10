-- Add General Inquiry Auto-Reply Rule
-- This rule catches emails that don't match specific rules but are legitimate inquiries

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

    -- Check if rule already exists
    IF EXISTS (
        SELECT 1 FROM "EmailAutoReplyRules"
        WHERE "MailboxId" = v_mailbox_id
        AND "Name" = 'رد آلي للاستفسارات العامة'
    ) THEN
        RAISE NOTICE 'General inquiry rule already exists, skipping...';
        RETURN;
    END IF;

    -- Add General Inquiry Rule
    INSERT INTO "EmailAutoReplyRules" (
        "Id", "MailboxId", "Name", "Description",
        "TriggerClassifications", "Action", "ReplyContent", "UseAiGeneration",
        "Priority", "IsActive", "MaxAutoRepliesPerThread",
        "CreatedDate", "IsDeleted"
    ) VALUES (
        gen_random_uuid(),
        v_mailbox_id,
        'رد آلي للاستفسارات العامة',
        'يرد على الاستفسارات العامة التي لا تطابق قواعد محددة',
        ARRAY[0, 10, 11, 13, 14, 20, 21, 22, 23]::integer[], -- Unclassified, TechnicalSupport, BillingInquiry, FeatureRequest, BugReport, QuoteRequest, DemoRequest, PricingInquiry, PartnershipInquiry
        0, -- CreateDraft (for safety - review before sending)
        '<html><body dir="rtl"><p>عزيزي/عزيزتي،</p><p>شكراً لتواصلك معنا.</p><p>تم استلام رسالتك وسيتم الرد عليك من قبل الفريق المختص في أقرب وقت ممكن.</p><p>مع أطيب التحيات،<br>فريق شاهين للأنظمة</p></body></html>',
        false,
        50, -- Medium priority (after specific rules, before catch-all)
        true,
        1,
        NOW(),
        false
    );

    RAISE NOTICE '✅ General inquiry rule added successfully!';
END $$;

-- Verify
SELECT 
    r."Name",
    r."Priority",
    CASE r."Action"
        WHEN 0 THEN 'CreateDraft'
        WHEN 1 THEN 'SendImmediately'
        ELSE 'Other'
    END as "Action",
    r."IsActive"
FROM "EmailAutoReplyRules" r
JOIN "EmailMailboxes" m ON r."MailboxId" = m."Id"
WHERE m."EmailAddress" = 'info@doganconsult.com'
  AND r."IsDeleted" = false
ORDER BY r."Priority";
