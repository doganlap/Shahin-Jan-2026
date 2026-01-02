namespace Grc.Application.Policy;

public interface IRoleResolver
{
    Task<IReadOnlyList<string>> GetCurrentRolesAsync();
}
