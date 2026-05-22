using BloodConnectAPI.Models;
using BloodConnectAPI.Models.DTOs;
using BloodConnectAPI.Models.Enums;
using BloodConnectAPI.Service.Common;

namespace BloodConnectAPI.Service.Interfaces;

/// <summary>
/// خدمة إدارة المتبرعين
/// </summary>
public interface IDonorService
{
    Task<ServiceResponse<DonorDto>> GetByIdAsync(int id);
    Task<ServiceResponse<PagedResult<DonorDto>>> GetAllAsync(PaginationParams pagination);
    Task<ServiceResponse<DonorDto>> CreateAsync(CreateDonorDto dto);
    Task<ServiceResponse<DonorDto>> UpdateAsync(int id, UpdateDonorDto dto);
    Task<ServiceResponse<bool>> DeleteAsync(int id);

    /// <summary>
    /// الحصول على سجل المتبرع من خلال معرف المستخدم المرتبط به
    /// </summary>
    Task<ServiceResponse<DonorDto>> GetByUserIdAsync(string userId);

    // Business Operations
    Task<ServiceResponse<DonorDto>> GetByNationalIdAsync(string nationalId);
    Task<ServiceResponse<IEnumerable<DonorDto>>> GetEligibleDonorsAsync(int? bloodTypeId = null);
    Task<ServiceResponse<bool>> CheckEligibilityAsync(int donorId);
    Task<ServiceResponse<DonorDto>> GetWithDonationsAsync(int id);
    
    /// <summary>
    /// ربط متبرع بمستخدم
    /// </summary>
    /// <param name="donorId">معرف المتبرع</param>
    /// <param name="userId">معرف المستخدم</param>
    /// <returns>استجابة الخدمة تحتوي على بيانات المتبرع المحدثة</returns>
    Task<ServiceResponse<DonorDto>> LinkDonorToUserAsync(int donorId, string userId);
    
    /// <summary>
    /// فك ربط متبرع من مستخدم
    /// </summary>
    /// <param name="donorId">معرف المتبرع</param>
    /// <returns>استجابة الخدمة تحتوي على بيانات المتبرع المحدثة</returns>
    Task<ServiceResponse<DonorDto>> UnlinkDonorFromUserAsync(int donorId);
    
    /// <summary>
    /// الحصول على متبرع مع معلومات المستخدم المرتبط
    /// </summary>
    /// <param name="donorId">معرف المتبرع</param>
    /// <returns>استجابة الخدمة تحتوي على بيانات المتبرع مع معلومات المستخدم</returns>
    Task<ServiceResponse<DonorDto>> GetWithUserAsync(int donorId);
    
    // Approval Workflow
    Task<ServiceResponse<DonorDto>> ApproveDonorAsync(int donorId, ApproveDonorDto dto, string reviewerUserId);
    Task<ServiceResponse<PagedResult<DonorDto>>> GetByApprovalStatusAsync(DonorApprovalStatus status, PaginationParams pagination);

    // Medical Documents
    Task<ServiceResponse<IEnumerable<DonorMedicalDocumentDto>>> GetMedicalDocumentsAsync(int donorId);
}
