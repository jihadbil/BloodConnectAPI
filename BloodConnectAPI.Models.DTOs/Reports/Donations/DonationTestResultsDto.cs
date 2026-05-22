namespace BloodConnectAPI.Models.DTOs.Reports.Donations;

/// <summary>
/// DTO for donation test results statistics
/// </summary>
public class DonationTestResultsDto
{
    /// <summary>
    /// Number of donations with pending test results
    /// </summary>
    public int PendingCount { get; set; }
    
    /// <summary>
    /// Number of donations with accepted test results
    /// </summary>
    public int AcceptedCount { get; set; }
    
    /// <summary>
    /// Number of donations with rejected test results
    /// </summary>
    public int RejectedCount { get; set; }
    
    /// <summary>
    /// Total number of donations
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// Acceptance rate percentage
    /// </summary>
    public double AcceptanceRate { get; set; }
    
    /// <summary>
    /// Rejection rate percentage
    /// </summary>
    public double RejectionRate { get; set; }
}
