using WorkflowEntity = Grc.Domain.Workflow.Workflow;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Grc.Application.Policy;
using Grc.Application.Contracts.Workflow;
using Grc.Domain.Workflow;
using Grc.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Grc.Application.Workflow;

[Authorize]
public class WorkflowAppService : BasePolicyAppService, IWorkflowAppService
{
    private readonly IWorkflowRepository _repository;

    public WorkflowAppService(
        IPolicyEnforcer policyEnforcer,
        Volo.Abp.Users.ICurrentUser currentUser,
        Volo.Abp.MultiTenancy.ICurrentTenant currentTenant,
        IWorkflowRepository repository,
        IEnvironmentProvider? environmentProvider = null,
        IRoleResolver? roleResolver = null)
        : base(policyEnforcer, currentUser, currentTenant, environmentProvider, roleResolver)
    {
        _repository = repository;
    }

    [Authorize(GrcPermissions.Workflow.View)]
    public async Task<PagedResultDto<WorkflowDto>> GetListAsync(WorkflowListInputDto input)
    {
        var query = await CreateFilteredQueryAsync(input);
        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(e => e.CreationTime)
                 .PageBy(input.SkipCount, input.MaxResultCount));

        return new PagedResultDto<WorkflowDto>(
            totalCount,
            ObjectMapper.Map<List<WorkflowEntity>, List<WorkflowDto>>(items)
        );
    }

    protected virtual async Task<IQueryable<WorkflowEntity>> CreateFilteredQueryAsync(WorkflowListInputDto input)
    {
        var query = await _repository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(w => w.Name.Contains(input.Filter) || w.Description.Contains(input.Filter));
        }

        if (!string.IsNullOrWhiteSpace(input.Status))
        {
            query = query.Where(w => w.Status == input.Status);
        }

        if (!string.IsNullOrWhiteSpace(input.Type))
        {
            query = query.Where(w => w.WorkflowType == input.Type);
        }

        if (!string.IsNullOrWhiteSpace(input.ResourceType))
        {
            query = query.Where(w => w.ResourceType == input.ResourceType);
        }

        return query;
    }

    public async Task<WorkflowExecutionResultDto> ExecuteAsync(Guid id, WorkflowExecutionDto input)
    {
        var entity = await _repository.GetAsync(id);
        entity.Status = "Running";
        entity.LastExecutedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(entity, autoSave: true);

        return new WorkflowExecutionResultDto
        {
            WorkflowId = id,
            Status = "Completed",
            ExecutedAt = DateTime.UtcNow,
            Result = "Workflow executed successfully"
        };
    }

    public async Task<WorkflowStatusDto> GetStatusAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        return new WorkflowStatusDto
        {
            WorkflowId = id,
            Status = entity.Status,
            LastExecutedAt = entity.LastExecutedAt,
            IsActive = entity.IsActive
        };
    }

    [Authorize(GrcPermissions.Workflow.Manage)]
    public async Task<WorkflowDto> CreateAsync(CreateWorkflowDto input)
    {
        var entity = new WorkflowEntity(GuidGenerator.Create(), input.Name)
        {
            Description = input.Description,
            Owner = input.Owner ?? CurrentUser.UserName,
            DataClassification = input.DataClassification ?? "internal",
            WorkflowType = input.WorkflowType,
            Definition = input.Definition,
            TriggerEvent = input.TriggerEvent,
            Conditions = input.Conditions,
            Steps = input.Steps,
            Labels = new Dictionary<string, string>
            {
                ["dataClassification"] = input.DataClassification ?? "internal",
                ["owner"] = input.Owner ?? CurrentUser.UserName ?? "unknown"
            }
        };

        await EnforceAsync("create", "Workflow", entity);
        await _repository.InsertAsync(entity, autoSave: true);

        return ObjectMapper.Map<WorkflowEntity, WorkflowDto>(entity);
    }

    [Authorize(GrcPermissions.Workflow.Manage)]
    public async Task<WorkflowDto> UpdateAsync(Guid id, UpdateWorkflowDto input)
    {
        var entity = await _repository.GetAsync(id);
        entity.Name = input.Name;
        entity.Description = input.Description;
        entity.Owner = input.Owner ?? entity.Owner;
        entity.DataClassification = input.DataClassification ?? entity.DataClassification;
        entity.WorkflowType = input.WorkflowType;
        entity.Definition = input.Definition;
        entity.TriggerEvent = input.TriggerEvent;
        entity.Conditions = input.Conditions;
        entity.Steps = input.Steps;
        entity.IsActive = input.IsActive;

        if (entity.Labels == null)
            entity.Labels = new Dictionary<string, string>();

        entity.Labels["dataClassification"] = entity.DataClassification ?? "internal";
        entity.Labels["owner"] = entity.Owner ?? "unknown";

        await EnforceAsync("update", "Workflow", entity);
        await _repository.UpdateAsync(entity, autoSave: true);

        return ObjectMapper.Map<WorkflowEntity, WorkflowDto>(entity);
    }

    [Authorize(GrcPermissions.Workflow.View)]
    public async Task<WorkflowDto> GetAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        return ObjectMapper.Map<WorkflowEntity, WorkflowDto>(entity);
    }

    [Authorize(GrcPermissions.Workflow.Manage)]
    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }
}
