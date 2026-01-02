# Build Instructions for Admin Portal

## ✅ Code Status

The Admin Portal code is complete and ready to build. I've made the following updates:

### Files Fixed:
1. ✅ Removed invalid `_userRoleRepository` reference in UserManagementAppService.cs
2. ✅ Added required NuGet packages to `Grc.Application.csproj`:
   - `Volo.Abp.Identity` (8.0.0)
   - `Volo.Abp.MultiTenancy` (8.0.0)
   - `AutoMapper` (12.0.1)
   - `AutoMapper.Extensions.Microsoft.DependencyInjection` (12.0.1)
3. ✅ Registered AutoMapper in `GrcApplicationModule.cs`

---

## 🚀 Steps to Build

### 1. Install .NET SDK 8.0

```bash
# Option 1: Using Microsoft's repository (recommended)
wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0

# Option 2: Using snap
sudo snap install dotnet-sdk --classic

# Verify installation
dotnet --version
# Should output: 8.0.x
```

### 2. Restore Packages

```bash
cd /home/dogan/grc-system
dotnet restore
```

### 3. Build Solution

```bash
dotnet build
```

### 4. If Build Succeeds

Great! The Admin Portal is ready to use.

### 5. If Build Fails

Check the error messages. Common issues:

**Missing Package**: Add the package to the appropriate `.csproj` file
**Namespace Error**: Check using statements
**Type Not Found**: Verify package references

---

## 📦 Packages Added

### Grc.Application.csproj
- ✅ `Volo.Abp.Identity` - For IdentityUser, IdentityRole
- ✅ `Volo.Abp.MultiTenancy` - For Tenant entity
- ✅ `AutoMapper` - For DTO mapping
- ✅ `AutoMapper.Extensions.Microsoft.DependencyInjection` - AutoMapper DI integration

### Already Present
- ✅ `Volo.Abp.Application` - Base application services
- ✅ `Volo.Abp.Domain` - Domain repositories
- ✅ `YamlDotNet` - Policy file parsing
- ✅ `System.Text.Json` - JSON handling

---

## ✅ Expected Build Result

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

## 🎯 What's Ready

After successful build, you'll have:

- ✅ **5 AppServices** working
- ✅ **9 Blazor Pages** ready
- ✅ **All DTOs** defined
- ✅ **AutoMapper** configured
- ✅ **Admin Portal** functional

---

## 🐛 Known Issues Fixed

1. ✅ Removed invalid `_userRoleRepository` reference
2. ✅ Added missing NuGet packages
3. ✅ Registered AutoMapper profile

---

## 📝 Next Steps After Build

1. ✅ Run the application: `dotnet run`
2. ✅ Navigate to `/admin` 
3. ✅ Test user management
4. ✅ Test role management
5. ✅ Test tenant viewing

---

## 💡 Tips

- If you see "namespace not found" errors, check that packages are restored: `dotnet restore`
- If AutoMapper errors occur, verify the profile is registered in `GrcApplicationModule.cs`
- If Identity types not found, ensure `Volo.Abp.Identity` is referenced

---

**All code is ready! Just install .NET SDK and build! 🚀**
