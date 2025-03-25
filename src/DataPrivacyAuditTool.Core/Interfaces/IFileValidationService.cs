using Microsoft.AspNetCore.Http;

namespace DataPrivacyAuditTool.Core.Interfaces
{
    public interface IFileValidationService
    {
        Task<bool> ValidateSettingsFileAsync(IFormFile file);
        Task<bool> ValidateAddressesFileAsync(IFormFile file);
    }
}
