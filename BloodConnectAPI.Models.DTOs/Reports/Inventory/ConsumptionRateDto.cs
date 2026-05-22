namespace BloodConnectAPI.Models.DTOs.Reports.Inventory;

public class ConsumptionRateDto
{
    public string BloodType { get; set; } = null!;
    public int TotalConsumed { get; set; }
    public double AverageDailyConsumption { get; set; }
    public int CurrentInventory { get; set; }
    public int ProjectedDaysUntilStockout { get; set; }
}
