using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Grc.Application.Contracts.Vendor;

public interface IVendorAppService : IApplicationService
{
    Task<PagedResultDto<VendorDto>> GetListAsync(VendorListInputDto input);
    Task<VendorDto> GetAsync(Guid id);
    Task<VendorDto> CreateAsync(CreateVendorDto input);
    Task<VendorDto> UpdateAsync(Guid id, UpdateVendorDto input);
    Task<VendorDto> AssessAsync(Guid id, VendorAssessmentDto input);
    Task DeleteAsync(Guid id);
}
