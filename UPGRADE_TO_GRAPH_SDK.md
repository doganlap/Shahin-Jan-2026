# 🚀 Upgrade to Microsoft Graph C# SDK

**Current Status**: Using raw HTTP calls  
**Target**: Use official Microsoft Graph C# SDK (already installed v5.100.0)

---

## ✅ What You Already Have

- ✅ `Microsoft.Graph` v5.100.0 installed
- ✅ Using `GraphServiceClient` in `SmtpEmailService.cs`
- ❌ Using raw HTTP in `MicrosoftGraphEmailService.cs`

---

## 🎯 Benefits of Using SDK

1. **Type Safety**: Strongly-typed models instead of JSON parsing
2. **Better Error Handling**: Structured exceptions
3. **Automatic Retry**: Built-in retry policies
4. **IntelliSense**: Better IDE support
5. **Pagination**: Automatic handling of `@odata.nextLink`
6. **Less Code**: SDK handles HTTP details

---

## 📋 Migration Steps

### Step 1: Update MicrosoftGraphEmailService.cs

**Before** (HTTP):
```csharp
var url = $"{GraphBaseUrl}/users/{mailboxId}/mailFolders/{folder}/messages";
var request = new HttpRequestMessage(HttpMethod.Get, url);
var response = await _httpClient.SendAsync(request);
var json = await response.Content.ReadAsStringAsync();
// Manual JSON parsing...
```

**After** (SDK):
```csharp
var messages = await graphClient
    .Users[mailboxId]
    .MailFolders[folder]
    .Messages
    .GetAsync(config => {
        config.QueryParameters.Top = top;
        config.QueryParameters.Orderby = new[] { "receivedDateTime desc" };
        if (since.HasValue) {
            config.QueryParameters.Filter = $"receivedDateTime ge {since.Value:yyyy-MM-ddTHH:mm:ssZ}";
        }
    });
```

---

## 🔧 Implementation Guide

### Initialize GraphServiceClient

```csharp
using Microsoft.Graph;
using Azure.Identity;

var credential = new ClientSecretCredential(
    tenantId,
    clientId,
    clientSecret);

var graphClient = new GraphServiceClient(credential);
```

### List Messages

```csharp
// GET /users/{id}/mailFolders/inbox/messages
var messages = await graphClient
    .Users[mailboxId]
    .MailFolders["inbox"]
    .Messages
    .GetAsync(config => {
        config.QueryParameters.Top = 50;
        config.QueryParameters.Select = new[] { "id", "subject", "sender", "receivedDateTime" };
        config.QueryParameters.Orderby = new[] { "receivedDateTime desc" };
    });
```

### Get Single Message

```csharp
// GET /users/{id}/messages/{messageId}
var message = await graphClient
    .Users[mailboxId]
    .Messages[messageId]
    .GetAsync();
```

### Send Email

```csharp
// POST /users/{id}/sendMail
var message = new Message
{
    Subject = "Test",
    Body = new ItemBody
    {
        ContentType = BodyType.Html,
        Content = "<h1>Hello</h1>"
    },
    ToRecipients = new List<Recipient>
    {
        new Recipient
        {
            EmailAddress = new EmailAddress
            {
                Address = "user@example.com"
            }
        }
    }
};

await graphClient
    .Users[fromEmail]
    .SendMail
    .PostAsync(new SendMailPostRequestBody
    {
        Message = message
    });
```

### Create Subscription (Webhook)

```csharp
// POST /subscriptions
var subscription = new Subscription
{
    ChangeType = "created",
    NotificationUrl = "https://portal.shahin-ai.com/api/webhooks/email",
    Resource = $"/users/{mailboxId}/mailFolders/inbox/messages",
    ExpirationDateTime = DateTimeOffset.UtcNow.AddDays(3),
    ClientState = Guid.NewGuid().ToString()
};

var created = await graphClient
    .Subscriptions
    .PostAsync(subscription);
```

---

## 📦 Required Packages

You already have:
- ✅ `Microsoft.Graph` v5.100.0
- ✅ `Azure.Identity` v1.17.1

No additional packages needed!

---

## 🔄 Refactored Service Example

See the updated `MicrosoftGraphEmailService.cs` file for full implementation using SDK methods.

---

## 📚 Resources

- **SDK Documentation**: https://aka.ms/csharpsdk
- **Graph API Reference**: https://learn.microsoft.com/graph/api/overview
- **SDK GitHub**: https://github.com/microsoftgraph/msgraph-sdk-dotnet
- **Code Snippets**: https://learn.microsoft.com/graph/api/resources/mail-api-overview

---

## ✅ Benefits You'll Get

1. **Less Code**: ~50% reduction in boilerplate
2. **Type Safety**: Compile-time checks instead of runtime JSON errors
3. **Better Performance**: SDK optimizations
4. **Easier Maintenance**: SDK updates automatically handle API changes

---

Would you like me to refactor `MicrosoftGraphEmailService.cs` to use the SDK? 🚀
