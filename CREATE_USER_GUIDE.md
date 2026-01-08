# Create User Guide

## Users Database

**Database**: `GrcAuthDb`  
**Tables**: ASP.NET Identity tables (AspNetUsers, AspNetRoles, etc.)

## How to Create Users

### Option 1: Via API Endpoint (Recommended)

I've created an API endpoint to create users. Once you provide the names, I'll call it.

**Endpoint**: `POST /api/seed/users/create`

**Request Body**:
```json
{
  "firstName": "Ahmed",
  "lastName": "Mohammed",
  "email": "ahmed@example.com",
  "password": "TempPassword123!",
  "department": "IT",
  "jobTitle": "Developer",
  "roleName": "TenantAdmin",
  "tenantId": null
}
```

**Example**:
```bash
curl -X POST http://localhost:8888/api/seed/users/create \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Ahmed",
    "lastName": "Mohammed",
    "email": "ahmed@example.com",
    "password": "TempPassword123!",
    "department": "IT",
    "jobTitle": "Developer"
  }'
```

### Option 2: Via Application Code

Use the `CreateUserHelper` class:
```csharp
await CreateUserHelper.CreateUserAsync(
    userManager,
    context,
    logger,
    "Ahmed",
    "Mohammed",
    "ahmed@example.com",
    "TempPassword123!",
    department: "IT",
    jobTitle: "Developer",
    roleName: "TenantAdmin"
);
```

## Password Requirements

Based on `Program.cs` configuration:
- **Minimum length**: 12 characters
- **Requires**: Uppercase, lowercase, digit, special character
- **Example**: `TempPassword123!`

## What Happens When User is Created

1. ✅ User created in `GrcAuthDb.AspNetUsers` table
2. ✅ User linked to default tenant (or specified tenant)
3. ✅ Role assigned (if provided)
4. ✅ TenantUser record created linking user to tenant
5. ✅ Password set with `MustChangePassword = true` (forces change on first login)

## Before Creating Users

**Important**: Make sure migrations have been run for `GrcAuthDb`:

```bash
# From host machine (not container)
cd /home/dogan/grc-system
export ConnectionStrings__GrcAuthDb="Host=localhost;Database=GrcAuthDb;Username=postgres;Password=postgres;Port=5433"
dotnet ef database update --context GrcAuthDbContext --project src/GrcMvc/GrcMvc.csproj
```

**Or** let the application run migrations automatically on startup (if configured).

## Ready to Create Users

**Please provide**:
- First Name
- Last Name  
- Email
- (Optional) Department
- (Optional) Job Title
- (Optional) Role Name (e.g., "TenantAdmin", "ComplianceManager")

I'll create them using the API endpoint!
