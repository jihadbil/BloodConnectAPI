using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace BloodConnectAPI.Models;
/// <summary>
    /// يمثل دور المستخدم داخل النظام
    /// مثل: مدير النظام، موظف بنك الدم، طبيب
    /// </summary>
public class Role
{


  
        /// <summary>
        /// المفتاح الأساسي للدور
        /// </summary>
        public int RoleID { get; set; }

        /// <summary>
        /// اسم الدور
        /// </summary>
        public string RoleName { get; set; } = null!;

        /// <summary>
        /// وصف مختصر للدور
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// تاريخ إنشاء السجل
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// تاريخ آخر تحديث
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // علاقات
    [JsonIgnore]
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    

}
