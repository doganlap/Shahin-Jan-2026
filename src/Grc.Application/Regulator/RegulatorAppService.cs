using RegulatorEntity = Grc.Domain.Regulator.Regulator;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Grc.Application.Policy;
using Grc.Application.Contracts.Regulator;
using Grc.Domain.Regulator;
using Grc.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Grc.Application.Regulator;

[Authorize]
public class RegulatorAppService : BasePolicyAppService, IRegulatorAppService
{
    private readonly IRegulatorRepository _repository;

    public RegulatorAppService(
        IPolicyEnforcer policyEnforcer,
        Volo.Abp.Users.ICurrentUser currentUser,
        Volo.Abp.MultiTenancy.ICurrentTenant currentTenant,
        IRegulatorRepository repository,
        IEnvironmentProvider? environmentProvider = null,
        IRoleResolver? roleResolver = null)
        : base(policyEnforcer, currentUser, currentTenant, environmentProvider, roleResolver)
    {
        _repository = repository;
    }

    [Authorize(GrcPermissions.Regulators.View)]
    public async Task<PagedResultDto<RegulatorDto>> GetListAsync(RegulatorListInputDto input)
    {
        var query = await CreateFilteredQueryAsync(input);
        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(e => e.CreationTime)
                 .PageBy(input.SkipCount, input.MaxResultCount));

        return new PagedResultDto<RegulatorDto>(
            totalCount,
            ObjectMapper.Map<List<RegulatorEntity>, List<RegulatorDto>>(items)
        );
    }

    protected virtual async Task<IQueryable<RegulatorEntity>> CreateFilteredQueryAsync(RegulatorListInputDto input)
    {
        var query = await _repository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(r => r.Name.Contains(input.Filter) || r.Description.Contains(input.Filter));
        }

        if (!string.IsNullOrWhiteSpace(input.Region))
        {
            query = query.Where(r => r.Region == input.Region);
        }

        if (!string.IsNullOrWhiteSpace(input.Industry))
        {
            query = query.Where(r => r.RegulatorType.Contains(input.Industry));
        }

        return query;
    }

    [Authorize(GrcPermissions.Regulators.Manage)]
    public async Task<RegulatorDto> CreateAsync(CreateRegulatorDto input)
    {
        var entity = new RegulatorEntity(GuidGenerator.Create(), input.Name)
        {
            Description = input.Description,
            Owner = input.Owner ?? CurrentUser.UserName,
            DataClassification = input.DataClassification ?? "internal",
            RegulatorType = input.RegulatorType,
            Country = input.Country,
            Region = input.Region,
            ContactEmail = input.ContactEmail,
            ContactPhone = input.ContactPhone,
            Website = input.Website,
            Address = input.Address,
            Labels = new Dictionary<string, string>
            {
                ["dataClassification"] = input.DataClassification ?? "internal",
                ["owner"] = input.Owner ?? CurrentUser.UserName ?? "unknown"
            }
        };

        await EnforceAsync("create", "Regulator", entity);
        await _repository.InsertAsync(entity, autoSave: true);

        return ObjectMapper.Map<RegulatorEntity, RegulatorDto>(entity);
    }

    [Authorize(GrcPermissions.Regulators.Manage)]
    public async Task<RegulatorDto> UpdateAsync(Guid id, UpdateRegulatorDto input)
    {
        var entity = await _repository.GetAsync(id);
        entity.Name = input.Name;
        entity.Description = input.Description;
        entity.Owner = input.Owner ?? entity.Owner;
        entity.DataClassification = input.DataClassification ?? entity.DataClassification;
        entity.RegulatorType = input.RegulatorType;
        entity.Country = input.Country;
        entity.Region = input.Region;
        entity.ContactEmail = input.ContactEmail;
        entity.ContactPhone = input.ContactPhone;
        entity.Website = input.Website;
        entity.Address = input.Address;

        if (entity.Labels == null)
            entity.Labels = new Dictionary<string, string>();

        entity.Labels["dataClassification"] = entity.DataClassification ?? "internal";
        entity.Labels["owner"] = entity.Owner ?? "unknown";

        await EnforceAsync("update", "Regulator", entity);
        await _repository.UpdateAsync(entity, autoSave: true);

        return ObjectMapper.Map<RegulatorEntity, RegulatorDto>(entity);
    }

    [Authorize(GrcPermissions.Regulators.View)]
    public async Task<RegulatorDto> GetAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        return ObjectMapper.Map<RegulatorEntity, RegulatorDto>(entity);
    }

    [Authorize(GrcPermissions.Regulators.Manage)]
    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }
}
