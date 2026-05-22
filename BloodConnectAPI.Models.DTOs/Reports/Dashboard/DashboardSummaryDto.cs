namespace BloodConnectAPI.Models.DTOs.Reports.Dashboard;

public class DashboardSummaryDto
{
    public int ActiveDonorsCount { get; set; }
    public int InactiveDonorsCount { get; set; }
    public int DonationsThisMonth { get; set; }
    public int DonationsThisYear { get; set; }
    public int PendingRequestsCount { get; set; }
    public int FulfilledRequestsThisMonth { get; set; }
    public int EmergencyRequestsCount { get; set; }
    public List<InventoryStatus> InventoryByBloodType { get; set; } = new();
    public int ExpiringUnitsCount { get; set; }
}

public class InventoryStatus
{
    public string BloodType { get; set; } = null!;
    public int QuantityAvailable { get; set; }
}
