using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Grc.Application.Contracts.Audit;

public interface IAuditAppService : IApplicationService
{
    Task<PagedResultDto<AuditDto>> GetListAsync(AuditListInputDto input);
    Task<AuditDto> GetAsync(Guid id);
    Task<AuditDto> CreateAsync(CreateAuditDto input);
    Task<AuditDto> UpdateAsync(Guid id, UpdateAuditDto input);
    Task<AuditDto> CloseAsync(Guid id);
    Task DeleteAsync(Guid id);
}
