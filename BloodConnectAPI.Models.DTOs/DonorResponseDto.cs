using BloodConnectAPI.Models.Enums;

namespace BloodConnectAPI.Models.DTOs;

public class DonorResponseDto
{
    public int ResponseID { get; set; }
    public int DonorID { get; set; }
    public string DonorName { get; set; } = string.Empty;
    public int RequestID { get; set; }
    public string BloodTypeName { get; set; } = string.Empty;
    public ResponseStatus Status { get; set; }
    public string? Notes { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime ResponseDate { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public int? DonationID { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateDonorResponseDto
{
    public int DonorID { get; set; }
    public int RequestID { get; set; }
    public string? Notes { get; set; }
}

public class UpdateResponseStatusDto
{
    public ResponseStatus Status { get; set; }
    public string? Notes { get; set; }
    public string? RejectionReason { get; set; }
    public int? DonationID { get; set; }
}
