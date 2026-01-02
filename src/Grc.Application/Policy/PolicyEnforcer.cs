using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Grc.Application.Policy.PolicyModels;
using PolicyDoc = Grc.Application.Policy.PolicyModels.PolicyDocument;

namespace Grc.Application.Policy;

public class PolicyEnforcer : IPolicyEnforcer
{
    private readonly IPolicyStore _policyStore;
    private readonly IPolicyAuditLogger _auditLogger;
    private readonly ILogger<PolicyEnforcer> _logger;
    private readonly string _defaultPolicyPath;

    public PolicyEnforcer(
        IPolicyStore policyStore,
        IPolicyAuditLogger auditLogger,
        ILogger<PolicyEnforcer> logger,
        string defaultPolicyPath = "grc-baseline.yml")
    {
        _policyStore = policyStore;
        _auditLogger = auditLogger;
        _logger = logger;
        _defaultPolicyPath = defaultPolicyPath;
    }

    public async Task EnforceAsync(PolicyContext ctx, CancellationToken ct = default)
    {
        var policy = await _policyStore.LoadPolicyAsync(_defaultPolicyPath, ct);
        if (policy == null)
        {
            _logger.LogWarning("Policy not found, defaulting to allow");
            return; // Default allow if no policy
        }

        var decision = await EvaluateAsync(policy, ctx, ct);
        
        _auditLogger.LogDecision(ctx, decision, decision.MatchedRuleIds ?? new List<string>());

        if (decision.Effect == "deny")
        {
            var remediation = decision.RemediationHint ?? "Contact your administrator";
            throw new PolicyViolationException(
                decision.RuleId ?? "UNKNOWN",
                decision.Message ?? "Policy violation",
                remediation,
                decision.Violations);
        }

        // Apply mutations if any
        if (decision.Effect == "mutate" && decision.MutatedResource != null)
        {
            // Note: In a real implementation, you'd need to update the resource
            // This is a simplified version - mutations should be applied to the actual entity
            _logger.LogInformation("Mutations applied to resource");
        }
    }

    private async Task<PolicyDecisionResult> EvaluateAsync(PolicyDoc policy, PolicyContext ctx, CancellationToken ct)
    {
        var matchedRuleIds = new List<string>();
        var violations = new List<string>();
        object? currentResource = ctx.Resource;
        var decisions = new List<PolicyDecision>();

        // Check exceptions first
        var activeExceptions = policy.Spec.Exceptions
            .Where(e => IsExceptionActive(e, ctx))
            .ToList();

        // Evaluate rules in priority order (OrderBy is stable)
        var enabledRules = policy.Spec.Rules
            .Where(r => r.Enabled)
            .OrderBy(r => r.Priority)
            .ToList();

        foreach (var rule in enabledRules)
        {
            // Skip if exception applies
            if (activeExceptions.Any(e => e.RuleIds.Contains(rule.Id)))
            {
                _logger.LogDebug("Rule {RuleId} skipped due to exception", rule.Id);
                continue;
            }

            if (!MatchesRule(rule, ctx))
            {
                continue;
            }

            matchedRuleIds.Add(rule.Id);

            // Evaluate conditions
            if (!EvaluateConditions(rule.When, currentResource))
            {
                continue;
            }

            var decision = new PolicyDecision
            {
                Effect = rule.Effect,
                RuleId = rule.Id,
                Message = rule.Message
            };

            decisions.Add(decision);

            // Apply mutations
            if (rule.Effect == "mutate" && rule.Mutations.Any())
            {
                foreach (var mutation in rule.Mutations)
                {
                    try
                    {
                        currentResource = MutationApplier.ApplyMutation(currentResource, mutation);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to apply mutation {MutationOp} on {Path}", mutation.Op, mutation.Path);
                        violations.Add($"Mutation failed: {mutation.Op} on {mutation.Path}");
                    }
                }
                decision.MutatedResource = currentResource;
            }

            // Short circuit if configured
            if (policy.Spec.Execution.ShortCircuit && (rule.Effect == "deny" || rule.Effect == "allow"))
            {
                break;
            }
        }

        // Apply conflict strategy
        var finalDecision = ApplyConflictStrategy(policy.Spec.Execution.ConflictStrategy, decisions, policy.Spec.DefaultEffect);

        return new PolicyDecisionResult
        {
            Effect = finalDecision.Effect,
            RuleId = finalDecision.RuleId,
            Message = finalDecision.Message,
            RemediationHint = decisions.FirstOrDefault(d => d.Effect == "deny")?.RuleId != null
                ? enabledRules.FirstOrDefault(r => r.Id == finalDecision.RuleId)?.Remediation?.Hint
                : null,
            MutatedResource = currentResource,
            MatchedRuleIds = matchedRuleIds,
            Violations = violations
        };
    }

