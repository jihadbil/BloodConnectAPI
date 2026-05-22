namespace BloodConnectAPI.Models.DTOs;

/// <summary>
/// بيانات الإشعار المُرجَعة للواجهة
/// </summary>
public class NotificationDto
{
    public int NotificationID { get; set; }
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;

    /// <summary>
    /// نوع الإشعار كنص (مثلاً: "NewBloodRequest")
    /// </summary>
    public string Type { get; set; } = null!;

    public bool IsRead { get; set; }

    /// <summary>
    /// نوع الكيان المرتبط (اختياري)
    /// </summary>
    public string? RelatedEntityType { get; set; }

    /// <summary>
    /// معرف الكيان المرتبط (اختياري)
    /// </summary>
    public int? RelatedEntityId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}
