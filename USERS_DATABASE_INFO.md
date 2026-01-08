# Users Tables Database Information

## Answer: Users Tables are in `GrcAuthDb` Database

### Database Configuration

**Two Separate Databases:**

1. **`GrcMvcDb`** (Main Application Database)
   - **Connection String**: `DefaultConnection`
   - **DbContext**: `GrcDbContext`
   - **Contains**: Application business data, entities, workflows, etc.
   - **Location**: Same PostgreSQL server, different database

2. **`GrcAuthDb`** (Authentication/Identity Database) ⭐
   - **Connection String**: `GrcAuthDb`
   - **DbContext**: `GrcAuthDbContext`
   - **Contains**: **Users tables (ASP.NET Identity)**
   - **Location**: Same PostgreSQL server, different database

### Users Tables Location

**Database**: `GrcAuthDb`

**Tables** (ASP.NET Identity standard tables):
- `AspNetUsers` - User accounts (ApplicationUser)
- `AspNetRoles` - Roles
- `AspNetUserRoles` - User-Role assignments
- `AspNetUserClaims` - User claims
- `AspNetRoleClaims` - Role claims
- `AspNetUserLogins` - External login providers
- `AspNetUserTokens` - Authentication tokens

### Configuration in Code

**Program.cs** (lines 200-203):
```csharp
// Register Auth DbContext for Identity (separate database)
var authConnectionString = builder.Configuration.GetConnectionString("GrcAuthDb") ?? connectionString;
builder.Services.AddDbContext<GrcAuthDbContext>(options =>
    options.UseNpgsql(authConnectionString), ServiceLifetime.Scoped);
```

**appsettings.json**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=GrcMvcDb;Username=postgres;Password=postgres;Port=5433",
    "GrcAuthDb": "Host=localhost;Database=GrcAuthDb;Username=postgres;Password=postgres;Port=5433"
  }
}
```

### Why Separate Databases?

From `GrcAuthDbContext.cs` comments:
- **Security isolation**: Authentication data separated from application data
- **Scalability**: Can scale/auth databases independently
- **Compliance**: Easier to apply different security policies/backups

### Current Status

**Database Exists**: ✅ `GrcAuthDb` database exists in PostgreSQL

**Tables**: ⚠️ **No tables found yet** - Migrations may not have been run

**To Verify/Migrate**:
```bash
# Check if migrations exist
ls src/GrcMvc/Data/Migrations/Auth/

# Run migrations for auth database
dotnet ef database update --context GrcAuthDbContext
```

### Connection String Reference

**For Docker Container** (from inside):
```
Host=db;Database=GrcAuthDb;Username=postgres;Password=postgres;Port=5432
```

**From Host Machine**:
```
Host=localhost;Database=GrcAuthDb;Username=postgres;Password=postgres;Port=5433
```

### Quick Verification

```bash
# List all databases
docker exec grc-db psql -U postgres -l | grep Grc

# Check tables in GrcAuthDb
docker exec grc-db psql -U postgres -d GrcAuthDb -c "\dt"

# Check tables in GrcMvcDb  
docker exec grc-db psql -U postgres -d GrcMvcDb -c "\dt"
```

## Summary

✅ **Users tables are in**: `GrcAuthDb` database
✅ **Connection string name**: `GrcAuthDb`
✅ **DbContext**: `GrcAuthDbContext`
✅ **Database exists**: Yes
⚠️ **Tables**: May need migrations run
