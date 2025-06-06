using ITHelpDesk.Data;
using ITHelpDesk.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ITHelpDesk.Services
{
    public class NotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            ApplicationDbContext context,
            ILogger<NotificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // For individual user notifications
        public async Task CreateUserNotification(string userId, string message, int? ticketId = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                TicketId = ticketId,
                CreatedAt = DateTime.Now,
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task CreateTechnicianNotification(int technicianId, string message, int? ticketId = null)
        {
            var technician = await _context.Technicians
                .FirstOrDefaultAsync(t => t.Id == technicianId);

            if (technician == null || string.IsNullOrEmpty(technician.UserId))
            {
                _logger.LogWarning($"Technician with Id {technicianId} not found or has no UserId.");
                return;
            }

            var notification = new Notification
            {
                UserId = technician.UserId,
                TechnicianGroupId = technician.TechnicianGroupId,
                Message = message,
                TicketId = ticketId,
                CreatedAt = DateTime.Now,
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }


        // Get notifications for current user
        public async Task<List<Notification>> GetUserNotifications(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5) // Limit to 5 notifications
                .ToListAsync();
        }



        // Get unread count
        public async Task<int> GetUnreadCount(string userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        // Mark notifications as read
        public async Task MarkAsRead(string userId)
        {
            var unread = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (unread.Any())
            {
                unread.ForEach(n => n.IsRead = true);
                await _context.SaveChangesAsync();
            }
        }

        // ⭐ Remove read notifications older than 1 day
        public async Task RemoveOldReadNotificationsAsync()
        {
            try
            {
                var oneDayAgo = DateTime.Now.AddDays(-1);

                var oldReadNotifications = await _context.Notifications
                    .Where(n => n.IsRead && n.CreatedAt < oneDayAgo)
                    .ToListAsync();

                if (oldReadNotifications.Any())
                {
                    _context.Notifications.RemoveRange(oldReadNotifications);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Removed {Count} old read notifications.", oldReadNotifications.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing old read notifications");
            }
        }
    }
}




