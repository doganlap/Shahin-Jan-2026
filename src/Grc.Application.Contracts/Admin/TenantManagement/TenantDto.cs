using Volo.Abp.Application.Dtos;

namespace Grc.Application.Contracts.Admin.TenantManagement;

public class TenantDto : EntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? CreationTime { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public int UserCount { get; set; }
    public string? ConnectionString { get; set; }
}

public class TenantListInputDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public bool? IsActive { get; set; }
}
