using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace BloodConnectAPI.Models;

/// <summary>
/// جدول وسيط يربط المستخدمين بالأدوار
/// (Many To Many)
/// </summary>
public class UserRole
{
 
        /// <summary>
        /// المفتاح الأساسي لجدول UserRole
        /// </summary>
        public int UserRoleID { get; set; }

        public int UserID { get; set; }
        public int RoleID { get; set; }

        /// <summary>
        /// تاريخ تعيين الدور للمستخدم
        /// </summary>
        public DateTime AssignedAt { get; set; }

        /// <summary>
        /// تاريخ آخر تحديث
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // علاقات
    [JsonIgnore]
    public User User { get; set; } = null!;
    [JsonIgnore]
    public Role Role { get; set; } = null!;
    }
