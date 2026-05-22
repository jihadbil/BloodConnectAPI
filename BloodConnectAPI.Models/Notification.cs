using BloodConnectAPI.Models.Enums;

namespace BloodConnectAPI.Models;

/// <summary>
/// يمثل إشعاراً يُرسل إلى مستخدم في النظام
/// </summary>
public class Notification
{
    /// <summary>
    /// المعرف الفريد للإشعار
    /// </summary>
    public int NotificationID { get; set; }

    /// <summary>
    /// معرف المستخدم المستقبِل (FK → AspNetUsers.Id)
    /// </summary>
    public string RecipientUserId { get; set; } = null!;

    /// <summary>
    /// عنوان الإشعار
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// نص الإشعار التفصيلي
    /// </summary>
    public string Message { get; set; } = null!;

    /// <summary>
    /// نوع الإشعار
    /// </summary>
    public NotificationType Type { get; set; }

    /// <summary>
    /// هل تمت قراءة الإشعار؟
    /// </summary>
    public bool IsRead { get; set; } = false;

    /// <summary>
    /// نوع الكيان المرتبط (مثلاً: "BloodRequest"، "DonorResponse"...)
    /// </summary>
    public string? RelatedEntityType { get; set; }

    /// <summary>
    /// معرف الكيان المرتبط
    /// </summary>
    public int? RelatedEntityId { get; set; }

    /// <summary>
    /// تاريخ إنشاء الإشعار
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// تاريخ قراءة الإشعار (null = لم يُقرأ بعد)
    /// </summary>
    public DateTime? ReadAt { get; set; }

    // ─── Navigation ───────────────────────────────────────────────────────────

    /// <summary>
    /// المستخدم المستقبِل
    /// </summary>
    public ApplicationUser Recipient { get; set; } = null!;
}
