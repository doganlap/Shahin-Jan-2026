using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Grc.Application.Contracts.ComplianceEvent;

public interface IComplianceCalendarAppService : IApplicationService
{
    Task<PagedResultDto<ComplianceEventDto>> GetListAsync(ComplianceEventListInputDto input);
    Task<ComplianceEventDto> GetAsync(Guid id);
    Task<ComplianceEventDto> CreateAsync(CreateComplianceEventDto input);
    Task<ComplianceEventDto> UpdateAsync(Guid id, UpdateComplianceEventDto input);
    Task DeleteAsync(Guid id);
    Task<List<ComplianceEventDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<List<ComplianceEventDto>> GetUpcomingAsync(int days = 30);
    Task<List<ComplianceEventDto>> GetOverdueAsync();
}
