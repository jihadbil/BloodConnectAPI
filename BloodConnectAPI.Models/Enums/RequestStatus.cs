using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BloodConnectAPI.Models.Enums;

/// <summary>
/// حالة طلب الدم
/// </summary>
public enum RequestStatus
{
    /// <summary>
    /// قيد الانتظار
    /// </summary>
    [Description("قيد الانتظار")]
    Pending = 1,

    /// <summary>
    /// تم التوفير بالكامل
    /// </summary>
    [Description("تم التوفير")]
    Fulfilled = 2,

    /// <summary>
    /// تم التوفير جزئياً
    /// </summary>
    [Description("تم التوفير جزئياً")]
    PartiallyFulfilled = 3,

    /// <summary>
    /// ملغى
    /// </summary>
    [Description("ملغى")]
    Cancelled = 4
}
