namespace Grc.Application.Contracts.Admin;

public interface IAdminAppService
{
    Task<AdminDashboardDto> GetDashboardAsync();
}
