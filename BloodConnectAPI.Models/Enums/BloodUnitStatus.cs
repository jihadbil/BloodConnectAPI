using System.ComponentModel;

namespace BloodConnectAPI.Models.Enums;

/// <summary>
/// حالة وحدة الدم في المخزون
/// </summary>
public enum BloodUnitStatus
{
    /// <summary>
    /// متاحة للاستخدام
    /// </summary>
    [Description("متاحة للاستخدام")]
    Available = 1,

    /// <summary>
    /// محجوزة لطلب معين
    /// </summary>
    [Description("محجوزة")]
    Reserved = 2,

    /// <summary>
    /// تم استخدامها (مصروفة)
    /// </summary>
    [Description("تم الاستخدام")]
    Used = 3,

    /// <summary>
    /// منتهية الصلاحية
    /// </summary>
    [Description("منتهية الصلاحية")]
    Expired = 4,

    /// <summary>
    /// تم التخلص منها
    /// </summary>
    [Description("تم التخلص منها")]
    Discarded = 5
}
