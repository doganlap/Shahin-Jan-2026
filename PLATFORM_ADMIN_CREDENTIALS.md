# Platform Admin Credentials

**Date**: 2026-01-07
**Application Status**: ✅ Running (Health Check: Healthy)

---

## 🔐 Available Admin Users

### User 1: Platform Owner (Active) ✅

**Login Credentials:**
- **Email/Username**: `Dooganlap@gmail.com`
- **Password**: (Check original setup or reset if needed)
- **Name**: Platform Owner
- **Roles**: Admin, Owner, PlatformAdmin
- **Status**: ✅ Active, Email Confirmed, Password Set

**Access**: This user has full platform access with Admin, Owner, and PlatformAdmin roles.

---

### User 2: Ahmet Dogan (To Be Created)

**Target Credentials:**
- **Email/Username**: `ahmet.dogan@doganconsult.com`
- **Password**: `DogCon@Admin2026`
- **Name**: Ahmet Dogan
- **Role**: PlatformAdmin
- **Department**: Administration
- **Job Title**: Platform Administrator
- **Status**: ⚠️ User creation pending (default tenant issue resolved)

**Note**: User creation requires a default tenant in the database. Tenant has been created, user will be created on next restart or via API.

---

## 🌐 Access URLs

- **Application URL**: http://localhost:8888
- **Login Page**: http://localhost:8888/Account/Login
- **Health Check**: http://localhost:8888/health
- **API Health**: http://localhost:8888/api/system/health

---

## 📋 Login Instructions

### For Existing Admin (Dooganlap@gmail.com)
1. Navigate to: http://localhost:8888/Account/Login
2. Enter email: `Dooganlap@gmail.com`
3. Enter password: (Use original password or reset)
4. Click Login

### For New Admin (ahmet.dogan@doganconsult.com)
1. Navigate to: http://localhost:8888/Account/Login
2. Enter email: `ahmet.dogan@doganconsult.com`
3. Enter password: `DogCon@Admin2026`
4. Click Login

**Note**: If user is not created yet, see "Create User" section below.

---

## 🔧 Create Ahmet Dogan User

### Option 1: Via API Endpoint (Recommended)
```bash
curl -X POST http://localhost:8888/api/seed/users/create \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Ahmet",
    "lastName": "Dogan",
    "email": "ahmet.dogan@doganconsult.com",
    "password": "DogCon@Admin2026",
    "department": "Administration",
    "jobTitle": "Platform Administrator",
    "roleName": "PlatformAdmin"
  }'
```

### Option 2: Restart Application
The user will be created automatically when `ApplicationInitializer` runs:
```bash
docker compose restart grcmvc
# Wait 30 seconds for initialization
```

### Option 3: Check Logs for Status
```bash
docker compose logs grcmvc --tail 100 | grep -i "ahmet\|dogan\|createuser"
```

---

## ✅ Verification Commands

### Check All Users
```bash
docker exec grc-db psql -U postgres -d GrcAuthDb -c \
  "SELECT u.\"UserName\", u.\"Email\", u.\"FirstName\", u.\"LastName\", r.\"Name\" as \"Role\" FROM \"AspNetUsers\" u LEFT JOIN \"AspNetUserRoles\" ur ON u.\"Id\" = ur.\"UserId\" LEFT JOIN \"AspNetRoles\" r ON ur.\"RoleId\" = r.\"Id\" ORDER BY u.\"CreatedDate\" DESC;"
```

### Check if Ahmet Dogan User Exists
```bash
docker exec grc-db psql -U postgres -d GrcAuthDb -c \
  "SELECT \"UserName\", \"Email\", \"FirstName\", \"LastName\" FROM \"AspNetUsers\" WHERE \"Email\" = 'ahmet.dogan@doganconsult.com';"
```

### Check Default Tenant
```bash
docker exec grc-db psql -U postgres -d GrcMvcDb -c \
  "SELECT \"Id\", \"TenantSlug\", \"Name\" FROM \"Tenants\" WHERE \"TenantSlug\" = 'default';"
```

### Check Available Roles
```bash
docker exec grc-db psql -U postgres -d GrcAuthDb -c \
  "SELECT \"Name\", \"NormalizedName\" FROM \"AspNetRoles\" ORDER BY \"Name\";"
```

---

## 📊 Current System Status

- **Database**: ✅ Connected (GrcAuthDb, GrcMvcDb)
- **Application**: ✅ Running (Health: Healthy)
- **Container**: ✅ Up (grc-system-grcmvc-1)
- **Port**: ✅ 8888 (HTTP), 8443 (HTTPS)
- **Default Tenant**: ✅ Created
- **Roles Available**: Admin, Owner, PlatformAdmin, TenantAdmin, ComplianceOfficer, RiskManager, Auditor, User

---

## 🔍 Troubleshooting

### If Ahmet Dogan User Not Created:

1. **Verify Default Tenant Exists**
   ```bash
   docker exec grc-db psql -U postgres -d GrcMvcDb -c "SELECT * FROM \"Tenants\" WHERE \"TenantSlug\" = 'default';"
   ```

2. **Create Default Tenant (if missing)**
   ```bash
   docker exec grc-db psql -U postgres -d GrcMvcDb -c "INSERT INTO \"Tenants\" (\"Id\", \"TenantSlug\", \"Name\", \"IsDeleted\", \"CreatedAt\", \"ModifiedAt\") VALUES (gen_random_uuid(), 'default', 'Default Tenant', false, NOW(), NOW()) ON CONFLICT DO NOTHING;"
   ```

3. **Check Application Logs**
   ```bash
   docker compose logs grcmvc --tail 100 | grep -i "tenant\|error\|ahmet"
   ```

4. **Create User via API**
   - Use the API endpoint shown above

5. **Restart Application**
   ```bash
   docker compose restart grcmvc
   sleep 30
   ```

---

## 📝 Available Roles

The following roles are available in the system:

1. **Admin** - System administrator
2. **Owner** - Platform owner
3. **PlatformAdmin** - Platform administrator
4. **TenantAdmin** - Tenant administrator
5. **ComplianceOfficer** - Compliance officer
6. **RiskManager** - Risk manager
7. **Auditor** - Auditor
8. **User** - Standard user

---

## 🔒 Security Notes

⚠️ **Important Security Reminders:**
- These are development credentials
- Change passwords in production environment
- Store credentials securely (use password manager)
- Never commit credentials to git
- Use environment variables for production secrets
- Enable 2FA in production
- Rotate passwords regularly

---

## 📋 User Details Summary

### Currently Active Users

| Email | Name | Roles | Status |
|-------|------|-------|--------|
| Dooganlap@gmail.com | Platform Owner | Admin, Owner, PlatformAdmin | ✅ Active |

### To Be Created

| Email | Name | Role | Password |
|-------|------|------|----------|
| ahmet.dogan@doganconsult.com | Ahmet Dogan | PlatformAdmin | DogCon@Admin2026 |

---

**Last Updated**: 2026-01-07
**Application Version**: 2.0.0
**Database**: PostgreSQL 15 (GrcAuthDb, GrcMvcDb)
