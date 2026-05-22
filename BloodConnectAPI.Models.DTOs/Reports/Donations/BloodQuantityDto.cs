namespace BloodConnectAPI.Models.DTOs.Reports.Donations;

/// <summary>
/// DTO for blood quantity donated by blood type
/// </summary>
public class BloodQuantityDto
{
    /// <summary>
    /// Blood type (e.g., "A+", "O-")
    /// </summary>
    public string BloodType { get; set; } = null!;
    
    /// <summary>
    /// Total quantity donated in milliliters
    /// </summary>
    public int TotalQuantityML { get; set; }
}
