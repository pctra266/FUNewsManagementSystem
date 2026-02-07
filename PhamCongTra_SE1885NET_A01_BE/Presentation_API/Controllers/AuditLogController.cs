using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BussinessLogic.Services;

namespace Presentation_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditService _auditService;

        public AuditLogController(IAuditService auditService)
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
        public async Task<IActionResult> GetAuditLogs(
            [FromQuery] short? userId,
            [FromQuery] string? entityType)
        {
            try
            {
                var logs = await _auditService.GetAuditLogsAsync(userId, entityType);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving audit logs", error = ex.Message });
            }
        }
    }
}
