namespace Grc.Application.Contracts.Admin;

public class AdminDashboardDto
{
    public int TotalUsers { get; set; }
    public int TotalRoles { get; set; }
    public int TotalTenants { get; set; }
    public int ActiveSubscriptions { get; set; }
    public int PendingUsers { get; set; }
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
}

public class RecentActivityDto
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? UserName { get; set; }
}
