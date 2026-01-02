using Volo.Abp.Application.Services;

namespace Grc.Application.Contracts.Subscriptions;

public interface ISubscriptionAppService : ICrudAppService<SubscriptionDto, Guid, SubscriptionListInputDto, CreateSubscriptionDto, UpdateSubscriptionDto>
{
    Task ActivateAsync(Guid id);
    Task DeactivateAsync(Guid id);
}
