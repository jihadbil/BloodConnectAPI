namespace BloodConnectAPI.Models.DTOs.Reports.Donors;

/// <summary>
/// تقرير توزيع المتبرعين حسب المدينة
/// Donors distribution by city report
/// </summary>
public class DonorsByCityDto
{
    /// <summary>
    /// اسم المدينة
    /// City name
    /// </summary>
    public string City { get; set; } = null!;

    /// <summary>
    /// عدد المتبرعين في هذه المدينة
    /// Count of donors in this city
    /// </summary>
    public int DonorCount { get; set; }
}
