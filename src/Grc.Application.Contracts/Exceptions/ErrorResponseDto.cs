namespace Grc.Application.Contracts.Exceptions;

public class ErrorResponseDto
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public Dictionary<string, object>? Data { get; set; }
    public string? RemediationHint { get; set; }
    public List<string>? ValidationErrors { get; set; }
}
