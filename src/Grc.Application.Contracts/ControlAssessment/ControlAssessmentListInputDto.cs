using Volo.Abp.Application.Dtos;

namespace Grc.Application.Contracts.ControlAssessment;

public class ControlAssessmentListInputDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public string? Status { get; set; }
    public string? Owner { get; set; }
    public Guid? AssessmentId { get; set; }
    public string? Effectiveness { get; set; }
}
