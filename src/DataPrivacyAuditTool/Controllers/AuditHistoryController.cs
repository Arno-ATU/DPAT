using DataPrivacyAuditTool.Core.Interfaces;
using DataPrivacyAuditTool.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace DataPrivacyAuditTool.Controllers
{
    public class AuditHistoryController : Controller
    {
        private readonly IAuditHistoryService _auditHistoryService;

        public AuditHistoryController(IAuditHistoryService auditHistoryService)
        {
            _auditHistoryService = auditHistoryService;
        }

        public async Task<IActionResult> Index()
        {
            var allAudits = await _auditHistoryService.GetAllAuditsAsync();
            return View(allAudits);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string username)
        {
            ViewBag.SearchUsername = username;

            if (string.IsNullOrWhiteSpace(username))
            {
                var allAudits = await _auditHistoryService.GetAllAuditsAsync();
                return View("Index", allAudits);
            }

            var userAudits = await _auditHistoryService.GetUserAuditHistoryAsync(username);
            return View("Index", userAudits);
        }
        }
    }
