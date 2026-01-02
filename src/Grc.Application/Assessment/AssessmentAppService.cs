using AssessmentEntity = Grc.Domain.Assessment.Assessment;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Grc.Application.Policy;
using Grc.Application.Contracts.Assessment;
using Grc.Domain.Assessment;
using Grc.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Grc.Application.Assessment;

[Authorize]
public class AssessmentAppService : BasePolicyAppService, IAssessmentAppService
{
    private readonly IAssessmentRepository _repository;

    public AssessmentAppService(
        IPolicyEnforcer policyEnforcer,
        Volo.Abp.Users.ICurrentUser currentUser,
        Volo.Abp.MultiTenancy.ICurrentTenant currentTenant,
        IAssessmentRepository repository,
        IEnvironmentProvider? environmentProvider = null,
        IRoleResolver? roleResolver = null)
        : base(policyEnforcer, currentUser, currentTenant, environmentProvider, roleResolver)
    {
        _repository = repository;
    }

    [Authorize(GrcPermissions.Assessments.View)]
    public async Task<PagedResultDto<AssessmentDto>> GetListAsync(AssessmentListInputDto input)
    {
        var query = await CreateFilteredQueryAsync(input);
        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(e => e.CreationTime)
                 .PageBy(input.SkipCount, input.MaxResultCount));

        return new PagedResultDto<AssessmentDto>(
            totalCount,
            ObjectMapper.Map<List<AssessmentEntity>, List<AssessmentDto>>(items)
        );
    }

    protected virtual async Task<IQueryable<AssessmentEntity>> CreateFilteredQueryAsync(AssessmentListInputDto input)
    {
        var query = await _repository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(a => a.Name.Contains(input.Filter) || a.Description.Contains(input.Filter));
        }

        if (!string.IsNullOrWhiteSpace(input.Status))
        {
            query = query.Where(a => a.Status == input.Status);
        }

        if (!string.IsNullOrWhiteSpace(input.Owner))
        {
            query = query.Where(a => a.Owner == input.Owner);
        }

        if (input.FrameworkId.HasValue)
        {
            query = query.Where(a => a.FrameworkId == input.FrameworkId.Value);
        }

        return query;
    }

    [Authorize(GrcPermissions.Assessments.Create)]
    public async Task<AssessmentDto> CreateAsync(CreateAssessmentDto input)
    {
        var entity = new AssessmentEntity(GuidGenerator.Create(), input.Name)
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

        await EnforceAsync("create", "Assessment", entity);
        await _repository.InsertAsync(entity, autoSave: true);

        return ObjectMapper.Map<AssessmentEntity, AssessmentDto>(entity);
    }

    [Authorize(GrcPermissions.Assessments.Update)]
    public async Task<AssessmentDto> UpdateAsync(Guid id, UpdateAssessmentDto input)
    {
        var entity = await _repository.GetAsync(id);
        entity.Name = input.Name;
        entity.Description = input.Description;
        entity.Owner = input.Owner ?? entity.Owner;
        entity.DataClassification = input.DataClassification ?? entity.DataClassification;

        if (entity.Labels == null)
            entity.Labels = new Dictionary<string, string>();

        entity.Labels["dataClassification"] = entity.DataClassification ?? "internal";
        entity.Labels["owner"] = entity.Owner ?? "unknown";

        await EnforceAsync("update", "Assessment", entity);
        await _repository.UpdateAsync(entity, autoSave: true);

        return ObjectMapper.Map<AssessmentEntity, AssessmentDto>(entity);
    }

    [Authorize(GrcPermissions.Assessments.Submit)]
    public async Task<AssessmentDto> SubmitAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        entity.Status = "Submitted";

        await EnforceAsync("submit", "Assessment", entity);
        await _repository.UpdateAsync(entity, autoSave: true);

        return ObjectMapper.Map<AssessmentEntity, AssessmentDto>(entity);
    }

    [Authorize(GrcPermissions.Assessments.Approve)]
    public async Task<AssessmentDto> ApproveAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        entity.Status = "Approved";

        await EnforceAsync("approve", "Assessment", entity);
        await _repository.UpdateAsync(entity, autoSave: true);

        return ObjectMapper.Map<AssessmentEntity, AssessmentDto>(entity);
    }

    public async Task<AssessmentDto> GetAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        return ObjectMapper.Map<AssessmentEntity, AssessmentDto>(entity);
    }

    [Authorize(GrcPermissions.Assessments.Update)]
    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }
}
