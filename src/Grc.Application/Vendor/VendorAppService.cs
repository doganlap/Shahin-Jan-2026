using VendorEntity = Grc.Domain.Vendor.Vendor;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Grc.Application.Policy;
using Grc.Application.Contracts.Vendor;
using Grc.Domain.Vendor;
using Grc.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Grc.Application.Vendor;

[Authorize]
public class VendorAppService : BasePolicyAppService, IVendorAppService
{
    private readonly IVendorRepository _repository;

    public VendorAppService(
        IPolicyEnforcer policyEnforcer,
        Volo.Abp.Users.ICurrentUser currentUser,
        Volo.Abp.MultiTenancy.ICurrentTenant currentTenant,
        IVendorRepository repository,
        IEnvironmentProvider? environmentProvider = null,
        IRoleResolver? roleResolver = null)
        : base(policyEnforcer, currentUser, currentTenant, environmentProvider, roleResolver)
    {
        _repository = repository;
    }

    [Authorize(GrcPermissions.Vendors.View)]
    public async Task<PagedResultDto<VendorDto>> GetListAsync(VendorListInputDto input)
    {
        var query = await CreateFilteredQueryAsync(input);
        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(e => e.CreationTime)
                 .PageBy(input.SkipCount, input.MaxResultCount));

        return new PagedResultDto<VendorDto>(
            totalCount,
            ObjectMapper.Map<List<VendorEntity>, List<VendorDto>>(items)
        );
    }

    protected virtual async Task<IQueryable<VendorEntity>> CreateFilteredQueryAsync(VendorListInputDto input)
    {
        var query = await _repository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(v => v.Name.Contains(input.Filter) || v.Description.Contains(input.Filter));
        }

        if (!string.IsNullOrWhiteSpace(input.Status))
        {
            query = query.Where(v => v.Status == input.Status);
        }

        if (!string.IsNullOrWhiteSpace(input.Category))
        {
            query = query.Where(v => v.VendorType == input.Category);
        }

        if (!string.IsNullOrWhiteSpace(input.RiskLevel))
        {
            query = query.Where(v => v.RiskRating == input.RiskLevel);
        }

        return query;
    }

    [Authorize(GrcPermissions.Vendors.Manage)]
    public async Task<VendorDto> CreateAsync(CreateVendorDto input)
    {
        var entity = new VendorEntity(GuidGenerator.Create(), input.Name)
        {
            Description = input.Description,
            Owner = input.Owner ?? CurrentUser.UserName,
            DataClassification = input.DataClassification ?? "internal",
            VendorType = input.VendorType,
            ContactName = input.ContactName,
            ContactEmail = input.ContactEmail,
            ContactPhone = input.ContactPhone,
            Website = input.Website,
            Address = input.Address,
            Country = input.Country,
            RiskRating = input.RiskRating,
            Labels = new Dictionary<string, string>
            {
                ["dataClassification"] = input.DataClassification ?? "internal",
                ["owner"] = input.Owner ?? CurrentUser.UserName ?? "unknown"
            }
        };

        await EnforceAsync("create", "Vendor", entity);
        await _repository.InsertAsync(entity, autoSave: true);

        return ObjectMapper.Map<VendorEntity, VendorDto>(entity);
    }

    [Authorize(GrcPermissions.Vendors.Manage)]
    public async Task<VendorDto> UpdateAsync(Guid id, UpdateVendorDto input)
    {
        var entity = await _repository.GetAsync(id);
        entity.Name = input.Name;
        entity.Description = input.Description;
        entity.Owner = input.Owner ?? entity.Owner;
        entity.DataClassification = input.DataClassification ?? entity.DataClassification;
        entity.VendorType = input.VendorType;
        entity.ContactName = input.ContactName;
        entity.ContactEmail = input.ContactEmail;
        entity.ContactPhone = input.ContactPhone;
        entity.Website = input.Website;
        entity.Address = input.Address;
        entity.Country = input.Country;
        entity.RiskRating = input.RiskRating;

        if (entity.Labels == null)
            entity.Labels = new Dictionary<string, string>();

        entity.Labels["dataClassification"] = entity.DataClassification ?? "internal";
        entity.Labels["owner"] = entity.Owner ?? "unknown";

        await EnforceAsync("update", "Vendor", entity);
        await _repository.UpdateAsync(entity, autoSave: true);

        return ObjectMapper.Map<VendorEntity, VendorDto>(entity);
    }

    [Authorize(GrcPermissions.Vendors.View)]
    public async Task<VendorDto> GetAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        return ObjectMapper.Map<VendorEntity, VendorDto>(entity);
    }

    [Authorize(GrcPermissions.Vendors.Assess)]
    public async Task<VendorDto> AssessAsync(Guid id, VendorAssessmentDto input)
    {
        var entity = await _repository.GetAsync(id);
        entity.RiskRating = input.RiskRating ?? entity.RiskRating;
        entity.LastAssessmentDate = input.AssessmentDate;
        entity.NextAssessmentDate = input.NextAssessmentDate;

        if (entity.Labels == null)
            entity.Labels = new Dictionary<string, string>();

        entity.Labels["lastAssessmentDate"] = input.AssessmentDate.ToString("yyyy-MM-dd");
        entity.Labels["riskRating"] = input.RiskRating ?? "Unknown";
        entity.Labels["complianceStatus"] = input.ComplianceStatus ?? "Unknown";

        await EnforceAsync("assess", "Vendor", entity);
        await _repository.UpdateAsync(entity, autoSave: true);

        return ObjectMapper.Map<VendorEntity, VendorDto>(entity);
    }

    [Authorize(GrcPermissions.Vendors.Manage)]
    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }
}
