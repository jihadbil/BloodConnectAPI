using BloodConnectAPI.Models;

namespace BloodConnectAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// Repository الإشعارات
/// </summary>
public interface INotificationRepository : IGenericRepository<Notification>
{
    /// <summary>
    /// جلب إشعارات مستخدم معين (آخر 30 يوم فقط)
    /// </summary>
    /// <param name="userId">معرف المستخدم</param>
    /// <param name="unreadOnly">إذا كان true يجلب غير المقروء فقط</param>
    Task<IEnumerable<Notification>> GetByUserIdAsync(string userId, bool unreadOnly = false);

    /// <summary>
    /// عدد الإشعارات غير المقروءة لمستخدم معين (آخر 30 يوم)
    /// </summary>
    Task<int> GetUnreadCountAsync(string userId);

    /// <summary>
    /// تحديد جميع إشعارات مستخدم كمقروءة
    /// </summary>
    Task MarkAllAsReadAsync(string userId);
}
