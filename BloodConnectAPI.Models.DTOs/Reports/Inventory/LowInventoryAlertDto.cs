namespace BloodConnectAPI.Models.DTOs.Reports.Inventory;

public class LowInventoryAlertDto
{
    public string BloodType { get; set; } = null!;
    public int CurrentQuantity { get; set; }
    public int Threshold { get; set; }
    public string Severity { get; set; } = null!;
}
