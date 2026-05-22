namespace BloodConnectAPI.Models.DTOs.Reports.Patients;

public class PatientsByBloodTypeDto
{
    public string BloodType { get; set; } = null!;
    public int PatientCount { get; set; }
    public double Percentage { get; set; }
}
