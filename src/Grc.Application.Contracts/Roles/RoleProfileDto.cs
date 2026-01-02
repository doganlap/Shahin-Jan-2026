using Volo.Abp.Application.Dtos;

namespace Grc.Application.Contracts.Roles;

public class RoleProfileDto : EntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SLA { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
    public bool IsPublic { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public int UserCount { get; set; }
}
