namespace BloodConnectAPI.Models.DTOs.Reports.Donations;

/// <summary>
/// DTO for donations grouped by time period
/// </summary>
public class DonationsByPeriodDto
{
    /// <summary>
    /// Period identifier (e.g., "2024-01" for month, "2024-W01" for week, "2024-01-15" for day)
    /// </summary>
    public string Period { get; set; } = null!;
    
    /// <summary>
    /// Number of donations in this period
    /// </summary>
    public int DonationCount { get; set; }
    
    /// <summary>
    /// Start date of the period
    /// </summary>
    public DateTime PeriodStart { get; set; }
    
    /// <summary>
    /// End date of the period
    /// </summary>
    public DateTime PeriodEnd { get; set; }
}
