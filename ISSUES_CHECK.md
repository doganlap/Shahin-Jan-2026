# Issues Check - What Might Be Broken

## ✅ Code Structure Looks Good

All files are in place and properly structured.

## 🔍 Potential Issues to Check

### 1. Blazor Pages - Missing Using Statements

The Blazor pages use `@inject` but might need explicit using statements. Check if these are needed:

```csharp
@using Grc.Application.Contracts.Admin
@using Grc.Application.Contracts.Admin.UserManagement
@using Grc.Application.Contracts.Admin.RoleManagement
@using Grc.Application.Contracts.Admin.TenantManagement
```

### 2. Dependency Injection

Make sure AppServices are registered. ABP Framework should auto-register them, but verify:
- All AppServices inherit from `ApplicationService` or `CrudAppService` ✅
- All interfaces are in `Application.Contracts` ✅

### 3. AutoMapper Profile

AutoMapper is registered in `GrcApplicationModule.cs` ✅

### 4. Missing Imports in Blazor

Check `_Imports.razor` file in Blazor project - might need:
```
@using Grc.Application.Contracts.Admin
@using Grc.Application.Contracts.Admin.UserManagement
@using Grc.Application.Contracts.Admin.RoleManagement
@using Grc.Application.Contracts.Admin.TenantManagement
```

### 5. NavigationManager

Blazor pages use `NavigationManager` - this should be available by default, but verify it's injected.

---

## 🔧 Quick Fixes

### If Pages Don't Load:

1. **Check _Imports.razor** - Add namespace imports
2. **Check DI Registration** - Verify services are registered
3. **Check Routes** - Verify route paths match menu

### If AppServices Fail:

1. **Check Permissions** - User needs `GrcPermissions.Admin.*` permissions
2. **Check Database** - Identity tables must exist
3. **Check DbContext** - Must include Identity entities

---

## 📋 Common Runtime Errors

### "Service not registered"
- Check if AppService is registered (should be auto-registered by ABP)
- Verify interface is in Contracts project

### "Permission denied"
- User needs proper permissions
- Check role assignments

### "Entity not found"
- Database might not be initialized
- Run migrations
- Seed data

### "AutoMapper mapping failed"
- Check AutoMapper profile is registered ✅
- Verify DTO properties match entity properties

---

## ✅ What's Already Fixed

- ✅ All AppServices properly structured
- ✅ AutoMapper registered
- ✅ NuGet packages added
- ✅ Code compilation issues fixed
- ✅ File structure correct

---

## 🎯 Next Steps to Debug

1. **Check Runtime Errors** - Look at browser console/logs
2. **Check Server Logs** - See what errors occur
3. **Verify Permissions** - User has Admin permissions
4. **Check Database** - Tables exist and seeded
5. **Test Routes** - Navigate to `/admin` directly

Let me know what specific error you're seeing and I can help fix it!
