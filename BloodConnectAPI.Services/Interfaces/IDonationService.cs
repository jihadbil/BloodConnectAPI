using BloodConnectAPI.Models;
using BloodConnectAPI.Models.DTOs;
using BloodConnectAPI.Service.Common;

namespace BloodConnectAPI.Service.Interfaces;

/// <summary>
/// خدمة إدارة التبرعات
/// </summary>
public interface IDonationService
{
    // CRUD
    Task<ServiceResponse<DonationDto>> GetByIdAsync(int id);
    Task<ServiceResponse<PagedResult<DonationDto>>> GetAllAsync(PaginationParams pagination);
    Task<ServiceResponse<DonationDto>> CreateAsync(CreateDonationDto dto);
    Task<ServiceResponse<DonationDto>> UpdateAsync(int id, UpdateDonationDto dto);
    Task<ServiceResponse<bool>> DeleteAsync(int id);
    
    // Business Operations
    Task<ServiceResponse<IEnumerable<DonationDto>>> GetByDonorIdAsync(int donorId);
    Task<ServiceResponse<IEnumerable<DonationDto>>> GetRecentDonationsAsync(int days = 30);
    Task<ServiceResponse<DonationDto>> UpdateTestResultAsync(int id, BloodConnectAPI.Models.Enums.TestResult testResult);
    
    // Lab Testing
    Task<ServiceResponse<DonationDto>> LabTestDonationAsync(int donationId, LabTestDonationDto dto, string testedByUserId);
}
