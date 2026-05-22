using System.ComponentModel;

namespace BloodConnectAPI.Models.Enums;

/// <summary>
/// حالة موافقة المتبرع
/// </summary>
public enum DonorApprovalStatus
{
    /// <summary>
    /// في انتظار رفع المستندات
    /// </summary>
    [Description("في انتظار رفع المستندات")]
    PendingDocuments = 1,

    /// <summary>
    /// تم الرفع، في انتظار مراجعة الإدارة
    /// </summary>
    [Description("في انتظار الموافقة")]
    PendingApproval = 2,

    /// <summary>
    /// تمت الموافقة — المتبرع نشط
    /// </summary>
    [Description("مقبول")]
    Approved = 3,

    /// <summary>
    /// مرفوض
    /// </summary>
    [Description("مرفوض")]
    Rejected = 4,

    /// <summary>
    /// طُلبت وثائق إضافية
    /// </summary>
    [Description("مطلوب وثائق إضافية")]
    RequestMoreDocs = 5
}
