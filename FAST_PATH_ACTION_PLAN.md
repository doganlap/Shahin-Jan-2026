# Fast Path to "Full GRC Lifecycle" Readiness

## Current Status Summary

✅ **Completed**:
1. ✅ Dynamic permission policy provider implemented
2. ✅ Routing gaps fixed (Plans, TenantAdmin, Admin, Subscriptions)
3. ✅ Health check enhanced with detailed error reporting
4. ✅ DB connection verification script created
5. ✅ Comprehensive documentation created

## Action Plan

### Step 1: Stop the 500s ✅ COMPLETE

**Status**: ✅ Implemented
- `PermissionPolicyProvider` creates policies on-demand for `[Authorize("Grc.*")]` attributes
- `PermissionAuthorizationHandler` checks permission claims with Admin/Owner fallback
- Registered in `Program.cs` (lines 358-361)

**Verification**:
- Endpoints should now return `302` (redirect) or `401/403` instead of `500`
- Test: `curl -I http://localhost:8888/Risk` should return `302` or `401`, not `500`

### Step 2: Restore DB Connectivity ⚠️ IN PROGRESS

**Status**: ⚠️ Configuration issue identified

**Current Situation**:
- ✅ Database container is running and healthy
- ✅ Internal container connections work
- ❌ Host-based connections fail (password authentication error)
- ✅ Connection string format verified: `Host=db;Port=5432` (correct for Docker)

**Actions Required**:

1. **Verify password matches**:
   ```bash
   # Test internal connection
   docker exec grc-db psql -U postgres -d GrcMvcDb -c "SELECT current_user;"
   ```

2. **If password mismatch, reset**:
   ```bash
   docker exec grc-db psql -U postgres -c "ALTER USER postgres PASSWORD 'postgres';"
   ```

3. **Verify `.env` configuration**:
   ```bash
   # Check current settings
   grep CONNECTION_STRING .env
   # Should show: Host=db;Database=GrcMvcDb;Username=postgres;Password=postgres;Port=5432
   ```

4. **Restart application**:
   ```bash
   docker-compose restart grcmvc
   ```

5. **Verify health endpoint**:
   ```bash
   curl http://localhost:8888/api/system/health
   # Expected: {"status":"healthy",...}
   ```

**Verification Script**:
```bash
./scripts/verify-db-connection.sh
```

**Expected Result**:
- `/api/system/health` returns `200 OK` with `"status": "healthy"`
- All data-dependent endpoints can load data
- No more `28P01` errors in logs

**Documentation**: See `DB_CONNECTION_GUIDE.md` for detailed troubleshooting

### Step 3: Seed Minimum Viable Security Model 📋 TODO

**Actions Required**:

1. **Ensure roles exist with permissions**:
   - Admin role with all permissions
   - Compliance Manager with framework/assessment permissions
   - Risk Manager with risk/action plan permissions
   - Evidence Officer with evidence upload permissions

2. **Create test user with permissions**:
   - User should have permission claims populated
   - Or user should be in a role that grants permissions

3. **Verify login produces permission claims**:
   - After login, check user claims contain `permission` claims
   - Or ensure role-based fallback works (Admin/Owner roles)

**Code to Check**:
- `GrcDataSeedContributor.cs` - Seed data for roles/permissions
- `Login/Claims generation` - Where permission claims are added to user principal
- `PermissionAuthorizationHandler.cs` - How claims are checked

**Verification**:
```bash
# After login, check user claims (via debug or logging)
# Should see claims with Type="permission" or Type="permissions"
```

### Step 4: UI Routing Alignment ✅ COMPLETE

**Status**: ✅ Fixed

**Routes Fixed**:
- ✅ `/Admin` - Working (AdminController)
- ✅ `/TenantAdmin` - Redirect added to `/t/{slug}/admin`
- ✅ `/subscriptions` - Index action exists
- ✅ `/plans` - MVC controller created

**Remaining UI Pages** (may not be implemented yet):
- CatalogAdmin, RoleDelegation, Analytics, AssessmentTemplate
- DocumentFlow, KRIDashboard, AuditPackage, Invitation

**Action**: Compare `_Layout.cshtml` menu items to actual controllers
- Either create missing controllers/views
- Or update menu items to point to existing routes
- Or remove menu items until features are delivered

**Verification**:
- Test all menu links in navigation
- Verify 404s are for intentional "not implemented" vs routing bugs

### Step 5: Feature Flags Review 📋 TODO

**Current Status**: Conservative (mostly off)

**Location**: `appsettings.json` and `appsettings.Development.json`

**Flags to Review**:
```json
"GrcFeatureFlags": {
  "UseSecurePasswordGeneration": false,
  "UseSessionBasedClaims": false,
  "UseEnhancedAuditLogging": false,
  "UseDeterministicTenantResolution": false,
  "DisableDemoLogin": false,
  "CanaryPercentage": 0,
  "VerifyConsistency": false,
  "LogFeatureFlagDecisions": true
}
```

**Recommendation**:
- Keep conservative until baseline is working
- After DB connectivity verified, gradually enable features
- Test each feature flag independently

**After Baseline Working**:
1. Enable `UseSessionBasedClaims` for better permission management
2. Enable `UseEnhancedAuditLogging` for production readiness
3. Enable `UseSecurePasswordGeneration` for security
4. Keep `DisableDemoLogin: false` until production

## Verification Checklist

### Immediate (Required for baseline)
- [ ] DB connection working (`/api/system/health` returns healthy)
- [ ] Permission policies resolve (no 500 errors, get 302/401/403)
- [ ] Core routes accessible (`/Risk`, `/Control`, `/Policy`, `/Audit`)
- [ ] Login works and produces valid user principal

### Short-term (Next sprint)
- [ ] All menu links work or are intentionally removed
- [ ] Seed data includes roles with permissions
- [ ] Test user can access assigned modules
- [ ] Audit logging works (if enabled)

### Before Production
- [ ] Feature flags reviewed and set appropriately
- [ ] Demo login disabled
- [ ] Secure password generation enabled
- [ ] Enhanced audit logging enabled
- [ ] Multi-tenant resolution tested
- [ ] All routing gaps closed

## Quick Reference

**Test DB Connection**:
```bash
./scripts/verify-db-connection.sh
```

**Check Health**:
```bash
curl http://localhost:8888/api/system/health
```

**View Logs**:
```bash
docker-compose logs grcmvc | tail -50
docker-compose logs grcmvc | grep -i "connection\|28P01\|policy"
```

**Restart Services**:
```bash
docker-compose restart grcmvc
docker-compose restart db
```

**Verify Routes**:
```bash
# Test each route
curl -I http://localhost:8888/Risk
curl -I http://localhost:8888/Control
curl -I http://localhost:8888/Admin
```

## Documentation Files

- `SYSTEM_STATUS.md` - Current system status
- `DB_CONNECTION_GUIDE.md` - Database troubleshooting
- `FAST_PATH_ACTION_PLAN.md` - This file

## Next Immediate Actions

1. **Fix DB Connection** (if still failing):
   - Run verification script
   - Check password match
   - Restart application
   - Verify health endpoint

2. **Verify Permission Policies**:
   - Test endpoints return 302/401/403 (not 500)
   - Check logs for policy resolution

3. **Seed Security Model**:
   - Verify roles exist
   - Create test user
   - Test login and permissions

4. **UI Routing**:
   - Test all menu links
   - Document "not implemented" vs "bugs"
