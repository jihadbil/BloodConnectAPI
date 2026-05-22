namespace BloodConnectAPI.Models.DTOs.Reports.Requests;

public class RequestsByUrgencyDto
{
    public string UrgencyLevel { get; set; } = null!;
    public int RequestCount { get; set; }
}
