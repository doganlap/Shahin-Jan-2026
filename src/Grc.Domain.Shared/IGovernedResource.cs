namespace Grc.Domain.Shared;

public interface IGovernedResource
{
    string ResourceType { get; }
    string? Owner { get; set; }
    string? DataClassification { get; set; } // public|internal|confidential|restricted
    Dictionary<string, string> Labels { get; }
}
