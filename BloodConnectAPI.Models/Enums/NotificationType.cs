using System.ComponentModel;

namespace BloodConnectAPI.Models.Enums;

/// <summary>
/// أنواع الإشعارات في النظام
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// طلب دم جديد تم إنشاؤه
    /// </summary>
    [Description("طلب دم جديد")]
    NewBloodRequest = 1,

    /// <summary>
    /// استجابة متبرع جديدة لطلب دم
    /// </summary>
    [Description("استجابة متبرع جديدة")]
    NewDonorResponse = 2,

    /// <summary>
    /// تحديث حالة استجابة متبرع
    /// </summary>
    [Description("تحديث حالة الاستجابة")]
    ResponseStatusUpdated = 3,

    /// <summary>
    /// تغيُّر حالة طلب الدم
    /// </summary>
    [Description("تغيُّر حالة طلب الدم")]
    BloodRequestStatusChanged = 4,

    /// <summary>
    /// وثيقة طبية جديدة مرفوعة
    /// </summary>
    [Description("وثيقة طبية جديدة")]
    NewMedicalDocument = 5,

    /// <summary>
    /// مخزون دم منخفض
    /// </summary>
    [Description("مخزون دم منخفض")]
    LowBloodInventory = 6,

    /// <summary>
    /// إشعار عام
    /// </summary>
    [Description("إشعار عام")]
    General = 7
}
