namespace BloodConnectAPI.Models.DTOs.Reports.Donors;

/// <summary>
/// إحصائيات المتبرعين حسب فصيلة الدم
/// Donor statistics by blood type
/// </summary>
public class DonorStatisticsDto
{
    /// <summary>
    /// قائمة الإحصائيات لكل فصيلة دم
    /// List of statistics for each blood type
    /// </summary>
    public List<BloodTypeStatistic> Statistics { get; set; } = new();
}

/// <summary>
/// إحصائية فصيلة دم واحدة
/// Single blood type statistic
/// </summary>
public class BloodTypeStatistic
{
    /// <summary>
    /// فصيلة الدم (A+, A-, B+, B-, AB+, AB-, O+, O-)
    /// Blood type
    /// </summary>
    public string BloodType { get; set; } = null!;

    /// <summary>
    /// عدد المتبرعين لهذه الفصيلة
    /// Count of donors for this blood type
    /// </summary>
    public int Count { get; set; }
}
