using Microsoft.AspNetCore.Identity;
using System;

namespace BloodConnectAPI.Models;

/// <summary>
/// يمثل مستخدم النظام (موظف، طبيب، مدير...)
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// الاسم الكامل
    /// </summary>
    public string FullName { get; set; } = null!;

    /// <summary>
    /// هل الحساب مفعل
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// تاريخ إنشاء الحساب
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// تاريخ آخر تحديث
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    // علاقات
    /// <summary>
    /// المتبرع المرتبط بالمستخدم (اختياري)
    /// </summary>
    public Donor? Donor { get; set; }
}
