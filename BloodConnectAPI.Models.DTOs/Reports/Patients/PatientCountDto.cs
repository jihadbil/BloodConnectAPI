namespace BloodConnectAPI.Models.DTOs.Reports.Patients;

public class PatientCountDto
{
    public int TotalCount { get; set; }
    public List<MonthlyTrend>? Trends { get; set; }
}

public class MonthlyTrend
{
    public string Month { get; set; } = null!;
    public int Count { get; set; }
}
