using Volo.Abp.Application.Dtos;

namespace Grc.Application.Contracts.PolicyDocument;

public class PolicyDocumentListInputDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public string? Status { get; set; }
    public string? Owner { get; set; }
    public string? Category { get; set; }
    public DateTime? EffectiveDateFrom { get; set; }
    public DateTime? EffectiveDateTo { get; set; }
}
