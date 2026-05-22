namespace BloodConnectAPI.Models.DTOs.Reports.Donors;

/// <summary>
/// تقرير المتبرعين المؤهلين للتبرع حسب فصيلة الدم
/// Eligible donors by blood type report
/// </summary>
public class EligibleDonorsByBloodTypeDto
{
    /// <summary>
    /// فصيلة الدم (A+, A-, B+, B-, AB+, AB-, O+, O-)
    /// Blood type
    /// </summary>
    public string BloodType { get; set; } = null!;

    /// <summary>
    /// عدد المتبرعين المؤهلين لهذه الفصيلة
    /// Count of eligible donors for this blood type
    /// </summary>
    public int EligibleCount { get; set; }
}
