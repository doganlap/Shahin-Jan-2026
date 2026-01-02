using RegulatoryFrameworkEntity = Grc.Domain.RegulatoryFramework.RegulatoryFramework;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Grc.Application.Policy;
using Grc.Application.Contracts.RegulatoryFramework;
using Grc.Domain.RegulatoryFramework;
using Grc.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Grc.Application.RegulatoryFramework;

[Authorize]
public class RegulatoryFrameworkAppService : BasePolicyAppService, IRegulatoryFrameworkAppService
{
    private readonly IRegulatoryFrameworkRepository _repository;

    public RegulatoryFrameworkAppService(
        IPolicyEnforcer policyEnforcer,
        Volo.Abp.Users.ICurrentUser currentUser,
        Volo.Abp.MultiTenancy.ICurrentTenant currentTenant,
        IRegulatoryFrameworkRepository repository,
        IEnvironmentProvider? environmentProvider = null,
        IRoleResolver? roleResolver = null)
        : base(policyEnforcer, currentUser, currentTenant, environmentProvider, roleResolver)
    {
        _repository = repository;
    }

    [Authorize(GrcPermissions.Frameworks.View)]
    public async Task<PagedResultDto<RegulatoryFrameworkDto>> GetListAsync(RegulatoryFrameworkListInputDto input)
    {
        var query = await CreateFilteredQueryAsync(input);
        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(e => e.CreationTime)
                 .PageBy(input.SkipCount, input.MaxResultCount));

        return new PagedResultDto<RegulatoryFrameworkDto>(
            totalCount,
            ObjectMapper.Map<List<RegulatoryFrameworkEntity>, List<RegulatoryFrameworkDto>>(items)
        );
    }

    protected virtual async Task<IQueryable<RegulatoryFrameworkEntity>> CreateFilteredQueryAsync(RegulatoryFrameworkListInputDto input)
    {
        var query = await _repository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(rf => rf.Name.Contains(input.Filter) || rf.Description.Contains(input.Filter));
        }

        if (!string.IsNullOrWhiteSpace(input.Region))
        {
            query = query.Where(rf => rf.Jurisdiction.Contains(input.Region));
        }

        if (!string.IsNullOrWhiteSpace(input.Industry))
        {
            query = query.Where(rf => rf.FrameworkType.Contains(input.Industry));
        }

        if (input.RegulatorId.HasValue)
        {
            query = query.Where(rf => rf.RegulatorId == input.RegulatorId.Value);
        }

        return query;
    }

    [Authorize(GrcPermissions.Frameworks.Create)]
    public async Task<RegulatoryFrameworkDto> CreateAsync(CreateRegulatoryFrameworkDto input)
    {
        var entity = new RegulatoryFrameworkEntity(GuidGenerator.Create(), input.Name)
        {
            Description = input.Description,
            Owner = input.Owner ?? CurrentUser.UserName,
            DataClassification = input.DataClassification ?? "internal",
            FrameworkType = input.FrameworkType,
            Version = input.Version,
            EffectiveDate = input.EffectiveDate,
            ExpirationDate = input.ExpirationDate,
            Jurisdiction = input.Jurisdiction,
            Website = input.Website,
            Labels = new Dictionary<string, string>
            {
                ["dataClassification"] = input.DataClassification ?? "internal",
                ["owner"] = input.Owner ?? CurrentUser.UserName ?? "unknown"
            }
        };

        await EnforceAsync("create", "RegulatoryFramework", entity);
        await _repository.InsertAsync(entity, autoSave: true);

        return ObjectMapper.Map<RegulatoryFrameworkEntity, RegulatoryFrameworkDto>(entity);
    }

    [Authorize(GrcPermissions.Frameworks.Update)]
    public async Task<RegulatoryFrameworkDto> UpdateAsync(Guid id, UpdateRegulatoryFrameworkDto input)
    {
        var entity = await _repository.GetAsync(id);
        entity.Name = input.Name;
        entity.Description = input.Description;
        entity.Owner = input.Owner ?? entity.Owner;
        entity.DataClassification = input.DataClassification ?? entity.DataClassification;
        entity.FrameworkType = input.FrameworkType;
        entity.Version = input.Version;
        entity.EffectiveDate = input.EffectiveDate;
        entity.ExpirationDate = input.ExpirationDate;
        entity.Jurisdiction = input.Jurisdiction;
        entity.Website = input.Website;

        if (entity.Labels == null)
            entity.Labels = new Dictionary<string, string>();

        entity.Labels["dataClassification"] = entity.DataClassification ?? "internal";
        entity.Labels["owner"] = entity.Owner ?? "unknown";

        await EnforceAsync("update", "RegulatoryFramework", entity);
        await _repository.UpdateAsync(entity, autoSave: true);

        return ObjectMapper.Map<RegulatoryFrameworkEntity, RegulatoryFrameworkDto>(entity);
    }

    [Authorize(GrcPermissions.Frameworks.View)]
    public async Task<RegulatoryFrameworkDto> GetAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        return ObjectMapper.Map<RegulatoryFrameworkEntity, RegulatoryFrameworkDto>(entity);
    }

    [Authorize(GrcPermissions.Frameworks.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }
}
