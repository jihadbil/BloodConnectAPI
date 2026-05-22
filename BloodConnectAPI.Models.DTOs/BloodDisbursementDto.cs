namespace BloodConnectAPI.Models.DTOs;

/// <summary>
/// DTO لعرض بيانات صرف الدم
/// </summary>
public class BloodDisbursementDto
{
    public int DisbursementID { get; set; }
    public int RequestID { get; set; }
    public int DonationID { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string DonorName { get; set; } = string.Empty;
    public string BloodTypeName { get; set; } = string.Empty;
    public int QuantityUsed { get; set; }
    public DateTime DisbursementDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO لإنشاء عملية صرف دم جديدة
/// </summary>
public class CreateBloodDisbursementDto
{
    public int RequestID { get; set; }
    public int DonationID { get; set; }
    public int QuantityUsed { get; set; }
    public DateTime DisbursementDate { get; set; }
}

/// <summary>
/// DTO لتحديث بيانات صرف الدم
/// </summary>
public class UpdateBloodDisbursementDto
{
    public int? QuantityUsed { get; set; }
    public DateTime? DisbursementDate { get; set; }
}
