using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Grc.Application.Contracts.PolicyDocument;

public interface IPolicyDocumentAppService : IApplicationService
{
    Task<PagedResultDto<PolicyDocumentDto>> GetListAsync(PolicyDocumentListInputDto input);
    Task<PolicyDocumentDto> GetAsync(Guid id);
    Task<PolicyDocumentDto> CreateAsync(CreatePolicyDocumentDto input);
    Task<PolicyDocumentDto> UpdateAsync(Guid id, UpdatePolicyDocumentDto input);
    Task<PolicyDocumentDto> ApproveAsync(Guid id);
    Task<PolicyDocumentDto> PublishAsync(Guid id);
    Task DeleteAsync(Guid id);
}
