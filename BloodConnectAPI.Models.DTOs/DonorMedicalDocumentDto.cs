using Microsoft.AspNetCore.Http;

namespace BloodConnectAPI.Models.DTOs;

public class DonorMedicalDocumentDto
{
    public int DocumentID { get; set; }
    public int DonorID { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;   // المسار النسبي في wwwroot
    public DateTime UploadedAt { get; set; }
    public bool IsVerified { get; set; }
    public string? Notes { get; set; }
}

public class SubmitMedicalDocumentDto
{
    public int DonorID { get; set; }
    public string DocumentType { get; set; } = string.Empty;  // CBC, HBsAg, HIV, HCV, Syphilis
    public IFormFile File { get; set; } = null!;
}

public class VerifyMedicalDocumentDto
{
    public bool IsVerified { get; set; }
    public string? Notes { get; set; }
}
