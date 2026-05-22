using BloodConnectAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace BloodConnectAPI.Models.DTOs;

/// <summary>
/// DTO لعرض بيانات المريض
/// </summary>
public class PatientDto
{
    public int PatientID { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string NationalID { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int BloodTypeID { get; set; }
    public string BloodTypeName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO لإنشاء مريض جديد
/// </summary>
public class CreatePatientDto
{
    [Required(ErrorMessage = "الاسم الكامل مطلوب")]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "الرقم الوطني مطلوب")]
    [StringLength(20)]
    public string NationalID { get; set; } = string.Empty;

    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }

    [Required(ErrorMessage = "رقم الهاتف مطلوب")]
    [Phone]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "المدينة مطلوبة")]
    public string City { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "يجب تحديد فصيلة الدم")]
    public int BloodTypeID { get; set; }
}

/// <summary>
/// DTO لتحديث بيانات مريض
/// </summary>
public class UpdatePatientDto
{
    public string? FullName { get; set; }
    public string? NationalID { get; set; }
    public Gender? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Phone { get; set; }
    public string? City { get; set; }
    public int? BloodTypeID { get; set; }
}
