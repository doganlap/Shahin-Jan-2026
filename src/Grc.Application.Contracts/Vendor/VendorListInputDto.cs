using Volo.Abp.Application.Dtos;

namespace Grc.Application.Contracts.Vendor;

public class VendorListInputDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public string? Status { get; set; }
    public string? Category { get; set; }
    public string? RiskLevel { get; set; }
}
