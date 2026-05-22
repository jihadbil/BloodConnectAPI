using System.ComponentModel;

namespace BloodConnectAPI.Models.Enums;

/// <summary>
/// حالة استجابة المتبرع لطلب الدم
/// </summary>
public enum ResponseStatus
{
    /// <summary>
    /// أبدى المتبرع اهتمامه بالتبرع
    /// </summary>
    [Description("مهتم بالتبرع")]
    Interested = 1,

    /// <summary>
    /// تم تأكيد الاستجابة من قِبل الموظف وتحديد موعد
    /// </summary>
    [Description("تم التأكيد")]
    Confirmed = 2,

    /// <summary>
    /// تم التبرع الفعلي وربطه بسجل تبرع
    /// </summary>
    [Description("تم التبرع")]
    Donated = 3,

    /// <summary>
    /// رُفضت الاستجابة (غير مؤهل، تراجع، إلخ)
    /// </summary>
    [Description("مرفوض")]
    Rejected = 4,

    /// <summary>
    /// أكد المتبرع لكنه لم يحضر
    /// </summary>
    [Description("لم يحضر")]
    NoShow = 5,

    /// <summary>
    /// تم إلغاء الاستجابة
    /// </summary>
    [Description("ملغى")]
    Cancelled = 6
}
