using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace BloodConnectAPI.Models;


/// <summary>
/// يمثل مستخدم النظام (موظف، طبيب، مدير...)
/// </summary>
public class User
{

   
        /// <summary>
        /// المفتاح الأساسي للمستخدم
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// اسم المستخدم لتسجيل الدخول
        /// </summary>
        public string Username { get; set; } = null!;

        /// <summary>
        /// كلمة المرور (مشفرة)
        /// </summary>
        public string PasswordHash { get; set; } = null!;

        /// <summary>
        /// الاسم الكامل
        /// </summary>
        public string FullName { get; set; } = null!;

        /// <summary>
        /// رقم الهاتف
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// هل الحساب مفعل
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// تاريخ إنشاء الحساب
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// تاريخ آخر تحديث
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // علاقات
    [JsonIgnore]
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    /// <summary>
    /// المتبرع المرتبط بالمستخدم (اختياري)
    /// </summary>
    [JsonIgnore]
    public Donor? Donor { get; set; }
    }
