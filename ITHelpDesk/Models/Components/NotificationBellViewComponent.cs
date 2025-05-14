using ITHelpDesk.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ITHelpDesk.Models.Components
{
    public class NotificationBellViewComponent : ViewComponent
    {
        private readonly NotificationService _notificationService;
        private readonly UserManager<IdentityUser> _userManager;

        public NotificationBellViewComponent(
            NotificationService notificationService,
            UserManager<IdentityUser> userManager)
        {
            _notificationService = notificationService;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null) return Content(string.Empty);

            var count = await _notificationService.GetUnreadCount(user.Id);
            return View(count);
        }
    }
}



