namespace Grc.Application.Contracts.Vendor;

public class VendorAssessmentDto
{
    public string AssessmentType { get; set; } = string.Empty;
    public DateTime AssessmentDate { get; set; }
    public string? RiskRating { get; set; }
    public string? ComplianceStatus { get; set; }
    public string? Notes { get; set; }
    public DateTime? NextAssessmentDate { get; set; }
}
