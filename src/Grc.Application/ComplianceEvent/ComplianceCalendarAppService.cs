using ComplianceEventEntity = Grc.Domain.ComplianceEvent.ComplianceEvent;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Grc.Application.Policy;
using Grc.Application.Contracts.ComplianceEvent;
using Grc.Domain.ComplianceEvent;
using Grc.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Grc.Application.ComplianceEvent;

[Authorize]
public class ComplianceCalendarAppService : BasePolicyAppService, IComplianceCalendarAppService
{
    private readonly IComplianceEventRepository _repository;

    public ComplianceCalendarAppService(
        IPolicyEnforcer policyEnforcer,
        Volo.Abp.Users.ICurrentUser currentUser,
        Volo.Abp.MultiTenancy.ICurrentTenant currentTenant,
        IComplianceEventRepository repository,
        IEnvironmentProvider? environmentProvider = null,
        IRoleResolver? roleResolver = null)
        : base(policyEnforcer, currentUser, currentTenant, environmentProvider, roleResolver)
    {
        _repository = repository;
    }

    [Authorize(GrcPermissions.ComplianceCalendar.View)]
    public async Task<PagedResultDto<ComplianceEventDto>> GetListAsync(ComplianceEventListInputDto input)
    {
        var query = await CreateFilteredQueryAsync(input);
        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(e => e.CreationTime)
                 .PageBy(input.SkipCount, input.MaxResultCount));

        return new PagedResultDto<ComplianceEventDto>(
            totalCount,
            ObjectMapper.Map<List<ComplianceEventEntity>, List<ComplianceEventDto>>(items)
        );
    }

    protected virtual async Task<IQueryable<ComplianceEventEntity>> CreateFilteredQueryAsync(ComplianceEventListInputDto input)
    {
        var query = await _repository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(ce => ce.Name.Contains(input.Filter) || ce.Description.Contains(input.Filter));
        }

        if (!string.IsNullOrWhiteSpace(input.Status))
        {
            query = query.Where(ce => ce.Status == input.Status);
        }

        if (input.DueDateFrom.HasValue)
        {
            query = query.Where(ce => ce.DueDate >= input.DueDateFrom.Value);
        }

        if (input.DueDateTo.HasValue)
        {
            query = query.Where(ce => ce.DueDate <= input.DueDateTo.Value);
        }

        if (input.FrameworkId.HasValue)
        {
            query = query.Where(ce => ce.RelatedFrameworkId == input.FrameworkId.Value);
        }

        return query;
    }

    public async Task<List<ComplianceEventDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var query = await _repository.GetQueryableAsync();
        var items = await AsyncExecuter.ToListAsync(
            query.Where(ce => ce.DueDate >= startDate && ce.DueDate <= endDate)
                 .OrderBy(ce => ce.DueDate));

        return ObjectMapper.Map<List<ComplianceEventEntity>, List<ComplianceEventDto>>(items);
    }

    public async Task<List<ComplianceEventDto>> GetUpcomingAsync(int days = 30)
    {
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddDays(days);
        return await GetByDateRangeAsync(startDate, endDate);
    }

    public async Task<List<ComplianceEventDto>> GetOverdueAsync()
    {
        var query = await _repository.GetQueryableAsync();
        var items = await AsyncExecuter.ToListAsync(
            query.Where(ce => ce.DueDate < DateTime.UtcNow && ce.Status != "Completed")
                 .OrderBy(ce => ce.DueDate));

        return ObjectMapper.Map<List<ComplianceEventEntity>, List<ComplianceEventDto>>(items);
    }

    [Authorize(GrcPermissions.ComplianceCalendar.Manage)]
    public async Task<ComplianceEventDto> CreateAsync(CreateComplianceEventDto input)
    {
        var entity = new ComplianceEventEntity(GuidGenerator.Create(), input.Name)
        {
            Description = input.Description,
            Owner = input.Owner ?? CurrentUser.UserName,
            DataClassification = input.DataClassification ?? "internal",
            EventType = input.EventType,
            EventDate = input.EventDate,
            DueDate = input.DueDate,
            RelatedFrameworkId = input.RelatedFrameworkId,
            RelatedRegulatorId = input.RelatedRegulatorId,
            Frequency = input.Frequency,
            Priority = input.Priority,
            Notes = input.Notes,
            Labels = new Dictionary<string, string>
            {
                ["dataClassification"] = input.DataClassification ?? "internal",
                ["owner"] = input.Owner ?? CurrentUser.UserName ?? "unknown"
            }
        };

        await EnforceAsync("create", "ComplianceEvent", entity);
        await _repository.InsertAsync(entity, autoSave: true);

        return ObjectMapper.Map<ComplianceEventEntity, ComplianceEventDto>(entity);
    }

    [Authorize(GrcPermissions.ComplianceCalendar.Manage)]
    public async Task<ComplianceEventDto> UpdateAsync(Guid id, UpdateComplianceEventDto input)
    {
        var entity = await _repository.GetAsync(id);
        entity.Name = input.Name;
        entity.Description = input.Description;
        entity.Owner = input.Owner ?? entity.Owner;
        entity.DataClassification = input.DataClassification ?? entity.DataClassification;
        entity.EventType = input.EventType;
        entity.EventDate = input.EventDate;
        entity.DueDate = input.DueDate;
        entity.RelatedFrameworkId = input.RelatedFrameworkId;
        entity.RelatedRegulatorId = input.RelatedRegulatorId;
        entity.Frequency = input.Frequency;
        entity.Priority = input.Priority;
        entity.Notes = input.Notes;

        if (entity.Labels == null)
            entity.Labels = new Dictionary<string, string>();

        entity.Labels["dataClassification"] = entity.DataClassification ?? "internal";
        entity.Labels["owner"] = entity.Owner ?? "unknown";

        await EnforceAsync("update", "ComplianceEvent", entity);
        await _repository.UpdateAsync(entity, autoSave: true);

        return ObjectMapper.Map<ComplianceEventEntity, ComplianceEventDto>(entity);
    }

    [Authorize(GrcPermissions.ComplianceCalendar.View)]
    public async Task<ComplianceEventDto> GetAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        return ObjectMapper.Map<ComplianceEventEntity, ComplianceEventDto>(entity);
    }

    [Authorize(GrcPermissions.ComplianceCalendar.Manage)]
    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }
}
