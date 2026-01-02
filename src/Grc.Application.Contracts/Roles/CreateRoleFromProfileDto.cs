namespace Grc.Application.Contracts.Roles;

public class CreateRoleFromProfileDto
{
    public string ProfileName { get; set; } = string.Empty;
    public string? CustomName { get; set; }
    public bool IsPublic { get; set; } = true;
    public bool IsDefault { get; set; } = false;
}
