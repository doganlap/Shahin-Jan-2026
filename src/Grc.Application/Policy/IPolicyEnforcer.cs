namespace Grc.Application.Policy;

public interface IPolicyEnforcer
{
    Task EnforceAsync(PolicyContext ctx, CancellationToken ct = default);
}
