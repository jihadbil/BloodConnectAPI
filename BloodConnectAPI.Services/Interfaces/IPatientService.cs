using BloodConnectAPI.Models;
using BloodConnectAPI.Models.DTOs;
using BloodConnectAPI.Service.Common;

namespace BloodConnectAPI.Service.Interfaces;

/// <summary>
/// خدمة إدارة المرضى
/// </summary>
public interface IPatientService
{
    // CRUD
    Task<ServiceResponse<PatientDto>> GetByIdAsync(int id);
    Task<ServiceResponse<PagedResult<PatientDto>>> GetAllAsync(PaginationParams pagination);
    Task<ServiceResponse<PatientDto>> CreateAsync(CreatePatientDto dto);
    Task<ServiceResponse<PatientDto>> UpdateAsync(int id, UpdatePatientDto dto);
    Task<ServiceResponse<bool>> DeleteAsync(int id);
    
    // Business Operations
    Task<ServiceResponse<PatientDto>> GetByNationalIdAsync(string nationalId);
    Task<ServiceResponse<PatientDto>> GetWithRequestsAsync(int id);
}
