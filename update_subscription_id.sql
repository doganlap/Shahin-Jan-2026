-- Update Webhook Subscription ID in Database
-- Run this AFTER you get the subscription ID from Microsoft Graph Explorer

-- Replace 'YOUR_SUBSCRIPTION_ID_HERE' with the actual ID from Graph Explorer response

UPDATE "EmailMailboxes"
SET 
    "WebhookSubscriptionId" = 'YOUR_SUBSCRIPTION_ID_HERE',
    "WebhookExpiresAt" = '2026-02-22T00:00:00Z',
    "ModifiedDate" = NOW()
WHERE "EmailAddress" = 'info@doganconsult.com';

-- Verify
SELECT 
    "EmailAddress",
    "GraphUserId",
    "WebhookSubscriptionId",
    "WebhookExpiresAt",
    "IsActive",
    "AutoReplyEnabled"
FROM "EmailMailboxes"
WHERE "EmailAddress" = 'info@doganconsult.com';
