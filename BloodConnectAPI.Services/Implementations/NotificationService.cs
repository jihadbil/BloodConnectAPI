using AutoMapper;
using BloodConnectAPI.DataAccess.Repositories.Interfaces;
using BloodConnectAPI.Models;
using BloodConnectAPI.Models.DTOs;
using BloodConnectAPI.Models.Enums;
using BloodConnectAPI.Service.Common;
using BloodConnectAPI.Service.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace BloodConnectAPI.Service.Implementations;

/// <summary>
/// خدمة إدارة الإشعارات
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        UserManager<ApplicationUser> userManager)
    {
        _unitOfWork  = unitOfWork;
        _mapper      = mapper;
        _userManager = userManager;
    }

    // ─── إنشاء الإشعارات ──────────────────────────────────────────────────────

    /// <summary>
    /// إنشاء إشعار لمستخدم واحد محدد
    /// </summary>
    public async Task CreateAsync(
        string recipientUserId,
        string title,
        string message,
        NotificationType type,
        string? relatedEntityType = null,
        int? relatedEntityId = null)
    {
        var notification = new Notification
        {
            RecipientUserId   = recipientUserId,
            Title             = title,
            Message           = message,
            Type              = type,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId   = relatedEntityId,
            IsRead            = false
        };

        await _unitOfWork.Notifications.AddAsync(notification);
        await _unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// إنشاء إشعار لجميع مستخدمي دور معين
    /// </summary>
    public async Task CreateForRoleAsync(
        string role,
        string title,
        string message,
        NotificationType type,
        string? relatedEntityType = null,
        int? relatedEntityId = null)
    {
        var usersInRole = await _userManager.GetUsersInRoleAsync(role);
        if (!usersInRole.Any()) return;

        var now = DateTime.UtcNow;

        var notifications = usersInRole.Select(user => new Notification
        {
            RecipientUserId   = user.Id,
            Title             = title,
            Message           = message,
            Type              = type,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId   = relatedEntityId,
            IsRead            = false,
            CreatedAt         = now
        }).ToList();

        await _unitOfWork.Notifications.AddRangeAsync(notifications);
        await _unitOfWork.SaveChangesAsync();
    }

    // ─── الاستعلام ────────────────────────────────────────────────────────────

    /// <summary>
    /// جلب إشعارات المستخدم (آخر 30 يوم فقط)
    /// </summary>
    public async Task<ServiceResponse<IEnumerable<NotificationDto>>> GetMyNotificationsAsync(
        string userId, bool unreadOnly = false)
    {
        var notifications = await _unitOfWork.Notifications.GetByUserIdAsync(userId, unreadOnly);
        var dtos = _mapper.Map<IEnumerable<NotificationDto>>(notifications);
        return ServiceResponse<IEnumerable<NotificationDto>>.SuccessResponse(dtos);
    }

    /// <summary>
    /// عدد الإشعارات غير المقروءة (آخر 30 يوم)
    /// </summary>
    public async Task<ServiceResponse<int>> GetUnreadCountAsync(string userId)
    {
        var count = await _unitOfWork.Notifications.GetUnreadCountAsync(userId);
        return ServiceResponse<int>.SuccessResponse(count);
    }

    // ─── تحديد كمقروء ─────────────────────────────────────────────────────────

    /// <summary>
    /// تحديد إشعار واحد كمقروء
    /// </summary>
    public async Task<ServiceResponse<bool>> MarkAsReadAsync(int notificationId, string userId)
    {
        var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);

        if (notification == null)
            return ServiceResponse<bool>.FailureResponse("الإشعار غير موجود");

        if (notification.RecipientUserId != userId)
            return ServiceResponse<bool>.FailureResponse("غير مصرح لك بتعديل هذا الإشعار");

        if (notification.IsRead)
            return ServiceResponse<bool>.SuccessResponse(true, "الإشعار مقروء مسبقاً");

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;

        await _unitOfWork.Notifications.UpdateAsync(notification);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResponse<bool>.SuccessResponse(true, "تم تحديد الإشعار كمقروء");
    }

    /// <summary>
    /// تحديد جميع الإشعارات كمقروءة
    /// </summary>
    public async Task<ServiceResponse<bool>> MarkAllAsReadAsync(string userId)
    {
        await _unitOfWork.Notifications.MarkAllAsReadAsync(userId);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResponse<bool>.SuccessResponse(true, "تم تحديد جميع الإشعارات كمقروءة");
    }

    // ─── الحذف ────────────────────────────────────────────────────────────────

    /// <summary>
    /// حذف إشعار
    /// </summary>
    public async Task<ServiceResponse<bool>> DeleteAsync(int notificationId, string userId)
    {
        var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);

        if (notification == null)
            return ServiceResponse<bool>.FailureResponse("الإشعار غير موجود");

        if (notification.RecipientUserId != userId)
            return ServiceResponse<bool>.FailureResponse("غير مصرح لك بحذف هذا الإشعار");

        await _unitOfWork.Notifications.DeleteAsync(notification);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResponse<bool>.SuccessResponse(true, "تم حذف الإشعار بنجاح");
    }
}
