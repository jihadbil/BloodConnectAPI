using BloodConnectAPI.Models.Enums;

namespace BloodConnectAPI.Models.DTOs;

/// <summary>
/// DTO لقرار موافقة الإدارة على متبرع
/// القيم المسموح بها: Approved, Rejected, RequestMoreDocs
/// </summary>
public class ApproveDonorDto
{
    public DonorApprovalStatus NewStatus { get; set; }
    public string? RejectionReason { get; set; }  // مطلوب إذا كان Rejected أو RequestMoreDocs
}
