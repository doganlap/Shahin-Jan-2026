using PolicyDocEntity = Grc.Domain.PolicyDocument.PolicyDocument;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Grc.Application.Policy;
using Grc.Application.Contracts.PolicyDocument;
using Grc.Domain.PolicyDocument;
using Grc.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Grc.Application.PolicyDocument;

[Authorize]
public class PolicyDocumentAppService : BasePolicyAppService, IPolicyDocumentAppService
{
    private readonly IPolicyDocumentRepository _repository;

    public PolicyDocumentAppService(
        IPolicyEnforcer policyEnforcer,
        Volo.Abp.Users.ICurrentUser currentUser,
        Volo.Abp.MultiTenancy.ICurrentTenant currentTenant,
        IPolicyDocumentRepository repository,
        IEnvironmentProvider? environmentProvider = null,
        IRoleResolver? roleResolver = null)
        : base(policyEnforcer, currentUser, currentTenant, environmentProvider, roleResolver)
    {
        _repository = repository;
    }

    [Authorize(GrcPermissions.Policies.View)]
    public async Task<PagedResultDto<PolicyDocumentDto>> GetListAsync(PolicyDocumentListInputDto input)
    {
        var query = await CreateFilteredQueryAsync(input);
        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(e => e.CreationTime)
                 .PageBy(input.SkipCount, input.MaxResultCount));

        return new PagedResultDto<PolicyDocumentDto>(
            totalCount,
            ObjectMapper.Map<List<PolicyDocEntity>, List<PolicyDocumentDto>>(items)
        );
    }

    protected virtual async Task<IQueryable<PolicyDocEntity>> CreateFilteredQueryAsync(PolicyDocumentListInputDto input)
    {
        var query = await _repository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(pd => pd.Name.Contains(input.Filter) || pd.Description.Contains(input.Filter));
        }

        if (!string.IsNullOrWhiteSpace(input.Status))
        {
            query = query.Where(pd => pd.Status == input.Status);
        }

        if (!string.IsNullOrWhiteSpace(input.Owner))
        {
            query = query.Where(pd => pd.Owner == input.Owner);
        }

        if (!string.IsNullOrWhiteSpace(input.Category))
        {
            query = query.Where(pd => pd.Category == input.Category);
        }

        if (input.EffectiveDateFrom.HasValue)
        {
            query = query.Where(pd => pd.EffectiveDate >= input.EffectiveDateFrom.Value);
        }

        if (input.EffectiveDateTo.HasValue)
        {
            query = query.Where(pd => pd.EffectiveDate <= input.EffectiveDateTo.Value);
        }

        return query;
    }

    [Authorize(GrcPermissions.Policies.Manage)]
    public async Task<PolicyDocumentDto> CreateAsync(CreatePolicyDocumentDto input)
    {
        var entity = new PolicyDocEntity(GuidGenerator.Create(), input.Name)
        {
            Description = input.Description,
            Owner = input.Owner ?? CurrentUser.UserName,
            DataClassification = input.DataClassification ?? "internal",
            Labels = new Dictionary<string, string>
            {
                ["dataClassification"] = input.DataClassification ?? "internal",
                ["owner"] = input.Owner ?? CurrentUser.UserName ?? "unknown",
                ["approvedForProd"] = input.ApprovedForProd.ToString().ToLowerInvariant()
            },
            ApprovedForProd = input.ApprovedForProd
        };

        await EnforceAsync("create", "PolicyDocument", entity);
        await _repository.InsertAsync(entity, autoSave: true);

        return ObjectMapper.Map<PolicyDocEntity, PolicyDocumentDto>(entity);
    }

    [Authorize(GrcPermissions.Policies.Approve)]
    public async Task<PolicyDocumentDto> ApproveAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        entity.Status = "Approved";

        await EnforceAsync("approve", "PolicyDocument", entity);
        await _repository.UpdateAsync(entity, autoSave: true);

        return ObjectMapper.Map<PolicyDocEntity, PolicyDocumentDto>(entity);
    }

    [Authorize(GrcPermissions.Policies.Publish)]
    public async Task<PolicyDocumentDto> PublishAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        entity.Status = "Published";

        await EnforceAsync("publish", "PolicyDocument", entity);
        await _repository.UpdateAsync(entity, autoSave: true);

        return ObjectMapper.Map<PolicyDocEntity, PolicyDocumentDto>(entity);
    }

    [Authorize(GrcPermissions.Policies.Manage)]
    public async Task<PolicyDocumentDto> UpdateAsync(Guid id, UpdatePolicyDocumentDto input)
    {
        var entity = await _repository.GetAsync(id);
        entity.Name = input.Name;
        entity.Description = input.Description;
        entity.Owner = input.Owner ?? entity.Owner;
        entity.DataClassification = input.DataClassification ?? entity.DataClassification;
        entity.ApprovedForProd = input.ApprovedForProd;

        if (entity.Labels == null)
            entity.Labels = new Dictionary<string, string>();

        entity.Labels["dataClassification"] = entity.DataClassification ?? "internal";
        entity.Labels["owner"] = entity.Owner ?? "unknown";
        entity.Labels["approvedForProd"] = entity.ApprovedForProd.ToString().ToLowerInvariant();

        await EnforceAsync("update", "PolicyDocument", entity);
        await _repository.UpdateAsync(entity, autoSave: true);

        return ObjectMapper.Map<PolicyDocEntity, PolicyDocumentDto>(entity);
    }

    public async Task<PolicyDocumentDto> GetAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        return ObjectMapper.Map<PolicyDocEntity, PolicyDocumentDto>(entity);
    }

    [Authorize(GrcPermissions.Policies.Manage)]
    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }
}
