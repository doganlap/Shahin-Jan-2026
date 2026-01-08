# Password Information

**Date**: 2026-01-07

---

## 🔐 Admin Account Passwords

### User 1: Dooganlap@gmail.com (Existing User)
- **Email**: `Dooganlap@gmail.com`
- **Password**: Unknown (password is hashed in database)
- **Status**: Active

**To Reset Password:**
1. Use password reset feature at: http://localhost:8888/Account/ForgotPassword
2. Or reset via database (see below)

---

### User 2: Ahmet Dogan (New Admin User)
- **Email**: `ahmet.dogan@doganconsult.com`
- **Password**: `DogCon@Admin2026`
- **Status**: Being created

---

## 🔧 Reset Password for Existing User

### Option 1: Via Application UI
1. Go to: http://localhost:8888/Account/ForgotPassword
2. Enter email: `Dooganlap@gmail.com`
3. Follow password reset instructions

### Option 2: Via API (if available)
```bash
curl -X POST http://localhost:8888/api/account/reset-password \
  -H "Content-Type: application/json" \
  -d '{"email": "Dooganlap@gmail.com"}'
```

### Option 3: Database Reset (Manual)
**WARNING**: This requires generating a new password hash. Recommended to use UI reset instead.

---

## 📋 Quick Login

### For Ahmet Dogan (New User):
1. **URL**: http://localhost:8888/Account/Login
2. **Email**: `ahmet.dogan@doganconsult.com`
3. **Password**: `DogCon@Admin2026`

### For Dooganlap@gmail.com (Existing User):
1. **URL**: http://localhost:8888/Account/Login
2. **Email**: `Dooganlap@gmail.com`
3. **Password**: Use password reset if you don't remember

---

## ✅ Current Users

| Email | Password | Status |
|-------|----------|--------|
| Dooganlap@gmail.com | (Unknown - use reset) | ✅ Active |
| ahmet.dogan@doganconsult.com | `DogCon@Admin2026` | ⏳ Creating |

---

**Last Updated**: 2026-01-07
