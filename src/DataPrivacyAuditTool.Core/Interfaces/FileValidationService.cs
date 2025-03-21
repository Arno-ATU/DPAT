// IFileValidationService.cs
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DataPrivacyAuditTool.Core.Interfaces
{
    public interface IFileValidationService
    {
        Task<bool> ValidateFileAsync(IFormFile file);
    }
}
