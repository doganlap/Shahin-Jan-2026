using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace Grc.Application.Policy;

public class RoleResolver : IRoleResolver
{
    private readonly IdentityUserManager _userManager;
    private readonly ICurrentUser _currentUser;

    public RoleResolver(
        IdentityUserManager userManager,
        ICurrentUser currentUser)
    {
        _userManager = userManager;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<string>> GetCurrentRolesAsync()
    {
        if (_currentUser.Id == null)
        {
            return Array.Empty<string>();
        }

        try
        {
            var user = await _userManager.GetByIdAsync(_currentUser.Id.Value);
            if (user == null)
            {
                return Array.Empty<string>();
            }

            // Load roles from user using IdentityUserManager
            var roles = await _userManager.GetRolesAsync(user);
            return roles.ToList();
        }
        catch
        {
            // If user not found or error, return empty list
            return Array.Empty<string>();
        }
    }
}
