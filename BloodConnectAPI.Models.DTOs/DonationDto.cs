using BloodConnectAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace BloodConnectAPI.Models.DTOs;

/// <summary>
/// DTO لعرض بيانات التبرع
/// </summary>
public class DonationDto
{
    public int DonationID { get; set; }
    public int DonorID { get; set; }
    public string DonorName { get; set; } = string.Empty;
    public int BloodTypeID { get; set; }
    public string BloodTypeName { get; set; } = string.Empty;
    public DateTime DonationDate { get; set; }
    public int Quantity { get; set; }
    public TestResult TestResult { get; set; }
    public string? Notes { get; set; }
    
    public DateTime? TestedAt { get; set; }
    public string? TestedByUserId { get; set; }
    public string? TestNotes { get; set; }
    public bool IsAddedToInventory { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO لإنشاء تبرع جديد
/// </summary>
public class CreateDonationDto
{
    [Range(1, int.MaxValue, ErrorMessage = "يجب تحديد رقم المتبرع")]
    public int DonorID { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "يجب تحديد فصيلة الدم")]
    public int BloodTypeID { get; set; }

    public DateTime DonationDate { get; set; }

    [Range(1, 5, ErrorMessage = "الكمية يجب أن تكون بين 1 و 5 وحدات")]
    public int Quantity { get; set; }

    public string? Notes { get; set; }
}

/// <summary>
/// DTO لتحديث بيانات تبرع
/// </summary>
public class UpdateDonationDto
{
    public DateTime? DonationDate { get; set; }
    public int? Quantity { get; set; }
    public TestResult? TestResult { get; set; }
    public string? Notes { get; set; }
    
    public string? TestNotes { get; set; }
    public bool? IsAddedToInventory { get; set; }
}
