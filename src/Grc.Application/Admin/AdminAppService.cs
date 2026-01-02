using Volo.Abp.Application.Services;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Domain.Repositories;
using Grc.Application.Contracts.Admin;
using Grc.Permissions;
using Microsoft.AspNetCore.Authorization;

namespace Grc.Application.Admin;

[Authorize(GrcPermissions.Admin.Access)]
public class AdminAppService : ApplicationService, IAdminAppService
{
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly IRepository<IdentityRole, Guid> _roleRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly ITenantRepository _tenantRepository;

    public AdminAppService(
        IRepository<IdentityUser, Guid> userRepository,
        IRepository<IdentityRole, Guid> roleRepository,
        ICurrentTenant currentTenant,
        ITenantRepository tenantRepository)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _currentTenant = currentTenant;
        _tenantRepository = tenantRepository;
    }

    public async Task<AdminDashboardDto> GetDashboardAsync()
    {
        var tenantId = _currentTenant.Id;

        var totalUsers = (int)await _userRepository.GetCountAsync();
        var totalRoles = (int)await _roleRepository.GetCountAsync();
        
        var totalTenants = 0;
        var activeSubscriptions = 0;

        // Only count tenants if we're in host context
        if (!tenantId.HasValue)
        {
            totalTenants = (int)await _tenantRepository.GetCountAsync();
        }

        // Get recent activities (simplified - in real app, use audit log)
        var recentActivities = new List<RecentActivityDto>();

        return new AdminDashboardDto
        {
            TotalUsers = totalUsers,
            TotalRoles = totalRoles,
            TotalTenants = totalTenants,
            ActiveSubscriptions = activeSubscriptions,
            PendingUsers = 0,
            RecentActivities = recentActivities
        };
    }
}
