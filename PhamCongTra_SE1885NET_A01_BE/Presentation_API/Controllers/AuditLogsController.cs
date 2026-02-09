using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BussinessLogic.Services;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;

namespace Presentation_API.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AuditLogsController : ODataController
    {
        private readonly IAuditService _auditService;
        public AuditLogsController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        /// <summary>
        /// Get audit logs with optional filtering
        /// </summary>
        /// <param name="userId">Optional: Filter by user ID</param>
        /// <param name="entityType">Optional: Filter by entity type (e.g., "NewsArticle", "Category", "Tag")</param>
        /// <returns>List of audit log entries</returns>
        [HttpGet]
        [EnableQuery]
        public IActionResult Get()
        {
            try
            {
                var query = _auditService.GetAuditLogsQueryable();
                return Ok(query);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving audit logs", error = ex.Message });
            }
        }
    }
}
