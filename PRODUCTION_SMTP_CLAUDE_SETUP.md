# 🔧 Production Configuration: SMTP & Claude AI

## Quick Start Checklist

| Service | Required Environment Variables | Status |
|---------|-------------------------------|--------|
| **SMTP (Email)** | `SMTP_PASSWORD` | ❌ Not configured |
| **Claude AI** | `CLAUDE_API_KEY` | ❌ Not configured |
| **Azure AD** | `AZURE_TENANT_ID` | ✅ Already set |

---

## 1️⃣ SMTP Configuration (Microsoft 365)

### Option A: Basic SMTP with App Password (Simpler)

1. **Generate App Password in Microsoft 365:**
   - Go to: https://account.microsoft.com/security
   - Sign in as `info@shahin-ai.com`
   - Navigate to: Security → Advanced Security Options → App Passwords
   - Create new app password for "Shahin AI GRC"
   - Copy the 16-character password

2. **Set Environment Variable:**
```bash
# Add to systemd service file or .env
SMTP_PASSWORD=your-16-char-app-password
```

### Option B: OAuth2 Authentication (Recommended for Production)

1. **Azure Portal Setup:**
   - Go to: https://portal.azure.com → App Registrations
   - Create or select existing app for SMTP
   - Note the **Application (client) ID** → `SMTP_CLIENT_ID`
   - Create a client secret → `SMTP_CLIENT_SECRET`
   - Add API Permission: `Mail.Send` (Application type)
   - Grant admin consent

2. **Set Environment Variables:**
```bash
SMTP_CLIENT_ID=<your-azure-app-client-id>
SMTP_CLIENT_SECRET=<your-azure-app-client-secret>
AZURE_TENANT_ID=c8847e8a-33a0-4b6c-8e01-2e0e6b4aaef5  # Already set
```

### Already Configured (No Changes Needed):
```bash
SMTP_FROM_EMAIL=info@shahin-ai.com
SMTP_USERNAME=info@shahin-ai.com
# Host: smtp.office365.com
# Port: 587
# EnableSsl: true
```

---

## 2️⃣ Claude AI Configuration

### Get Your API Key:

1. **Sign up/Login to Anthropic:**
   - Go to: https://console.anthropic.com
   - Sign in or create account

2. **Create API Key:**
   - Navigate to: Settings → API Keys
   - Click "Create Key"
   - Name it: "Shahin-AI-GRC-Production"
   - Copy the key (starts with `sk-ant-`)

3. **Set Environment Variable:**
```bash
CLAUDE_API_KEY=sk-ant-api03-xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
```

### Already Configured (No Changes Needed):
```json
{
  "ClaudeAgents": {
    "Enabled": true,
    "Model": "claude-sonnet-4-20250514",
    "MaxTokens": 4096,
    "TimeoutSeconds": 60
  }
}
```

---

## 3️⃣ Deploy to Production Server

### Method 1: Systemd Service Environment

```bash
# SSH to production server
ssh root@shahin-ai.com

# Edit the service file
sudo nano /etc/systemd/system/grcmvc.service

# Add/Update Environment variables:
[Service]
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=SMTP_PASSWORD=your-app-password-here
Environment=SMTP_CLIENT_ID=your-client-id
Environment=SMTP_CLIENT_SECRET=your-client-secret
Environment=CLAUDE_API_KEY=sk-ant-api03-your-key-here

# Reload and restart
sudo systemctl daemon-reload
sudo systemctl restart grcmvc
```

### Method 2: Environment File (.env)

```bash
# Create/edit secure env file
sudo nano /var/www/grcmvc/.env.production

# Add:
SMTP_PASSWORD=your-app-password-here
SMTP_CLIENT_ID=your-client-id
SMTP_CLIENT_SECRET=your-client-secret
CLAUDE_API_KEY=sk-ant-api03-your-key-here

# Set permissions
sudo chmod 600 /var/www/grcmvc/.env.production
sudo chown www-data:www-data /var/www/grcmvc/.env.production

# Update service to load env file
# In /etc/systemd/system/grcmvc.service add:
EnvironmentFile=/var/www/grcmvc/.env.production
```

---

## 4️⃣ Test Configuration

### Test Email:
```bash
curl -X POST https://app.shahin-ai.com/api/diagnostics/test-email \
  -H "Authorization: Bearer <your-jwt-token>" \
  -H "Content-Type: application/json" \
  -d '{"toEmail": "your-email@example.com", "subject": "Test Email", "body": "Testing SMTP configuration"}'
```

### Test Claude AI:
```bash
curl -X POST https://app.shahin-ai.com/api/diagnostics/test-ai-agents \
  -H "Authorization: Bearer <your-jwt-token>" \
  -H "Content-Type: application/json" \
  -d '{"prompt": "Hello, can you confirm you are operational?"}'
```

---

## 5️⃣ Verification Commands

```bash
# Check service status
sudo systemctl status grcmvc

# Check environment variables are loaded
sudo systemctl show grcmvc --property=Environment

# View logs for errors
sudo journalctl -u grcmvc -f --since "5 minutes ago"

# Test SMTP connection manually
openssl s_client -connect smtp.office365.com:587 -starttls smtp
```

---

## ⚠️ Security Notes

1. **Never commit** API keys or passwords to git
2. **Rotate Claude API key** every 90 days
3. **Use Azure Key Vault** for production secrets (recommended)
4. **Set restrictive file permissions** on `.env` files (600)
5. **Monitor API usage** at console.anthropic.com/usage

---

## 📋 Complete Environment Variables Summary

```bash
# REQUIRED FOR EMAIL
SMTP_FROM_EMAIL=info@shahin-ai.com          # ✅ Already set
SMTP_USERNAME=info@shahin-ai.com             # ✅ Already set
SMTP_PASSWORD=<YOUR_APP_PASSWORD>            # ❌ NEED TO SET
SMTP_CLIENT_ID=<AZURE_APP_CLIENT_ID>         # ❌ Optional (for OAuth2)
SMTP_CLIENT_SECRET=<AZURE_APP_SECRET>        # ❌ Optional (for OAuth2)

# REQUIRED FOR CLAUDE AI
CLAUDE_API_KEY=sk-ant-api03-xxxxx            # ❌ NEED TO SET

# SHARED AZURE (Already configured)
AZURE_TENANT_ID=c8847e8a-33a0-4b6c-8e01-2e0e6b4aaef5  # ✅ Already set

# MICROSOFT GRAPH (For advanced email features)
MSGRAPH_CLIENT_ID=4e2575c6-e269-48eb-b055-ad730a2150a7  # ✅ Already set
MSGRAPH_CLIENT_SECRET=<YOUR_SECRET>          # ❌ NEED TO SET
```

---

## 🚀 Next Steps

1. [ ] Obtain SMTP app password or OAuth2 credentials
2. [ ] Create Claude API key at console.anthropic.com
3. [ ] SSH to production and update environment variables
4. [ ] Restart the grcmvc service
5. [ ] Test email using `/api/diagnostics/test-email`
6. [ ] Test AI using `/api/diagnostics/test-ai-agents`

---

*Last Updated: 2026-01-11*
