using System.Threading.Tasks;
using DataPrivacyAuditTool.Core.Models;
using DataPrivacyAuditTool.Core.Models.ViewModels;

namespace DataPrivacyAuditTool.Core.Interfaces
{
    public interface IPrivacyDashboardService
    {
        PrivacyDashboardViewModel PrepareDashboardData(PrivacyAnalysisResult result);
    }
}
