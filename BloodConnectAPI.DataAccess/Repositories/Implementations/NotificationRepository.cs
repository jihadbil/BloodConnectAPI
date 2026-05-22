using BloodConnectAPI.DataAccess.Data;
using BloodConnectAPI.DataAccess.Repositories.Interfaces;
using BloodConnectAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BloodConnectAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// تنفيذ Repository الإشعارات
/// </summary>
public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    public NotificationRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// جلب إشعارات مستخدم معين (آخر 30 يوم فقط)
    /// </summary>
    public async Task<IEnumerable<Notification>> GetByUserIdAsync(string userId, bool unreadOnly = false)
    {
        var cutoff = DateTime.UtcNow.AddDays(-30);

        var query = _context.Notifications
            .Where(n => n.RecipientUserId == userId && n.CreatedAt >= cutoff);

        if (unreadOnly)
            query = query.Where(n => !n.IsRead);

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// عدد الإشعارات غير المقروءة لمستخدم معين (آخر 30 يوم)
    /// </summary>
    public async Task<int> GetUnreadCountAsync(string userId)
    {
        var cutoff = DateTime.UtcNow.AddDays(-30);

        return await _context.Notifications
            .CountAsync(n => n.RecipientUserId == userId
                          && !n.IsRead
                          && n.CreatedAt >= cutoff);
    }

    /// <summary>
    /// تحديد جميع إشعارات مستخدم كمقروءة (آخر 30 يوم)
    /// </summary>
    public async Task MarkAllAsReadAsync(string userId)
    {
        var cutoff = DateTime.UtcNow.AddDays(-30);
        var now    = DateTime.UtcNow;

        var unread = await _context.Notifications
            .Where(n => n.RecipientUserId == userId && !n.IsRead && n.CreatedAt >= cutoff)
            .ToListAsync();

        foreach (var n in unread)
        {
            n.IsRead = true;
            n.ReadAt = now;
        }

        // لا نستدعي SaveChanges هنا — تُدار من UnitOfWork
    }
}
