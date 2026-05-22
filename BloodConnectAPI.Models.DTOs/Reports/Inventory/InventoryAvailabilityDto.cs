namespace BloodConnectAPI.Models.DTOs.Reports.Inventory;

public class InventoryAvailabilityDto
{
    public string BloodType { get; set; } = null!;
    public int QuantityAvailable { get; set; }
    public int QuantityReserved { get; set; }
    public int TotalAvailable { get; set; }
}
