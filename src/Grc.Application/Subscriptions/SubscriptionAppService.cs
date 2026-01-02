using Volo.Abp.Application.Services;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Grc.Application.Contracts.Subscriptions;
using Grc.Domain.Subscription;
using Grc.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.MultiTenancy;

namespace Grc.Application.Subscriptions;

[Authorize(GrcPermissions.Subscriptions.View)]
public class SubscriptionAppService : CrudAppService<
    Subscription,
    SubscriptionDto,
    Guid,
    SubscriptionListInputDto,
    CreateSubscriptionDto,
    UpdateSubscriptionDto>, ISubscriptionAppService
{
    private readonly IRepository<Subscription, Guid> _subscriptionRepository;
    private readonly ICurrentTenant _currentTenant;

    public SubscriptionAppService(
        IRepository<Subscription, Guid> subscriptionRepository,
        ICurrentTenant currentTenant)
        : base(subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
        _currentTenant = currentTenant;
    }

    protected override async Task<IQueryable<Subscription>> CreateFilteredQueryAsync(SubscriptionListInputDto input)
    {
        var query = await base.CreateFilteredQueryAsync(input);

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(s => s.Name.Contains(input.Filter) || s.PlanType.Contains(input.Filter));
        }

        if (input.IsActive.HasValue)
        {
            query = query.Where(s => s.IsActive == input.IsActive.Value);
        }

        if (input.TenantId.HasValue)
        {
            query = query.Where(s => s.TenantId == input.TenantId.Value);
        }

        return query;
    }

    [Authorize(GrcPermissions.Subscriptions.Manage)]
    public override async Task<SubscriptionDto> CreateAsync(CreateSubscriptionDto input)
    {
        var entity = new Subscription(GuidGenerator.Create(), input.Name, input.PlanType)
        {
            StartDate = input.StartDate,
            EndDate = input.EndDate,
            Price = input.Price,
            Currency = input.Currency ?? "USD",
            TenantId = input.TenantId ?? _currentTenant.Id
        };

        await _subscriptionRepository.InsertAsync(entity, autoSave: true);

        return ObjectMapper.Map<Subscription, SubscriptionDto>(entity);
    }

    [Authorize(GrcPermissions.Subscriptions.Manage)]
    public override async Task<SubscriptionDto> UpdateAsync(Guid id, UpdateSubscriptionDto input)
    {
        var entity = await _subscriptionRepository.GetAsync(id);

        if (!string.IsNullOrWhiteSpace(input.Name))
            entity.Name = input.Name;

        if (!string.IsNullOrWhiteSpace(input.PlanType))
            entity.PlanType = input.PlanType;

        if (input.IsActive.HasValue)
            entity.IsActive = input.IsActive.Value;

        if (input.StartDate.HasValue)
            entity.StartDate = input.StartDate.Value;

        if (input.EndDate.HasValue)
            entity.EndDate = input.EndDate;

        if (input.Price.HasValue)
            entity.Price = input.Price;

        if (!string.IsNullOrWhiteSpace(input.Currency))
            entity.Currency = input.Currency;

        await _subscriptionRepository.UpdateAsync(entity, autoSave: true);

        return ObjectMapper.Map<Subscription, SubscriptionDto>(entity);
    }

    [Authorize(GrcPermissions.Subscriptions.Manage)]
    public async Task ActivateAsync(Guid id)
    {
        var entity = await _subscriptionRepository.GetAsync(id);
        entity.IsActive = true;
        await _subscriptionRepository.UpdateAsync(entity, autoSave: true);
    }

    [Authorize(GrcPermissions.Subscriptions.Manage)]
    public async Task DeactivateAsync(Guid id)
    {
        var entity = await _subscriptionRepository.GetAsync(id);
        entity.IsActive = false;
        await _subscriptionRepository.UpdateAsync(entity, autoSave: true);
    }
}
