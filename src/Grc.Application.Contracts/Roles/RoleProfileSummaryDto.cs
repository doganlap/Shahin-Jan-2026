namespace Grc.Application.Contracts.Roles;

/// <summary>
/// Summary DTO for role profile (lightweight version for lists)
/// </summary>
public class RoleProfileSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PermissionCount { get; set; }
    public int UserCount { get; set; }
    public bool IsPublic { get; set; }
    public bool IsDefault { get; set; }
}
