using Volo.Abp;

namespace Grc.Application.Policy;

public class PolicyViolationException : BusinessException
{
    public string RuleId { get; }
    public string? RemediationHint { get; }
    public List<string> Violations { get; }

    public PolicyViolationException(
        string ruleId,
        string message,
        string? remediationHint = null,
        List<string>? violations = null)
        : base("Grc:PolicyViolation", message)
    {
        RuleId = ruleId;
        RemediationHint = remediationHint;
        Violations = violations ?? new List<string>();
    }
}
