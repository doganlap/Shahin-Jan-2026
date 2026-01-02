using AutoMapper;
using Volo.Abp.Identity;
using Volo.Abp.TenantManagement;
using Grc.Application.Contracts.Admin;
using Grc.Application.Contracts.Admin.UserManagement;
using Grc.Application.Contracts.Admin.RoleManagement;
using TenantDtoContract = Grc.Application.Contracts.Admin.TenantManagement.TenantDto;

namespace Grc.Application.Admin;

public class AdminApplicationAutoMapperProfile : Profile
{
    public AdminApplicationAutoMapperProfile()
    {
        // User mappings
        CreateMap<IdentityUser, UserDto>()
            .ForMember(dest => dest.LastLoginTime, opt => opt.MapFrom(src => 
                src.ExtraProperties.ContainsKey("LastLoginTime") 
                    ? (DateTime?)src.ExtraProperties["LastLoginTime"] 
                    : null));

        // Role mappings
        CreateMap<IdentityRole, RoleDto>();

        // Tenant mappings
        CreateMap<Tenant, TenantDtoContract>();
    }
}
