using Volo.Abp.Application.Dtos;

namespace Grc.Application.Contracts.Admin.RoleManagement;

public class RoleDto : EntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsStatic { get; set; }
    public bool IsPublic { get; set; }
    public DateTime CreationTime { get; set; }
    public int UserCount { get; set; }
    public List<string> Permissions { get; set; } = new();
}

public class CreateRoleDto
{
    public string Name { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsPublic { get; set; }
    public List<string> Permissions { get; set; } = new();
}

public class UpdateRoleDto
{
    public string Name { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsPublic { get; set; }
    public List<string> Permissions { get; set; } = new();
}

public class RoleListInputDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public bool? IsDefault { get; set; }
    public bool? IsPublic { get; set; }
    public bool? IsStatic { get; set; }
}
