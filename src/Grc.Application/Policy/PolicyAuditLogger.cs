using Microsoft.Extensions.Logging;

namespace Grc.Application.Policy;

public interface IPolicyAuditLogger
{
    void LogDecision(PolicyContext context, PolicyDecision decision, List<string> matchedRuleIds);
}

public class PolicyAuditLogger : IPolicyAuditLogger
{
    private readonly ILogger<PolicyAuditLogger> _logger;

    public PolicyAuditLogger(ILogger<PolicyAuditLogger> logger)
    {
        _logger = logger;
    }

    public void LogDecision(PolicyContext context, PolicyDecision decision, List<string> matchedRuleIds)
    {
        _logger.LogInformation(
            "Policy Decision: Action={Action}, ResourceType={ResourceType}, Environment={Environment}, " +
            "TenantId={TenantId}, PrincipalId={PrincipalId}, Decision={Decision}, MatchedRules=[{MatchedRules}]",
            context.Action,
            context.ResourceType,
            context.Environment,
            context.TenantId,
            context.PrincipalId,
            decision.Effect,
            string.Join(", ", matchedRuleIds));
    }
}

public class PolicyDecision
{
    public required string Effect { get; init; } // allow/deny/audit/mutate
    public string? RuleId { get; init; }
    public string? Message { get; init; }
    public object? MutatedResource { get; set; }
}
