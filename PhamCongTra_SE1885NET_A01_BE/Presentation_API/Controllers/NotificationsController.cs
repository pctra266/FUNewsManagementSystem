using Microsoft.AspNetCore.Mvc;
using BussinessLogic.Services;

namespace Presentation_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public IActionResult GetRecentNotifications()
        {
            try
            {
                var notifications = _notificationService.GetRecentNotifications(10);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving notifications", error = ex.Message });
            }
        }
    }
}
