namespace BloodConnectAPI.Models.DTOs.Reports.Patients;

public class PatientsWithActiveRequestsDto
{
    public int PatientID { get; set; }
    public string PatientName { get; set; } = null!;
    public string BloodType { get; set; } = null!;
    public int ActiveRequestCount { get; set; }
    public string HighestUrgencyLevel { get; set; } = null!;
}
