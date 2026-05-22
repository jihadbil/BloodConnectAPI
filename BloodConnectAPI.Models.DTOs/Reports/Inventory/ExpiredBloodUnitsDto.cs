namespace BloodConnectAPI.Models.DTOs.Reports.Inventory;

public class ExpiredBloodUnitsDto
{
    public string BloodType { get; set; } = null!;
    public int ExpiredUnitCount { get; set; }
    public int TotalQuantityWasted { get; set; }
}
