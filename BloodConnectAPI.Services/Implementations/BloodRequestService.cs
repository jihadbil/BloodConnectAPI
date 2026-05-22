using AutoMapper;
using BloodConnectAPI.DataAccess.Repositories.Interfaces;
using BloodConnectAPI.Models;
using BloodConnectAPI.Models.Enums;
using BloodConnectAPI.Models.DTOs;
using BloodConnectAPI.Service.Common;
using BloodConnectAPI.Service.Exceptions;
using BloodConnectAPI.Service.Interfaces;

namespace BloodConnectAPI.Service.Implementations;

/// <summary>
/// خدمة إدارة طلبات الدم
/// </summary>
public class BloodRequestService : IBloodRequestService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;

    public BloodRequestService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService)
    {
        _unitOfWork          = unitOfWork;
        _mapper              = mapper;
        _notificationService = notificationService;
    }

    public async Task<ServiceResponse<BloodRequestDto>> GetByIdAsync(int id)
    {
        var request = await _unitOfWork.BloodRequests.GetByIdAsync(id);
        
        if (request == null)
            return ServiceResponse<BloodRequestDto>.FailureResponse("الطلب غير موجود");

        var dto = _mapper.Map<BloodRequestDto>(request);
        return ServiceResponse<BloodRequestDto>.SuccessResponse(dto);
    }

    public async Task<ServiceResponse<PagedResult<BloodRequestDto>>> GetAllAsync(PaginationParams pagination)
    {
        var (requests, totalCount) = await _unitOfWork.BloodRequests.GetPagedAsync(
            pagination.PageNumber,
            pagination.PageSize,
            orderBy: query => pagination.SortDescending ?
                     query.OrderByDescending(r => r.RequestDate) :
                     query.OrderBy(r => r.RequestDate)
        );

        var dtos = _mapper.Map<List<BloodRequestDto>>(requests);
        
        var result = new PagedResult<BloodRequestDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };

        return ServiceResponse<PagedResult<BloodRequestDto>>.SuccessResponse(result);
    }

    public async Task<ServiceResponse<BloodRequestDto>> CreateAsync(CreateBloodRequestDto dto)
    {
        // Validation
        var patient = await _unitOfWork.Patients.GetByIdAsync(dto.PatientID);
        if (patient == null)
            return ServiceResponse<BloodRequestDto>.FailureResponse("المريض غير موجود");

        var bloodType = await _unitOfWork.BloodTypes.GetByIdAsync(dto.BloodTypeID);
        if (bloodType == null)
            return ServiceResponse<BloodRequestDto>.FailureResponse("فصيلة الدم غير موجودة");

        var request = _mapper.Map<BloodRequest>(dto);
        request.RequestDate = DateTime.UtcNow;
        request.Status = RequestStatus.Pending;
        request.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.BloodRequests.AddAsync(request);
        await _unitOfWork.SaveChangesAsync();

        // إشعار الموظفين والمدير بطلب دم جديد
        await _notificationService.CreateForRoleAsync(
            role: "Staff",
            title: "طلب دم جديد",
            message: $"تم إنشاء طلب دم جديد للمريض '{patient.FullName}' بفصيلة {bloodType.TypeName}",
            type: NotificationType.NewBloodRequest,
            relatedEntityType: "BloodRequest",
            relatedEntityId: request.RequestID);

        var responseDto = _mapper.Map<BloodRequestDto>(request);
        return ServiceResponse<BloodRequestDto>.SuccessResponse(responseDto, "تم إنشاء الطلب بنجاح");
    }

    public async Task<ServiceResponse<BloodRequestDto>> UpdateStatusAsync(int id, RequestStatus status, string? notes = null)
    {
        var request = await _unitOfWork.BloodRequests.GetByIdAsync(id);
        if (request == null)
            return ServiceResponse<BloodRequestDto>.FailureResponse("الطلب غير موجود");

        request.Status = status;
        if (!string.IsNullOrWhiteSpace(notes))
            request.Notes = notes;
        request.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.BloodRequests.UpdateAsync(request);
        await _unitOfWork.SaveChangesAsync();

        // إشعار الموظفين بتغيُّر حالة الطلب
        await _notificationService.CreateForRoleAsync(
            role: "Staff",
            title: "تغيُّر حالة طلب دم",
            message: $"تم تحديث حالة طلب الدم #{id} إلى: {status}",
            type: NotificationType.BloodRequestStatusChanged,
            relatedEntityType: "BloodRequest",
            relatedEntityId: id);

        var dto = _mapper.Map<BloodRequestDto>(request);
        return ServiceResponse<BloodRequestDto>.SuccessResponse(dto, "تم تحديث حالة الطلب بنجاح");
    }

    public async Task<ServiceResponse<bool>> CancelAsync(int id, string reason)
    {
        var request = await _unitOfWork.BloodRequests.GetByIdAsync(id);
        if (request == null)
            return ServiceResponse<bool>.FailureResponse("الطلب غير موجود");

        request.Status = RequestStatus.Cancelled;
        request.Notes = reason;
        request.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.BloodRequests.UpdateAsync(request);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResponse<bool>.SuccessResponse(true, "تم إلغاء الطلب بنجاح");
    }

    public async Task<ServiceResponse<IEnumerable<BloodRequestDto>>> GetPendingRequestsAsync()
    {
        var requests = await _unitOfWork.BloodRequests.GetPendingRequestsAsync();
        var dtos = _mapper.Map<IEnumerable<BloodRequestDto>>(requests);
        return ServiceResponse<IEnumerable<BloodRequestDto>>.SuccessResponse(dtos);
    }

    public async Task<ServiceResponse<IEnumerable<BloodRequestDto>>> GetUrgentRequestsAsync()
    {
        var requests = await _unitOfWork.BloodRequests.GetUrgentRequestsAsync();
        var dtos = _mapper.Map<IEnumerable<BloodRequestDto>>(requests);
        return ServiceResponse<IEnumerable<BloodRequestDto>>.SuccessResponse(dtos);
    }

    public async Task<ServiceResponse<BloodRequestDto>> FulfillRequestAsync(int requestId, FulfillBloodRequestDto dto, string fulfilledByUserId)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // جلب الطلب
            var request = await _unitOfWork.BloodRequests.GetByIdAsync(requestId);
            if (request == null)
                throw new NotFoundException("BloodRequest", requestId);

            // جلب التبرع
            var donation = await _unitOfWork.Donations.GetByIdAsync(dto.DonationId);
            if (donation == null)
                throw new NotFoundException("Donation", dto.DonationId);

            // Validation
            if (request.BloodTypeID != donation.BloodTypeID)
                throw new BusinessException("فصيلة الدم للتبرع لا تطابق الطلب");

            if (donation.TestResult != TestResult.Accepted)
                throw new BusinessException("التبرع لم يجتز الفحوصات");

            if (dto.QuantityToFulfill > request.QuantityNeeded - request.QuantityFulfilled)
                throw new BusinessException("الكمية أكبر من الحاجة المتبقية");

            // تحديث الطلب
            request.QuantityFulfilled += dto.QuantityToFulfill;
            if (request.QuantityFulfilled >= request.QuantityNeeded)
                request.Status = RequestStatus.Fulfilled;
            else
                request.Status = RequestStatus.PartiallyFulfilled;

            request.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.BloodRequests.UpdateAsync(request);

            // تحديث المخزون
            await _unitOfWork.BloodInventories.UpdateQuantityAsync(
                request.BloodTypeID,
                -dto.QuantityToFulfill
            );

            // إضافة سجل الصرف
            var disbursement = new BloodDisbursement
            {
                RequestID = requestId,
                DonationID = dto.DonationId,
                QuantityUsed = dto.QuantityToFulfill,
                DisbursementDate = DateTime.UtcNow
            };
            await _unitOfWork.BloodDisbursements.AddAsync(disbursement);

            await _unitOfWork.CommitTransactionAsync();

            var responseDto = _mapper.Map<BloodRequestDto>(request);
            return ServiceResponse<BloodRequestDto>.SuccessResponse(responseDto, "تم تنفيذ الطلب بنجاح");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return ServiceResponse<BloodRequestDto>.FailureResponse(ex.Message);
        }
    }

    public async Task<ServiceResponse<BloodRequestDto>> GetWithDetailsAsync(int id)
    {
        var request = await _unitOfWork.BloodRequests.GetWithDetailsAsync(id);
        
        if (request == null)
            return ServiceResponse<BloodRequestDto>.FailureResponse("الطلب غير موجود");

        var dto = _mapper.Map<BloodRequestDto>(request);
        return ServiceResponse<BloodRequestDto>.SuccessResponse(dto);
    }
}
