using BloodConnectAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace BloodConnectAPI.Models.DTOs;

/// <summary>
/// DTO لعرض بيانات طلب الدم
/// </summary>
public class BloodRequestDto
{
    public int RequestID { get; set; }
    public int PatientID { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int BloodTypeID { get; set; }
    public string BloodTypeName { get; set; } = string.Empty;
    public int QuantityNeeded { get; set; }
    public UrgencyLevel UrgencyLevel { get; set; }
    public DateTime RequestDate { get; set; }
    public DateTime RequiredDate { get; set; }
    public RequestStatus Status { get; set; }
    public string? Notes { get; set; }
    public int QuantityFulfilled { get; set; }
    public int QuantityRemaining => QuantityNeeded - QuantityFulfilled;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO لإنشاء طلب دم جديد
/// </summary>
public class CreateBloodRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "يجب تحديد رقم المريض")]
    public int PatientID { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "يجب تحديد فصيلة الدم")]
    public int BloodTypeID { get; set; }

    [Range(1, 100, ErrorMessage = "الكمية المطلوبة يجب أن تكون بين 1 و 100")]
    public int QuantityNeeded { get; set; }

    public UrgencyLevel UrgencyLevel { get; set; }
    public DateTime RequiredDate { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO لتحديث بيانات طلب دم
/// </summary>
public class UpdateBloodRequestDto
{
    public int? QuantityNeeded { get; set; }
    public UrgencyLevel? UrgencyLevel { get; set; }
    public DateTime? RequiredDate { get; set; }
    public RequestStatus? Status { get; set; }
    public string? Notes { get; set; }
    public int? QuantityFulfilled { get; set; }
}
