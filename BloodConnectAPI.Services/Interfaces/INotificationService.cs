using BloodConnectAPI.Models.DTOs;
using BloodConnectAPI.Models.Enums;
using BloodConnectAPI.Service.Common;

namespace BloodConnectAPI.Service.Interfaces;

/// <summary>
/// واجهة خدمة الإشعارات
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// إنشاء إشعار لمستخدم محدد
    /// </summary>
    Task CreateAsync(
        string recipientUserId,
        string title,
        string message,
        NotificationType type,
        string? relatedEntityType = null,
        int? relatedEntityId = null);

    /// <summary>
    /// إنشاء إشعار لجميع مستخدمي دور معين
    /// </summary>
    Task CreateForRoleAsync(
        string role,
        string title,
        string message,
        NotificationType type,
        string? relatedEntityType = null,
        int? relatedEntityId = null);

    /// <summary>
    /// جلب إشعارات المستخدم الحالي (آخر 30 يوم)
    /// </summary>
    Task<ServiceResponse<IEnumerable<NotificationDto>>> GetMyNotificationsAsync(
        string userId, bool unreadOnly = false);

    /// <summary>
    /// عدد الإشعارات غير المقروءة
    /// </summary>
    Task<ServiceResponse<int>> GetUnreadCountAsync(string userId);

    /// <summary>
    /// تحديد إشعار واحد كمقروء
    /// </summary>
    Task<ServiceResponse<bool>> MarkAsReadAsync(int notificationId, string userId);

    /// <summary>
    /// تحديد جميع الإشعارات كمقروءة
    /// </summary>
    Task<ServiceResponse<bool>> MarkAllAsReadAsync(string userId);

    /// <summary>
    /// حذف إشعار
    /// </summary>
    Task<ServiceResponse<bool>> DeleteAsync(int notificationId, string userId);
}
