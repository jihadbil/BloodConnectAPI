namespace BloodConnectAPI.Models.DTOs.Reports.Requests;

public class AvgFulfillmentTimeDto
{
    public string UrgencyLevel { get; set; } = null!;
    public double AverageHours { get; set; }
    public int RequestCount { get; set; }
}
