using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BloodConnectAPI.Models.Enums;

/// <summary>
/// مستوى الاستعجال لطلب الدم
/// </summary>
public enum UrgencyLevel
{
    /// <summary>
    /// عادي - غير عاجل
    /// </summary>
    [Description("عادي")]
    Normal = 1,

    /// <summary>
    /// عاجل - يحتاج أولوية
    /// </summary>
    [Description("عاجل")]
    Urgent = 2,

    /// <summary>
    /// طارئ - حالة حرجة
    /// </summary>
    [Description("طارئ")]
    Emergency = 3
}
