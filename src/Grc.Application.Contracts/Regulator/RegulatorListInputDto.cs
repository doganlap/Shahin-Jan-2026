using Volo.Abp.Application.Dtos;

namespace Grc.Application.Contracts.Regulator;

public class RegulatorListInputDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public string? Region { get; set; }
    public string? Industry { get; set; }
}
