# 🔌 Microsoft Graph Toolkit & Adaptive Cards Integration Guide

**Purpose**: Integrate Microsoft Graph Toolkit and Adaptive Cards with your email auto-reply system

---

## 🎯 What You Can Do

1. **Frontend UI**: Use Microsoft Graph Toolkit components to display emails in Blazor UI
2. **Rich Notifications**: Send Adaptive Cards via email/Teams for email notifications
3. **Interactive Email Alerts**: Create adaptive card notifications when emails arrive

---

## 📦 Part 1: Microsoft Graph Toolkit for Blazor Frontend

### Installation

Add Microsoft Graph Toolkit to your Blazor project:

```bash
cd /home/Shahin-ai/Shahin-Jan-2026/src/GrcMvc
dotnet add package Microsoft.Graph --version 5.62.0
```

For Blazor frontend (if using separate client project):
```bash
npm install @microsoft/mgt-components @microsoft/mgt-msal2-provider
```

### Usage in Blazor

Reference: [Microsoft Graph Toolkit Documentation](https://github.com/microsoftgraph/microsoft-graph-toolkit)

**Example: Display Email Threads**

```razor
@page "/email/threads"
@using Microsoft.Graph

<h3>Email Threads</h3>

<mgt-agenda></mgt-agenda>

<mgt-todo></mgt-todo>

<script type="module">
  import { Providers } from 'https://unpkg.com/@microsoft/mgt@4';
  import { Msal2Provider } from 'https://unpkg.com/@microsoft/mgt-msal2-provider@4';
  
  Providers.globalProvider = new Msal2Provider({
    clientId: '@Configuration["EmailOperations:MicrosoftGraph:ClientId"]',
    authority: 'https://login.microsoftonline.com/@Configuration["EmailOperations:MicrosoftGraph:TenantId"]'
  });
</script>
```

---

## 🎴 Part 2: Adaptive Cards for Email Notifications

### Create Adaptive Card Service

Create a new service to generate Adaptive Cards for email notifications:

**File**: `src/GrcMvc/Services/EmailOperations/AdaptiveCardEmailService.cs`

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GrcMvc.Services.EmailOperations;

public class AdaptiveCardEmailService
{
    private readonly ILogger<AdaptiveCardEmailService> _logger;

    public AdaptiveCardEmailService(ILogger<AdaptiveCardEmailService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Generate Adaptive Card for new email notification
    /// </summary>
    public string GenerateEmailNotificationCard(EmailMessage email, EmailThread thread)
    {
        var card = new
        {
            type = "AdaptiveCard",
            body = new object[]
            {
                new
                {
                    type = "TextBlock",
                    size = "Medium",
                    weight = "Bolder",
                    text = $"📧 New Email: {email.Subject}"
                },
                new
                {
                    type = "ColumnSet",
                    columns = new object[]
                    {
                        new
                        {
                            type = "Column",
                            items = new object[]
                            {
                                new
                                {
                                    type = "TextBlock",
                                    weight = "Bolder",
                                    text = "From:",
                                    wrap = true
                                },
                                new
                                {
                                    type = "TextBlock",
                                    text = email.FromName ?? email.FromEmail,
                                    wrap = true
                                }
                            },
                            width = "stretch"
                        },
                        new
                        {
                            type = "Column",
                            items = new object[]
                            {
                                new
                                {
                                    type = "TextBlock",
                                    weight = "Bolder",
                                    text = "Received:",
                                    wrap = true
                                },
                                new
                                {
                                    type = "TextBlock",
                                    text = email.ReceivedAt.ToString("yyyy-MM-dd HH:mm"),
                                    wrap = true
                                }
                            },
                            width = "auto"
                        }
                    }
                },
                new
                {
                    type = "TextBlock",
                    text = email.BodyPreview ?? "No preview available",
                    wrap = true,
                    maxLines = 3
                },
                new
                {
                    type = "FactSet",
                    facts = new object[]
                    {
                        new { title = "Thread ID:", value = thread.Id.ToString() },
                        new { title = "Status:", value = thread.Status.ToString() },
                        new { title = "Priority:", value = thread.Priority.ToString() },
                        new { title = "Classification:", value = thread.Classification.ToString() }
                    }
                }
            },
            actions = new object[]
            {
                new
                {
                    type = "Action.OpenUrl",
                    title = "View in Portal",
                    url = $"https://portal.shahin-ai.com/email/threads/{thread.Id}"
                },
                new
                {
                    type = "Action.Submit",
                    title = "Mark as Read",
                    data = new
                    {
                        action = "markAsRead",
                        threadId = thread.Id.ToString(),
                        messageId = email.Id.ToString()
                    }
                }
            },
            version = "1.6",
            $schema = "http://adaptivecards.io/schemas/adaptive-card.json"
        };

        return JsonSerializer.Serialize(card, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
    }

    /// <summary>
    /// Generate Adaptive Card for auto-reply notification
    /// </summary>
    public string GenerateAutoReplyCard(string recipientEmail, string subject, string replyContent, string ruleName)
    {
        var card = new
        {
            type = "AdaptiveCard",
            body = new object[]
            {
                new
                {
                    type = "TextBlock",
                    size = "Medium",
                    weight = "Bolder",
                    text = "✅ Auto-Reply Sent"
                },
                new
                {
                    type = "TextBlock",
                    text = $"An automatic reply was sent for: **{subject}**",
                    wrap = true
                },
                new
                {
                    type = "FactSet",
                    facts = new object[]
                    {
                        new { title = "To:", value = recipientEmail },
                        new { title = "Rule:", value = ruleName },
                        new { title = "Sent At:", value = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC") }
                    }
                },
                new
                {
                    type = "Container",
                    items = new object[]
                    {
                        new
                        {
                            type = "TextBlock",
                            text = "Reply Content:",
                            weight = "Bolder",
                            spacing = "Medium"
                        },
                        new
                        {
                            type = "TextBlock",
                            text = replyContent.Replace("<html>", "").Replace("</html>", "").Replace("<body>", "").Replace("</body>", "").Replace("<p>", "").Replace("</p>", "<br>").Replace("<br>", "\n"),
                            wrap = true,
                            maxLines = 10
                        }
                    }
                }
            },
            version = "1.6",
            $schema = "http://adaptivecards.io/schemas/adaptive-card.json"
        };

        return JsonSerializer.Serialize(card, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
    }
}
```

---

## 📧 Part 3: Send Adaptive Cards via Email

### Update EmailProcessingJob to Send Adaptive Card Notifications

Modify `EmailProcessingJob.cs` to send Adaptive Card notifications:

```csharp
using GrcMvc.Services.EmailOperations;

// Add this method to EmailProcessingJob
private async Task SendAdaptiveCardNotificationAsync(
    EmailMailbox mailbox, 
    EmailThread thread, 
    EmailMessage message,
    EmailAutoReplyRule? matchedRule = null)
{
    try
    {
        var adaptiveCardService = _serviceProvider.GetRequiredService<AdaptiveCardEmailService>();
        
        // Generate Adaptive Card
        string cardJson;
        string emailSubject;
        
        if (matchedRule != null)
        {
            // Auto-reply notification
            cardJson = adaptiveCardService.GenerateAutoReplyCard(
                message.FromEmail,
                message.Subject,
                matchedRule.ReplyContent ?? "",
                matchedRule.Name
            );
            emailSubject = $"✅ Auto-Reply Sent: {message.Subject}";
        }
        else
        {
            // New email notification
            cardJson = adaptiveCardService.GenerateEmailNotificationCard(message, thread);
            emailSubject = $"📧 New Email: {message.Subject}";
        }

        // Send via Microsoft Graph (email with Adaptive Card as HTML attachment)
        var token = await _graphService.GetAccessTokenAsync(
            mailbox.TenantId!,
            mailbox.ClientId!,
            DecryptSecret(mailbox.EncryptedClientSecret!));

        // Convert Adaptive Card to HTML for email body
        var htmlBody = $@"
<html>
<body dir=""rtl"">
    <h2>Email Notification</h2>
    <p>Adaptive Card JSON:</p>
    <pre>{cardJson}</pre>
    <p><a href='https://portal.shahin-ai.com/email/threads/{thread.Id}'>View in Portal</a></p>
</body>
</html>";

        // Send notification email (to admin/yourself)
        await _graphService.SendEmailAsync(
            token,
            "ahmet.dogan@doganconsult.com", // Your notification email
            emailSubject,
            htmlBody
        );

        _logger.LogInformation("Adaptive Card notification sent for thread {ThreadId}", thread.Id);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to send Adaptive Card notification");
    }
}

// Call this in ProcessNewEmailAsync after auto-reply processing
await SendAdaptiveCardNotificationAsync(mailbox, thread, emailMessage, matchedRule);
```

---

## 🎯 Part 4: Teams Integration with Adaptive Cards

### Send Adaptive Cards to Microsoft Teams

Update `TeamsNotificationService.cs` to support Adaptive Cards:

```csharp
public async Task<bool> SendAdaptiveCardAsync(string cardJson, string webhookUrl)
{
    try
    {
        var payload = new
        {
            type = "message",
            attachments = new[]
            {
                new
                {
                    contentType = "application/vnd.microsoft.card.adaptive",
                    content = JsonSerializer.Deserialize<object>(cardJson)
                }
            }
        };

        var content = new StringContent(
            JsonSerializer.Serialize(payload), 
            Encoding.UTF8, 
            "application/json");

        var response = await _httpClient.PostAsync(webhookUrl, content);

        return response.IsSuccessStatusCode;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to send Adaptive Card to Teams");
        return false;
    }
}
```

---

## 📋 Quick Setup Checklist

### 1. Install Dependencies

```bash
# Backend (if needed)
dotnet add package System.Text.Json

# Frontend (Blazor client)
npm install @microsoft/mgt-components @microsoft/mgt-msal2-provider
```

### 2. Register Services

In `Program.cs`:

```csharp
builder.Services.AddScoped<AdaptiveCardEmailService>();
```

### 3. Update EmailProcessingJob

Add Adaptive Card notification sending after email processing.

### 4. Test

1. Send test email to `info@doganconsult.com`
2. Check for Adaptive Card notification
3. Verify Adaptive Card displays correctly

---

## 🔗 Resources

- **Microsoft Graph Toolkit**: https://github.com/microsoftgraph/microsoft-graph-toolkit
- **Adaptive Cards Designer**: https://adaptivecards.io/designer/
- **Adaptive Cards Documentation**: https://adaptivecards.io/
- **Graph Toolkit Docs**: https://docs.microsoft.com/graph/toolkit/overview

---

## ✅ Benefits

1. **Rich UI**: Display emails in modern, interactive components
2. **Better Notifications**: Adaptive Cards provide structured, actionable notifications
3. **Integration**: Works with Teams, Outlook, and web apps
4. **User Experience**: Better than plain text emails

---

## 🎯 Next Steps

1. ✅ Create `AdaptiveCardEmailService.cs`
2. ✅ Register service in `Program.cs`
3. ✅ Update `EmailProcessingJob` to send Adaptive Cards
4. ✅ Test with sample email
5. ✅ (Optional) Add Graph Toolkit components to Blazor UI

Would you like me to create these files now? 🚀
