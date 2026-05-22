using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BloodConnectAPI.Models.Enums;

/// <summary>
/// نتيجة فحص الدم المتبرع به
/// </summary>
public enum TestResult
{
    /// <summary>
    /// قيد الفحص - لم يتم الفحص بعد
    /// </summary>
    [Description("قيد الفحص")]
    Pending = 1,

    /// <summary>
    /// مقبول - صالح للاستخدام
    /// </summary>
    [Description("مقبول")]
    Accepted = 2,

    /// <summary>
    /// مرفوض - غير صالح للاستخدام
    /// </summary>
    [Description("مرفوض")]
    Rejected = 3
}
