using BloodConnectAPI.Models;
using BloodConnectAPI.Models.Enums;
using BloodConnectAPI.Models.DTOs;
using BloodConnectAPI.Service.Common;

namespace BloodConnectAPI.Service.Interfaces;

/// <summary>
/// خدمة إدارة طلبات الدم
/// </summary>
public interface IBloodRequestService
{
    // CRUD
    Task<ServiceResponse<BloodRequestDto>> GetByIdAsync(int id);
    Task<ServiceResponse<PagedResult<BloodRequestDto>>> GetAllAsync(PaginationParams pagination);
    Task<ServiceResponse<BloodRequestDto>> CreateAsync(CreateBloodRequestDto request);
    Task<ServiceResponse<BloodRequestDto>> UpdateStatusAsync(int id, RequestStatus status, string? notes = null);
    Task<ServiceResponse<bool>> CancelAsync(int id, string reason);
    
    // Business Operations
    Task<ServiceResponse<IEnumerable<BloodRequestDto>>> GetPendingRequestsAsync();
    Task<ServiceResponse<IEnumerable<BloodRequestDto>>> GetUrgentRequestsAsync();
    Task<ServiceResponse<BloodRequestDto>> FulfillRequestAsync(int requestId, FulfillBloodRequestDto dto, string fulfilledByUserId);
    Task<ServiceResponse<BloodRequestDto>> GetWithDetailsAsync(int id);
}
