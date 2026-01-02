using Volo.Abp.Application.Dtos;

namespace Grc.Application.Contracts.Evidence;

public class EvidenceDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Owner { get; set; }
    public string? DataClassification { get; set; }
    public Dictionary<string, string> Labels { get; set; } = new();
    public bool ApprovedForProd { get; set; }
    public string Status { get; set; } = "Draft";
}

public class CreateEvidenceDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DataClassification { get; set; }
    public string? Owner { get; set; }
    public bool ApprovedForProd { get; set; }
}

public class UpdateEvidenceDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DataClassification { get; set; }
    public string? Owner { get; set; }
    public bool ApprovedForProd { get; set; }
    public string Status { get; set; } = "Draft";
}

public class EvidenceListInputDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public string? Status { get; set; }
    public string? Owner { get; set; }
    public string? DataClassification { get; set; }
}
