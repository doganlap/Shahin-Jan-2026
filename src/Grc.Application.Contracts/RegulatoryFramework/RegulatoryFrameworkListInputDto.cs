using Volo.Abp.Application.Dtos;

namespace Grc.Application.Contracts.RegulatoryFramework;

public class RegulatoryFrameworkListInputDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public string? Region { get; set; }
    public string? Industry { get; set; }
    public Guid? RegulatorId { get; set; }
}
