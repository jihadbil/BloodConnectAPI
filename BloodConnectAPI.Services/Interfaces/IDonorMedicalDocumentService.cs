using BloodConnectAPI.Models.DTOs;
using BloodConnectAPI.Service.Common;

namespace BloodConnectAPI.Service.Interfaces;

/// <summary>
/// خدمة إدارة وثائق المتبرعين الطبية
/// </summary>
public interface IDonorMedicalDocumentService
{
    Task<ServiceResponse<DonorMedicalDocumentDto>> UploadDocumentAsync(SubmitMedicalDocumentDto dto, string uploadsRootPath, string uploadedByUserId);
    Task<ServiceResponse<DonorMedicalDocumentDto>> GetByIdAsync(int documentId);
    Task<ServiceResponse<IEnumerable<DonorMedicalDocumentDto>>> GetByDonorIdAsync(int donorId);
    Task<ServiceResponse<DonorMedicalDocumentDto>> VerifyDocumentAsync(int documentId, VerifyMedicalDocumentDto dto, string verifiedByUserId);
    Task<ServiceResponse<bool>> DeleteAsync(int documentId, string uploadsRootPath);
}
