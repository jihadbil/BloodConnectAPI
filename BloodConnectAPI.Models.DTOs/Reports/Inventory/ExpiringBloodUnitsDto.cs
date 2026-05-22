namespace BloodConnectAPI.Models.DTOs.Reports.Inventory;

public class ExpiringBloodUnitsDto
{
    public string BloodType { get; set; } = null!;
    public int UnitCount { get; set; }
    public int DaysUntilExpiry { get; set; }
    public DateTime ExpiryDate { get; set; }
}
