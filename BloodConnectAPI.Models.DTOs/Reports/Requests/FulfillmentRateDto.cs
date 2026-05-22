namespace BloodConnectAPI.Models.DTOs.Reports.Requests;

public class FulfillmentRateDto
{
    public string BloodType { get; set; } = null!;
    public int TotalRequests { get; set; }
    public int FulfilledRequests { get; set; }
    public double FulfillmentRate { get; set; }
}
