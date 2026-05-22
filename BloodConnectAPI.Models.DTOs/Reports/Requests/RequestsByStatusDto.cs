namespace BloodConnectAPI.Models.DTOs.Reports.Requests;

public class RequestsByStatusDto
{
    public int PendingCount { get; set; }
    public int FulfilledCount { get; set; }
    public int CancelledCount { get; set; }
    public int TotalCount { get; set; }
    public double FulfillmentRate { get; set; }
    public double CancellationRate { get; set; }
}
