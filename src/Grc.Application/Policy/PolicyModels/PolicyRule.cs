namespace Grc.Application.Policy.PolicyModels;

public class PolicyRule
{
    public required string Id { get; init; }
    public required int Priority { get; init; }
    public required string Description { get; init; }
    public bool Enabled { get; init; } = true;
    public required MatchConfig Match { get; init; }
    public List<Condition> When { get; init; } = new();
    public required string Effect { get; init; } // allow/deny/audit/mutate
    public string Message { get; init; } = string.Empty;
    public string Severity { get; init; } = "medium";
    public List<Mutation> Mutations { get; init; } = new();
    public Remediation? Remediation { get; init; }
}

public class MatchConfig
{
    public required ResourceMatch Resource { get; init; }
    public PrincipalMatch? Principal { get; init; }
    public string Environment { get; init; } = "*";
}

public class ResourceMatch
{
    public required string Type { get; init; }
    public string Name { get; init; } = "*";
    public Dictionary<string, string> Labels { get; init; } = new();
}

public class PrincipalMatch
{
    public string? Id { get; init; }
    public List<string> Roles { get; init; } = new();
}

public class Condition
{
    public required string Op { get; init; } // exists/equals/notEquals/in/notIn/matches/notMatches
    public required string Path { get; init; }
    public object? Value { get; init; }
}

public class Mutation
{
    public required string Op { get; init; } // set/remove/add
    public required string Path { get; init; }
    public object? Value { get; init; }
}

public class Remediation
{
    public string? Url { get; init; }
    public string? Hint { get; init; }
}

public class PolicyException
{
    public required string Id { get; init; }
    public required List<string> RuleIds { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTime? ExpiresAt { get; init; }
    public required MatchConfig Match { get; init; }
}
