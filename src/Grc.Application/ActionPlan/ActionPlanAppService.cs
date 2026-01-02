using ActionPlanEntity = Grc.Domain.ActionPlan.ActionPlan;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Grc.Application.Policy;
using Grc.Application.Contracts.ActionPlan;
using Grc.Domain.ActionPlan;
using Grc.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Grc.Application.ActionPlan;

[Authorize]
public class ActionPlanAppService : BasePolicyAppService, IActionPlanAppService
{
    private readonly IActionPlanRepository _repository;

    public ActionPlanAppService(
        IPolicyEnforcer policyEnforcer,
        Volo.Abp.Users.ICurrentUser currentUser,
        Volo.Abp.MultiTenancy.ICurrentTenant currentTenant,
        IActionPlanRepository repository,
        IEnvironmentProvider? environmentProvider = null,
        IRoleResolver? roleResolver = null)
        : base(policyEnforcer, currentUser, currentTenant, environmentProvider, roleResolver)
    {
        _repository = repository;
    }

    [Authorize(GrcPermissions.ActionPlans.View)]
    public async Task<PagedResultDto<ActionPlanDto>> GetListAsync(ActionPlanListInputDto input)
    {
        var query = await CreateFilteredQueryAsync(input);
        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(e => e.CreationTime)
                 .PageBy(input.SkipCount, input.MaxResultCount));

        return new PagedResultDto<ActionPlanDto>(
            totalCount,
            ObjectMapper.Map<List<ActionPlanEntity>, List<ActionPlanDto>>(items)
        );
    }

    protected virtual async Task<IQueryable<ActionPlanEntity>> CreateFilteredQueryAsync(ActionPlanListInputDto input)
    {
        var query = await _repository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(ap => ap.Name.Contains(input.Filter) || ap.Description.Contains(input.Filter));
        }

        if (!string.IsNullOrWhiteSpace(input.Status))
        {
            query = query.Where(ap => ap.Status == input.Status);
        }

        if (!string.IsNullOrWhiteSpace(input.Owner))
        {
            query = query.Where(ap => ap.Owner == input.Owner);
        }

        if (input.RiskId.HasValue)
        {
            query = query.Where(ap => ap.RiskId == input.RiskId.Value);
        }

        if (input.AuditId.HasValue)
        {
            query = query.Where(ap => ap.AuditId == input.AuditId.Value);
        }

        if (input.DueDateFrom.HasValue)
        {
            query = query.Where(ap => ap.DueDate >= input.DueDateFrom.Value);
        }

        if (input.DueDateTo.HasValue)
        {
            query = query.Where(ap => ap.DueDate <= input.DueDateTo.Value);
        }

        return query;
    }

    [Authorize(GrcPermissions.ActionPlans.Manage)]
    public async Task<ActionPlanDto> UpdateAsync(Guid id, UpdateActionPlanDto input)
    {
        var entity = await _repository.GetAsync(id);
        entity.Name = input.Name;
        entity.Description = input.Description;
        entity.Owner = input.Owner ?? entity.Owner;
        entity.DataClassification = input.DataClassification ?? entity.DataClassification;
        entity.DueDate = input.DueDate ?? entity.DueDate;

        if (entity.Labels == null)
            entity.Labels = new Dictionary<string, string>();

        entity.Labels["dataClassification"] = entity.DataClassification ?? "internal";
        entity.Labels["owner"] = entity.Owner ?? "unknown";

        await EnforceAsync("update", "ActionPlan", entity);
        await _repository.UpdateAsync(entity, autoSave: true);

        return ObjectMapper.Map<ActionPlanEntity, ActionPlanDto>(entity);
    }

    [Authorize(GrcPermissions.ActionPlans.Manage)]
    public async Task<ActionPlanDto> CreateAsync(CreateActionPlanDto input)
    {
        var entity = new ActionPlanEntity(GuidGenerator.Create(), input.Name)
        {
            Description = input.Description,
            Owner = input.Owner ?? CurrentUser.UserName,
            DataClassification = input.DataClassification ?? "internal",
            Labels = new Dictionary<string, string>
            {
                ["dataClassification"] = input.DataClassification ?? "internal",
                ["owner"] = input.Owner ?? CurrentUser.UserName ?? "unknown"
            }
        };

        await EnforceAsync("create", "ActionPlan", entity);
        await _repository.InsertAsync(entity, autoSave: true);

        return ObjectMapper.Map<ActionPlanEntity, ActionPlanDto>(entity);
    }

    [Authorize(GrcPermissions.ActionPlans.Assign)]
    public async Task<ActionPlanDto> AssignAsync(Guid id, string assignee)
    {
        var entity = await _repository.GetAsync(id);
        entity.Owner = assignee;

        await EnforceAsync("assign", "ActionPlan", entity);
        await _repository.UpdateAsync(entity, autoSave: true);

        return ObjectMapper.Map<ActionPlanEntity, ActionPlanDto>(entity);
    }

    [Authorize(GrcPermissions.ActionPlans.Close)]
    public async Task<ActionPlanDto> CloseAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        entity.Status = "Closed";

        await EnforceAsync("close", "ActionPlan", entity);
        await _repository.UpdateAsync(entity, autoSave: true);

        return ObjectMapper.Map<ActionPlanEntity, ActionPlanDto>(entity);
    }

    public async Task<ActionPlanDto> GetAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        return ObjectMapper.Map<ActionPlanEntity, ActionPlanDto>(entity);
    }

    [Authorize(GrcPermissions.ActionPlans.Manage)]
    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }
}
