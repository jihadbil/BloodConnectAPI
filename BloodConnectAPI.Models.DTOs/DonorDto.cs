using System.Text.Json;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using BloodConnectAPI.Models.Enums;

namespace BloodConnectAPI.Models.DTOs;

/// <summary>
/// DTO لعرض بيانات المتبرع
/// </summary>
public class DonorDto
{
    public int DonorID { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string NationalID { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Phone { get; set; } = string.Empty;
    public int BloodTypeID { get; set; }
    public string BloodTypeName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public DateTime? LastDonationDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// معرف المستخدم المرتبط بالمتبرع (اختياري)
    /// </summary>
    public string? UserId { get; set; }
    
    /// <summary>
    /// اسم المستخدم المرتبط (اختياري)
    /// </summary>
    public string? Username { get; set; }

    public string? UserEmail { get; set; }
    
    public DonorApprovalStatus ApprovalStatus { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public string? ReviewedByUserId { get; set; }
}

/// <summary>
/// DTO لإنشاء متبرع جديد
/// </summary>
public class CreateDonorDto
{
    [Required(ErrorMessage = "الاسم الكامل مطلوب")]
    [StringLength(100, ErrorMessage = "الاسم لا يتجاوز 100 حرف")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "الرقم الوطني مطلوب")]
    [StringLength(20, ErrorMessage = "الرقم الوطني لا يتجاوز 20 حرفاً")]
    public string NationalID { get; set; } = string.Empty;

    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }

    [Required(ErrorMessage = "رقم الهاتف مطلوب")]
    [Phone(ErrorMessage = "رقم هاتف غير صالح")]
    public string Phone { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "يجب تحديد فصيلة الدم")]
    public int BloodTypeID { get; set; }

    [Required(ErrorMessage = "المدينة مطلوبة")]
    public string City { get; set; } = string.Empty;
    
    /// <summary>
    /// معرف المستخدم لربطه بالمتبرع عند الإنشاء (اختياري)
    /// </summary>
    public string? UserId { get; set; }

    public DonorApprovalStatus ApprovalStatus { get; set; } = DonorApprovalStatus.PendingDocuments;
}

/// <summary>
/// DTO لتحديث بيانات متبرع
/// </summary>
public class UpdateDonorDto
{
    public string? FullName { get; set; }
    public string? NationalID { get; set; }
    public Gender? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Phone { get; set; }
    public int? BloodTypeID { get; set; }
    public string? City { get; set; }
    public bool? IsActive { get; set; }

    /// <summary>
    /// معرف المستخدم لتحديث الربط (اختياري، null لفك الربط)
    /// </summary>
    public string? UserId { get; set; }

    public DonorApprovalStatus? ApprovalStatus { get; set; }
}
