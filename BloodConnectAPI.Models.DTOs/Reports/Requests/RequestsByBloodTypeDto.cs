namespace BloodConnectAPI.Models.DTOs.Reports.Requests;

public class RequestsByBloodTypeDto
{
    public string BloodType { get; set; } = null!;
    public int RequestCount { get; set; }
    public double Percentage { get; set; }
}
