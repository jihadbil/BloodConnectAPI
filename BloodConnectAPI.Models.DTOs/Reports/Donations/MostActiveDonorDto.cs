namespace BloodConnectAPI.Models.DTOs.Reports.Donations;

/// <summary>
/// DTO for most active donors report
/// </summary>
public class MostActiveDonorDto
{
    /// <summary>
    /// Donor ID
    /// </summary>
    public int DonorID { get; set; }
    
    /// <summary>
    /// Donor full name
    /// </summary>
    public string DonorName { get; set; } = null!;
    
    /// <summary>
    /// Donor's blood type
    /// </summary>
    public string BloodType { get; set; } = null!;
    
    /// <summary>
    /// Total number of donations
    /// </summary>
    public int DonationCount { get; set; }
}
