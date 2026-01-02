namespace Grc.Application.Policy.PolicyModels;

public class PolicyDocument
{
    public required string ApiVersion { get; init; }
    public required string Kind { get; init; }
    public required PolicyMetadata Metadata { get; init; }
    public required PolicySpec Spec { get; init; }
}

public class PolicyMetadata
{
    public required string Name { get; init; }
    public string Namespace { get; init; } = "default";
    public required string Version { get; init; }
    public required DateTime CreatedAt { get; init; }
    public Dictionary<string, string> Labels { get; init; } = new();
    public Dictionary<string, string> Annotations { get; init; } = new();
}

public class PolicySpec
{
    public required string Mode { get; init; } // enforce/audit
    public string DefaultEffect { get; init; } = "allow";
    public required ExecutionConfig Execution { get; init; }
    public required TargetConfig Target { get; init; }
    public required List<PolicyRule> Rules { get; init; }
    public List<PolicyException> Exceptions { get; init; } = new();
    public AuditConfig? Audit { get; init; }
}

public class ExecutionConfig
{
    public required string Order { get; init; } // sequential
    public required bool ShortCircuit { get; init; }
    public required string ConflictStrategy { get; init; } // denyOverrides/allowOverrides/highestPriorityWins
}

public class TargetConfig
{
    public required List<string> ResourceTypes { get; init; }
    public List<string> Environments { get; init; } = new();
}

public class AuditConfig
{
    public bool LogDecisions { get; init; } = true;
    public int RetentionDays { get; init; } = 365;
    public List<AuditSink> Sinks { get; init; } = new();
}

public class AuditSink
{
    public required string Type { get; init; } // stdout/file/http
    public string? Path { get; init; }
    public string? Url { get; init; }
    public Dictionary<string, string> Headers { get; init; } = new();
}
