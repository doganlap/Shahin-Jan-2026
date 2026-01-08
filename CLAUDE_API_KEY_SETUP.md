# Claude API Key Setup

**Date**: 2026-01-07

---

## 🔑 Claude API Key Configuration

The application uses Claude AI for various features. The API key needs to be configured in the `.env` file.

---

## ✅ Setup Steps

### 1. Get Your Claude API Key

1. Visit: https://console.anthropic.com/
2. Sign in or create an account
3. Navigate to API Keys section
4. Create a new API key
5. Copy the API key (starts with `sk-ant-api03-`)

### 2. Add to .env File

**Option 1: Edit .env file directly**
```bash
# Claude AI API Configuration
CLAUDE_API_KEY=sk-ant-api03-your-actual-key-here
```

**Option 2: Use environment variable**
```bash
export CLAUDE_API_KEY=sk-ant-api03-your-actual-key-here
```

### 3. Restart Application

After adding the key, restart the application:
```bash
docker compose restart grcmvc
```

---

## 📋 Current Configuration

The Claude API key is expected in the `.env` file as:
```bash
CLAUDE_API_KEY=your-key-here
```

**Note**: The placeholder has been added to `.env`. Replace `your-claude-api-key-here` with your actual API key.

---

## ✅ Verification

After adding your API key and restarting:

1. **Check if key is loaded**:
   ```bash
   docker exec grc-system-grcmvc-1 env | grep CLAUDE_API_KEY
   ```

2. **Check application logs**:
   ```bash
   docker compose logs grcmvc | grep -i claude
   ```

3. **Test Claude features** (if available in UI):
   - Navigate to AI-powered features
   - Verify they work without errors

---

## 🔒 Security Notes

⚠️ **Important:**
- ✅ Never commit the `.env` file to git (already in .gitignore)
- ✅ Keep your API key secure
- ✅ Rotate keys regularly
- ✅ Use different keys for dev/staging/production
- ✅ Monitor API usage in Anthropic console

---

## 📝 Current Status

- ✅ Placeholder added to `.env` file
- ⏳ **Action Required**: Replace placeholder with your actual API key
- ⏳ **Action Required**: Restart application after adding key

---

## 🚀 Next Steps

1. Get your Claude API key from https://console.anthropic.com/
2. Edit `.env` file and replace `your-claude-api-key-here` with your actual key
3. Restart application: `docker compose restart grcmvc`
4. Verify it's working

---

**Last Updated**: 2026-01-07
