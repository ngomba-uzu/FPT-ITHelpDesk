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
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        // For group notifications
        public async Task CreateGroupNotification(int groupId, string message, int? ticketId = null)
        {
            var technicians = await _context.Technicians
                .Where(t => t.TechnicianGroupId == groupId && t.UserId != null)
                .ToListAsync();

            foreach (var tech in technicians)
            {
                var notification = new Notification
                {
                    UserId = tech.UserId,
                    TechnicianGroupId = groupId,
                    Message = message,
                    TicketId = ticketId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Notifications.Add(notification);
            }

            await _context.SaveChangesAsync();
        }

        // Get notifications for current user
        public async Task<List<Notification>> GetUserNotifications(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(20)
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
    }

}


