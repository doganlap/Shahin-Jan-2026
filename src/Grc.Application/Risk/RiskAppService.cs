using RiskEntity = Grc.Domain.Risk.Risk;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Grc.Application.Policy;
using Grc.Application.Contracts.Risk;
using Grc.Domain.Risk;
using Grc.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Grc.Application.Risk;

[Authorize]
public class RiskAppService : BasePolicyAppService, IRiskAppService
{
    private readonly IRiskRepository _repository;

    public RiskAppService(
        IPolicyEnforcer policyEnforcer,
        Volo.Abp.Users.ICurrentUser currentUser,
        Volo.Abp.MultiTenancy.ICurrentTenant currentTenant,
        IRiskRepository repository,
        IEnvironmentProvider? environmentProvider = null,
        IRoleResolver? roleResolver = null)
        : base(policyEnforcer, currentUser, currentTenant, environmentProvider, roleResolver)
    {
        _repository = repository;
    }

    [Authorize(GrcPermissions.Risks.View)]
    public async Task<PagedResultDto<RiskDto>> GetListAsync(RiskListInputDto input)
    {
        var query = await CreateFilteredQueryAsync(input);
        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(e => e.CreationTime)
                 .PageBy(input.SkipCount, input.MaxResultCount));

        return new PagedResultDto<RiskDto>(
            totalCount,
            ObjectMapper.Map<List<RiskEntity>, List<RiskDto>>(items)
        );
    }

    protected virtual async Task<IQueryable<RiskEntity>> CreateFilteredQueryAsync(RiskListInputDto input)
    {
        var query = await _repository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(r => r.Name.Contains(input.Filter) || r.Description.Contains(input.Filter));
        }

        if (!string.IsNullOrWhiteSpace(input.Status))
        {
            query = query.Where(r => r.Status == input.Status);
        }

        if (!string.IsNullOrWhiteSpace(input.Owner))
        {
            query = query.Where(r => r.Owner == input.Owner);
        }

        if (!string.IsNullOrWhiteSpace(input.Severity))
        {
            query = query.Where(r => r.Severity == input.Severity);
        }

        if (!string.IsNullOrWhiteSpace(input.Category))
        {
            query = query.Where(r => r.Category == input.Category);
        }

        return query;
    }

    [Authorize(GrcPermissions.Risks.Manage)]
    public async Task<RiskDto> CreateAsync(CreateRiskDto input)
    {
        var entity = new RiskEntity(GuidGenerator.Create(), input.Name)
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

        await EnforceAsync("create", "Risk", entity);
        await _repository.InsertAsync(entity, autoSave: true);

        return ObjectMapper.Map<RiskEntity, RiskDto>(entity);
    }

    [Authorize(GrcPermissions.Risks.Manage)]
    public async Task<RiskDto> UpdateAsync(Guid id, UpdateRiskDto input)
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

        await EnforceAsync("update", "Risk", entity);
        await _repository.UpdateAsync(entity, autoSave: true);

        return ObjectMapper.Map<RiskEntity, RiskDto>(entity);
    }

    [Authorize(GrcPermissions.Risks.Accept)]
    public async Task<RiskDto> AcceptAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        entity.Status = "Accepted";

        await EnforceAsync("accept", "Risk", entity);
        await _repository.UpdateAsync(entity, autoSave: true);

        return ObjectMapper.Map<RiskEntity, RiskDto>(entity);
    }

    public async Task<RiskDto> GetAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        return ObjectMapper.Map<RiskEntity, RiskDto>(entity);
    }

    [Authorize(GrcPermissions.Risks.Manage)]
    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }
}
