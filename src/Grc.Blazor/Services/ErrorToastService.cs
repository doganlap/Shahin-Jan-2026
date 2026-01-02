using Microsoft.JSInterop;
using Grc.Application.Contracts.Exceptions;

namespace Grc.Blazor.Services;

public class ErrorToastService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<ErrorToastService> _logger;

    public ErrorToastService(IJSRuntime jsRuntime, ILogger<ErrorToastService> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    public async Task ShowErrorAsync(string message, string? details = null)
    {
        _logger.LogError("Error: {Message}, Details: {Details}", message, details);
        
        try
        {
            await _jsRuntime.InvokeVoidAsync("console.error", message);
            // In a real implementation, you'd call a toast notification library
            // For now, we'll use browser console
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to show error toast");
        }
    }

    public async Task ShowErrorAsync(ErrorResponseDto error)
    {
        var message = $"{error.Message}";
        if (!string.IsNullOrWhiteSpace(error.Details))
        {
            message += $"\n{error.Details}";
        }
        if (!string.IsNullOrWhiteSpace(error.RemediationHint))
        {
            message += $"\n\nالحل المقترح: {error.RemediationHint}";
        }

        await ShowErrorAsync(message, error.Details);
    }

    public async Task ShowSuccessAsync(string message)
    {
        _logger.LogInformation("Success: {Message}", message);
        
        try
        {
            await _jsRuntime.InvokeVoidAsync("console.log", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to show success toast");
        }
    }
}
