using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BloodConnectAPI.Models.Enums;

/// <summary>
/// الجنس
/// </summary>
public enum Gender
{
    /// <summary>
    /// ذكر
    /// </summary>
    [Description("ذكر")]
    Male = 1,

    /// <summary>
    /// أنثى
    /// </summary>
    [Description("أنثى")]
    Female = 2
}
