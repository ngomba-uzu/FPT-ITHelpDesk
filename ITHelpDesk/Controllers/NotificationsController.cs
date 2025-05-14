using ITHelpDesk.Data;
using ITHelpDesk.Models;
using ITHelpDesk.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITHelpDesk.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly NotificationService _notificationService;
        private readonly UserManager<IdentityUser> _userManager;

        public NotificationsController(
            NotificationService notificationService,
            UserManager<IdentityUser> userManager)
        {
            _notificationService = notificationService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Json(new { success = false, message = "User not found" });

                var notifications = await _notificationService.GetUserNotifications(user.Id);

                var result = notifications.Select(n => new {
                    Id = n.Id,
                    Message = n.Message,
                    CreatedAt = n.CreatedAt.ToString("o"),
                    IsRead = n.IsRead,
                    TicketId = n.TicketId
                }).ToList();

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(0);

            var count = await _notificationService.GetUnreadCount(user.Id);
            return Json(count);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return BadRequest();

            await _notificationService.MarkAsRead(user.Id);
            return Ok();
        }
    }
}





