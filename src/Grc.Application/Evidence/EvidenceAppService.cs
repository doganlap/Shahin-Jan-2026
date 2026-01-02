using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Grc.Application.Policy;
using Grc.Application.Contracts.Evidence;
using Grc.Domain.Evidence;
using Grc.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using EvidenceEntity = Grc.Domain.Evidence.Evidence;

namespace Grc.Application.Evidence;

[Authorize]
public class EvidenceAppService : BasePolicyAppService, IEvidenceAppService
{
    private readonly IEvidenceRepository _repository;

    public EvidenceAppService(
        IPolicyEnforcer policyEnforcer,
        Volo.Abp.Users.ICurrentUser currentUser,
        Volo.Abp.MultiTenancy.ICurrentTenant currentTenant,
        IEvidenceRepository repository,
        IEnvironmentProvider? environmentProvider = null,
        IRoleResolver? roleResolver = null)
        : base(policyEnforcer, currentUser, currentTenant, environmentProvider, roleResolver)
    {
        _repository = repository;
    }

    [Authorize(GrcPermissions.Evidence.View)]
    public async Task<PagedResultDto<EvidenceDto>> GetListAsync(EvidenceListInputDto input)
    {
        var query = await CreateFilteredQueryAsync(input);
        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(e => e.CreationTime)
                 .PageBy(input.SkipCount, input.MaxResultCount));

        return new PagedResultDto<EvidenceDto>(
            totalCount,
            ObjectMapper.Map<List<EvidenceEntity>, List<EvidenceDto>>(items)
        );
    }

    protected virtual async Task<IQueryable<EvidenceEntity>> CreateFilteredQueryAsync(EvidenceListInputDto input)
    {
        var query = await _repository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(e => e.Name.Contains(input.Filter) || e.Description.Contains(input.Filter));
        }

        if (!string.IsNullOrWhiteSpace(input.Status))
        {
            query = query.Where(e => e.Status == input.Status);
        }

        if (!string.IsNullOrWhiteSpace(input.Owner))
        {
            query = query.Where(e => e.Owner == input.Owner);
        }

        if (!string.IsNullOrWhiteSpace(input.DataClassification))
        {
            query = query.Where(e => e.DataClassification == input.DataClassification);
        }

        return query;
    }

    [Authorize(GrcPermissions.Evidence.Upload)]
    public async Task<EvidenceDto> CreateAsync(CreateEvidenceDto input)
    {
        var entity = new EvidenceEntity(GuidGenerator.Create(), input.Name, input.Description)
        {
            Owner = input.Owner ?? CurrentUser.UserName,
            DataClassification = input.DataClassification ?? "internal",
            ApprovedForProd = input.ApprovedForProd,
            Labels = new Dictionary<string, string>
            {
                ["dataClassification"] = input.DataClassification ?? "internal",
                ["owner"] = input.Owner ?? CurrentUser.UserName ?? "unknown",
                ["approvedForProd"] = input.ApprovedForProd.ToString().ToLowerInvariant()
            }
        };

        await EnforceAsync("create", "Evidence", entity);
        await _repository.InsertAsync(entity, autoSave: true);

        return ObjectMapper.Map<EvidenceEntity, EvidenceDto>(entity);
    }

    [Authorize(GrcPermissions.Evidence.Update)]
    public async Task<EvidenceDto> UpdateAsync(Guid id, UpdateEvidenceDto input)
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

        await EnforceAsync("update", "Evidence", entity);
        await _repository.UpdateAsync(entity, autoSave: true);

        return ObjectMapper.Map<EvidenceEntity, EvidenceDto>(entity);
    }

    [Authorize(GrcPermissions.Evidence.Approve)]
    public async Task<EvidenceDto> ApproveAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        entity.ApprovedForProd = true;
        entity.Status = "Approved";

        if (entity.Labels == null)
            entity.Labels = new Dictionary<string, string>();

        entity.Labels["approvedForProd"] = "true";

        await EnforceAsync("approve", "Evidence", entity);
        await _repository.UpdateAsync(entity, autoSave: true);

        return ObjectMapper.Map<EvidenceEntity, EvidenceDto>(entity);
    }

    public async Task<EvidenceDto> GetAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        return ObjectMapper.Map<EvidenceEntity, EvidenceDto>(entity);
    }

    [Authorize(GrcPermissions.Evidence.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }
}
