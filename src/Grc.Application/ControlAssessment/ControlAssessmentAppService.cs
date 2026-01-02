using ControlAssessmentEntity = Grc.Domain.ControlAssessment.ControlAssessment;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Grc.Application.Policy;
using Grc.Application.Contracts.ControlAssessment;
using Grc.Domain.ControlAssessment;
using Grc.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Grc.Application.ControlAssessment;

[Authorize]
public class ControlAssessmentAppService : BasePolicyAppService, IControlAssessmentAppService
{
    private readonly IControlAssessmentRepository _repository;

    public ControlAssessmentAppService(
        IPolicyEnforcer policyEnforcer,
        Volo.Abp.Users.ICurrentUser currentUser,
        Volo.Abp.MultiTenancy.ICurrentTenant currentTenant,
        IControlAssessmentRepository repository,
        IEnvironmentProvider? environmentProvider = null,
        IRoleResolver? roleResolver = null)
        : base(policyEnforcer, currentUser, currentTenant, environmentProvider, roleResolver)
    {
        _repository = repository;
    }

    [Authorize(GrcPermissions.ControlAssessments.View)]
    public async Task<PagedResultDto<ControlAssessmentDto>> GetListAsync(ControlAssessmentListInputDto input)
    {
        var query = await CreateFilteredQueryAsync(input);
        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(e => e.CreationTime)
                 .PageBy(input.SkipCount, input.MaxResultCount));

        return new PagedResultDto<ControlAssessmentDto>(
            totalCount,
            ObjectMapper.Map<List<ControlAssessmentEntity>, List<ControlAssessmentDto>>(items)
        );
    }

    protected virtual async Task<IQueryable<ControlAssessmentEntity>> CreateFilteredQueryAsync(ControlAssessmentListInputDto input)
    {
        var query = await _repository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(ca => ca.Name.Contains(input.Filter) || ca.Description.Contains(input.Filter));
        }

        if (!string.IsNullOrWhiteSpace(input.Status))
        {
            query = query.Where(ca => ca.Status == input.Status);
        }

        if (!string.IsNullOrWhiteSpace(input.Owner))
        {
            query = query.Where(ca => ca.Owner == input.Owner);
        }

        if (input.AssessmentId.HasValue)
        {
            query = query.Where(ca => ca.AssessmentId == input.AssessmentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(input.Effectiveness))
        {
            query = query.Where(ca => ca.Effectiveness == input.Effectiveness);
        }

        return query;
    }

    [Authorize(GrcPermissions.ControlAssessments.Manage)]
    public async Task<ControlAssessmentDto> CreateAsync(CreateControlAssessmentDto input)
    {
        var entity = new ControlAssessmentEntity(GuidGenerator.Create(), input.Name)
        {
            Description = input.Description,
            Owner = input.Owner ?? CurrentUser.UserName,
            DataClassification = input.DataClassification ?? "internal",
            AssessmentId = input.AssessmentId,
            ControlId = input.ControlId,
            ControlName = input.ControlName,
            Effectiveness = input.Effectiveness,
            Notes = input.Notes,
            Labels = new Dictionary<string, string>
            {
                ["dataClassification"] = input.DataClassification ?? "internal",
                ["owner"] = input.Owner ?? CurrentUser.UserName ?? "unknown"
            }
        };

        await EnforceAsync("create", "ControlAssessment", entity);
        await _repository.InsertAsync(entity, autoSave: true);

        return ObjectMapper.Map<ControlAssessmentEntity, ControlAssessmentDto>(entity);
    }

    [Authorize(GrcPermissions.ControlAssessments.Manage)]
    public async Task<ControlAssessmentDto> UpdateAsync(Guid id, UpdateControlAssessmentDto input)
    {
        var entity = await _repository.GetAsync(id);
        entity.Name = input.Name;
        entity.Description = input.Description;
        entity.Owner = input.Owner ?? entity.Owner;
        entity.DataClassification = input.DataClassification ?? entity.DataClassification;
        entity.Effectiveness = input.Effectiveness ?? entity.Effectiveness;
        entity.Notes = input.Notes ?? entity.Notes;

        if (entity.Labels == null)
            entity.Labels = new Dictionary<string, string>();

        entity.Labels["dataClassification"] = entity.DataClassification ?? "internal";
        entity.Labels["owner"] = entity.Owner ?? "unknown";

        await EnforceAsync("update", "ControlAssessment", entity);
        await _repository.UpdateAsync(entity, autoSave: true);

        return ObjectMapper.Map<ControlAssessmentEntity, ControlAssessmentDto>(entity);
    }

    [Authorize(GrcPermissions.ControlAssessments.View)]
    public async Task<ControlAssessmentDto> GetAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        return ObjectMapper.Map<ControlAssessmentEntity, ControlAssessmentDto>(entity);
    }

    [Authorize(GrcPermissions.ControlAssessments.Manage)]
    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }
}
