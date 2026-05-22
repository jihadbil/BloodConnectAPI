namespace BloodConnectAPI.Models.DTOs.Reports.Donors;

/// <summary>
/// تقرير مقارنة المتبرعين النشطين وغير النشطين
/// Active vs inactive donors comparison report
/// </summary>
public class ActiveVsInactiveDonorsDto
{
    /// <summary>
    /// عدد المتبرعين النشطين
    /// Count of active donors
    /// </summary>
    public int ActiveCount { get; set; }

    /// <summary>
    /// عدد المتبرعين غير النشطين
    /// Count of inactive donors
    /// </summary>
    public int InactiveCount { get; set; }

    /// <summary>
    /// إجمالي عدد المتبرعين
    /// Total count of donors
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// نسبة المتبرعين النشطين
    /// Percentage of active donors
    /// </summary>
    public double ActivePercentage { get; set; }

    /// <summary>
    /// نسبة المتبرعين غير النشطين
    /// Percentage of inactive donors
    /// </summary>
    public double InactivePercentage { get; set; }
}
