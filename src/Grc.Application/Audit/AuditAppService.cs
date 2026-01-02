using AuditEntity = Grc.Domain.Audit.Audit;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Grc.Application.Policy;
using Grc.Application.Contracts.Audit;
using Grc.Domain.Audit;
using Grc.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Grc.Application.Audit;

[Authorize]
public class AuditAppService : BasePolicyAppService, IAuditAppService
{
    private readonly IAuditRepository _repository;

    public AuditAppService(
        IPolicyEnforcer policyEnforcer,
        Volo.Abp.Users.ICurrentUser currentUser,
        Volo.Abp.MultiTenancy.ICurrentTenant currentTenant,
        IAuditRepository repository,
        IEnvironmentProvider? environmentProvider = null,
        IRoleResolver? roleResolver = null)
        : base(policyEnforcer, currentUser, currentTenant, environmentProvider, roleResolver)
    {
        _repository = repository;
    }

    [Authorize(GrcPermissions.Audits.View)]
    public async Task<PagedResultDto<AuditDto>> GetListAsync(AuditListInputDto input)
    {
        var query = await CreateFilteredQueryAsync(input);
        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(e => e.CreationTime)
                 .PageBy(input.SkipCount, input.MaxResultCount));

        return new PagedResultDto<AuditDto>(
            totalCount,
            ObjectMapper.Map<List<AuditEntity>, List<AuditDto>>(items)
        );
    }

    protected virtual async Task<IQueryable<AuditEntity>> CreateFilteredQueryAsync(AuditListInputDto input)
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

        if (input.StartDateFrom.HasValue)
        {
            query = query.Where(a => a.StartDate >= input.StartDateFrom.Value);
        }

        if (input.StartDateTo.HasValue)
        {
            query = query.Where(a => a.StartDate <= input.StartDateTo.Value);
        }

        return query;
    }

    [Authorize(GrcPermissions.Audits.Manage)]
    public async Task<AuditDto> CreateAsync(CreateAuditDto input)
    {
        var entity = new AuditEntity(GuidGenerator.Create(), input.Name)
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

        await EnforceAsync("create", "Audit", entity);
        await _repository.InsertAsync(entity, autoSave: true);

        return ObjectMapper.Map<AuditEntity, AuditDto>(entity);
    }

    [Authorize(GrcPermissions.Audits.Manage)]
    public async Task<AuditDto> UpdateAsync(Guid id, UpdateAuditDto input)
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

        await EnforceAsync("update", "Audit", entity);
        await _repository.UpdateAsync(entity, autoSave: true);

        return ObjectMapper.Map<AuditEntity, AuditDto>(entity);
    }

    [Authorize(GrcPermissions.Audits.Close)]
    public async Task<AuditDto> CloseAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        entity.Status = "Closed";

        await EnforceAsync("close", "Audit", entity);
        await _repository.UpdateAsync(entity, autoSave: true);

        return ObjectMapper.Map<AuditEntity, AuditDto>(entity);
    }

    public async Task<AuditDto> GetAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        return ObjectMapper.Map<AuditEntity, AuditDto>(entity);
    }

    [Authorize(GrcPermissions.Audits.Manage)]
    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }
}
