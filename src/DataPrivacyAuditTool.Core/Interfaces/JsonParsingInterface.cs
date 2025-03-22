using Microsoft.AspNetCore.Http;
using DataPrivacyAuditTool.Core.Models;

namespace DataPrivacyAuditTool.Core.Interfaces
{
    public interface IJsonParsingService
    {
        Task<SettingsData> ParseSettingsJsonAsync(IFormFile file);
        Task<AddressData> ParseAddressesJsonAsync(IFormFile file);
    }
}