    private bool IsExceptionActive(PolicyException exception, PolicyContext ctx)
    {
        if (exception.ExpiresAt.HasValue && exception.ExpiresAt.Value < DateTime.UtcNow)
        {
            return false;
        }

        return MatchesRuleMatch(exception.Match, ctx);
    }

    private bool MatchesRule(PolicyRule rule, PolicyContext ctx)
    {
        return MatchesRuleMatch(rule.Match, ctx);
    }

    private bool MatchesRuleMatch(MatchConfig match, PolicyContext ctx)
    {
        // Check resource type
        if (match.Resource.Type != "*" && match.Resource.Type != ctx.ResourceType)
        {
            return false;
        }

        // Check environment
        if (match.Environment != "*" && match.Environment != ctx.Environment)
        {
            return false;
        }

        // Check principal
        if (match.Principal != null)
        {
            if (!string.IsNullOrEmpty(match.Principal.Id) && match.Principal.Id != ctx.PrincipalId)
            {
                return false;
            }

            if (match.Principal.Roles.Any() && !match.Principal.Roles.Any(r => ctx.PrincipalRoles.Contains(r)))
            {
                return false;
            }
        }

        return true;
    }

    private bool EvaluateConditions(List<Condition> conditions, object? resource)
    {
        if (conditions == null || !conditions.Any())
        {
            return true; // No conditions means match
        }

        foreach (var condition in conditions)
        {
            if (!EvaluateCondition(condition, resource))
            {
                return false;
            }
        }

        return true;
    }

    private bool EvaluateCondition(Condition condition, object? resource)
    {
        var value = DotPathResolver.Resolve(resource, condition.Path);
        var conditionValue = condition.Value;

        return condition.Op.ToLowerInvariant() switch
        {
            "exists" => value != null,
            "equals" => Equals(value, conditionValue),
            "notequals" => !Equals(value, conditionValue),
            "in" => conditionValue is IEnumerable<object> enumerable && enumerable.Contains(value),
            "notin" => conditionValue is IEnumerable<object> enumerable && !enumerable.Contains(value),
            "matches" => conditionValue is string pattern && value is string strValue && Regex.IsMatch(strValue, pattern),
            "notmatches" => conditionValue is string pattern && (value is not string strValue2 || !Regex.IsMatch(strValue2, pattern)),
            _ => false
        };
    }

    private PolicyDecision ApplyConflictStrategy(string strategy, List<PolicyDecision> decisions, string defaultEffect)
    {
        if (!decisions.Any())
        {
            return new PolicyDecision { Effect = defaultEffect };
        }

        return strategy.ToLowerInvariant() switch
        {
            "denyoverrides" => decisions.FirstOrDefault(d => d.Effect == "deny") ?? decisions.Last(),
            "allowoverrides" => decisions.FirstOrDefault(d => d.Effect == "allow") ?? decisions.Last(),
            "highestprioritywins" => decisions.First(), // Already sorted by priority
            _ => decisions.Last()
        };
    }

    private class PolicyDecisionResult : PolicyDecision
    {
        public List<string>? MatchedRuleIds { get; set; }
        public List<string>? Violations { get; set; }
        public string? RemediationHint { get; set; }
    }
}
