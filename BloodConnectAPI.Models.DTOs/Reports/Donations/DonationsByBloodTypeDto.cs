namespace BloodConnectAPI.Models.DTOs.Reports.Donations;

/// <summary>
/// DTO for donations grouped by blood type
/// </summary>
public class DonationsByBloodTypeDto
{
    /// <summary>
    /// Blood type (e.g., "A+", "O-")
    /// </summary>
    public string BloodType { get; set; } = null!;
    
    /// <summary>
    /// Number of donations for this blood type
    /// </summary>
    public int DonationCount { get; set; }
    
    /// <summary>
    /// Percentage of total donations
    /// </summary>
    public double Percentage { get; set; }
}
