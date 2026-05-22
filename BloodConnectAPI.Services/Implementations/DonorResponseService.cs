using AutoMapper;
using BloodConnectAPI.DataAccess.Repositories.Interfaces;
using BloodConnectAPI.Models;
using BloodConnectAPI.Models.DTOs;
using BloodConnectAPI.Models.Enums;
using BloodConnectAPI.Service.Common;
using BloodConnectAPI.Service.Interfaces;

namespace BloodConnectAPI.Service.Implementations;

/// <summary>
/// خدمة إدارة استجابات المتبرعين لطلبات الدم
/// </summary>
public class DonorResponseService : IDonorResponseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;

    public DonorResponseService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService)
    {
        _unitOfWork          = unitOfWork;
        _mapper              = mapper;
        _notificationService = notificationService;
    }

    /// <summary>
    /// تسجيل اهتمام متبرع بطلب دم
    /// </summary>
    public async Task<ServiceResponse<DonorResponseDto>> RespondToRequestAsync(CreateDonorResponseDto dto)
    {
        // التحقق من وجود الطلب
        var request = await _unitOfWork.BloodRequests.GetWithDetailsAsync(dto.RequestID);
        if (request == null)
            return ServiceResponse<DonorResponseDto>.FailureResponse("طلب الدم غير موجود");

        // التحقق من حالة الطلب (يجب أن يكون قابلاً للاستجابة)
        if (request.Status != RequestStatus.Pending && request.Status != RequestStatus.PartiallyFulfilled)
            return ServiceResponse<DonorResponseDto>.FailureResponse(
                $"لا يمكن الاستجابة لهذا الطلب — حالته الحالية: {request.Status}");

        // التحقق من وجود المتبرع
        var donor = await _unitOfWork.Donors.GetByIdAsync(dto.DonorID);
        if (donor == null)
            return ServiceResponse<DonorResponseDto>.FailureResponse("المتبرع غير موجود");

        // التحقق من أن المتبرع نشط
        if (!donor.IsActive)
            return ServiceResponse<DonorResponseDto>.FailureResponse("المتبرع غير نشط ولا يمكنه الاستجابة");

        // التحقق من توافق فصيلة الدم
        if (donor.BloodTypeID != request.BloodTypeID)
            return ServiceResponse<DonorResponseDto>.FailureResponse(
                "فصيلة دم المتبرع لا تتوافق مع فصيلة الدم المطلوبة في هذا الطلب");

        // التحقق من عدم وجود استجابة نشطة مسبقاً
        var hasActive = await _unitOfWork.DonorResponseRepository.HasActiveResponseAsync(dto.DonorID, dto.RequestID);
        if (hasActive)
            return ServiceResponse<DonorResponseDto>.FailureResponse(
                "لديك استجابة نشطة مسبقاً لهذا الطلب. لا يمكن تسجيل استجابة جديدة قبل إغلاق السابقة");

        // إنشاء الاستجابة
        var response = new DonorRequestResponse
        {
            DonorID = dto.DonorID,
            RequestID = dto.RequestID,
            Status = ResponseStatus.Interested,
            Notes = dto.Notes,
            ResponseDate = DateTime.UtcNow
        };

        await _unitOfWork.DonorResponseRepository.AddAsync(response);
        await _unitOfWork.SaveChangesAsync();

        // إشعار الموظفين باستجابة متبرع جديد
        await _notificationService.CreateForRoleAsync(
            role: "Staff",
            title: "استجابة متبرع جديدة",
            message: $"أبدى المتبرع '{donor.FullName}' اهتمامه بطلب الدم #{dto.RequestID}",
            type: NotificationType.NewDonorResponse,
            relatedEntityType: "DonorResponse",
            relatedEntityId: response.ResponseID);

        // جلب الاستجابة مع تفاصيلها الكاملة للـ DTO
        var fullResponse = await _unitOfWork.DonorResponseRepository.GetWithDetailsAsync(response.ResponseID);
        var responseDto = _mapper.Map<DonorResponseDto>(fullResponse);

        return ServiceResponse<DonorResponseDto>.SuccessResponse(responseDto, "تم تسجيل استجابتك بنجاح. سيتواصل معك الفريق قريباً");
    }

    /// <summary>
    /// تحديث حالة الاستجابة من قِبل الموظف
    /// </summary>
    public async Task<ServiceResponse<DonorResponseDto>> UpdateResponseStatusAsync(int responseId, UpdateResponseStatusDto dto)
    {
        var response = await _unitOfWork.DonorResponseRepository.GetWithDetailsAsync(responseId);
        if (response == null)
            return ServiceResponse<DonorResponseDto>.FailureResponse("الاستجابة غير موجودة");

        // التحقق من صحة الانتقال بين الحالات
        var validationError = ValidateStatusTransition(response.Status, dto.Status);
        if (validationError != null)
            return ServiceResponse<DonorResponseDto>.FailureResponse(validationError);

        // تحديث التفاصيل حسب الحالة الجديدة
        await ApplyStatusUpdateAsync(response, dto);

        await _unitOfWork.DonorResponseRepository.UpdateAsync(response);
        await _unitOfWork.SaveChangesAsync();

        // إشعار المتبرع (إن كان لديه حساب مستخدم) بتحديث حالة استجابته
        var donor = await _unitOfWork.Donors.GetByIdAsync(response.DonorID);
        if (donor?.UserId != null)
        {
            await _notificationService.CreateAsync(
                recipientUserId: donor.UserId,
                title: "تحديث حالة استجابتك",
                message: $"تم تحديث حالة استجابتك لطلب الدم #{response.RequestID} إلى: {dto.Status}",
                type: NotificationType.ResponseStatusUpdated,
                relatedEntityType: "DonorResponse",
                relatedEntityId: responseId);
        }

        // إعادة جلب البيانات المحدثة
        var updated = await _unitOfWork.DonorResponseRepository.GetWithDetailsAsync(responseId);
        var responseDto = _mapper.Map<DonorResponseDto>(updated);

        return ServiceResponse<DonorResponseDto>.SuccessResponse(responseDto, "تم تحديث حالة الاستجابة بنجاح");
    }

    /// <summary>
    /// جلب جميع الاستجابات لطلب دم معين
    /// </summary>
    public async Task<ServiceResponse<IEnumerable<DonorResponseDto>>> GetResponsesByRequestAsync(int requestId)
    {
        var request = await _unitOfWork.BloodRequests.GetByIdAsync(requestId);
        if (request == null)
            return ServiceResponse<IEnumerable<DonorResponseDto>>.FailureResponse("طلب الدم غير موجود");

        var responses = await _unitOfWork.DonorResponseRepository.GetByRequestIdAsync(requestId);
        var dtos = _mapper.Map<IEnumerable<DonorResponseDto>>(responses);

        return ServiceResponse<IEnumerable<DonorResponseDto>>.SuccessResponse(dtos);
    }

    /// <summary>
    /// جلب جميع استجابات متبرع معين
    /// </summary>
    public async Task<ServiceResponse<IEnumerable<DonorResponseDto>>> GetResponsesByDonorAsync(int donorId)
    {
        var donor = await _unitOfWork.Donors.GetByIdAsync(donorId);
        if (donor == null)
            return ServiceResponse<IEnumerable<DonorResponseDto>>.FailureResponse("المتبرع غير موجود");

        var responses = await _unitOfWork.DonorResponseRepository.GetByDonorIdAsync(donorId);
        var dtos = _mapper.Map<IEnumerable<DonorResponseDto>>(responses);

        return ServiceResponse<IEnumerable<DonorResponseDto>>.SuccessResponse(dtos);
    }

    /// <summary>
    /// جلب تفاصيل استجابة واحدة
    /// </summary>
    public async Task<ServiceResponse<DonorResponseDto>> GetResponseByIdAsync(int responseId)
    {
        var response = await _unitOfWork.DonorResponseRepository.GetWithDetailsAsync(responseId);
        if (response == null)
            return ServiceResponse<DonorResponseDto>.FailureResponse("الاستجابة غير موجودة");

        var dto = _mapper.Map<DonorResponseDto>(response);
        return ServiceResponse<DonorResponseDto>.SuccessResponse(dto);
    }

    /// <summary>
    /// إلغاء استجابة مع ذكر السبب
    /// </summary>
    public async Task<ServiceResponse<bool>> CancelResponseAsync(int responseId, string reason)
    {
        var response = await _unitOfWork.DonorResponseRepository.GetByIdAsync(responseId);
        if (response == null)
            return ServiceResponse<bool>.FailureResponse("الاستجابة غير موجودة");

        // لا يمكن إلغاء استجابة منتهية بالفعل
        if (response.Status == ResponseStatus.Donated ||
            response.Status == ResponseStatus.Cancelled)
            return ServiceResponse<bool>.FailureResponse(
                $"لا يمكن إلغاء الاستجابة — حالتها الحالية: {response.Status}");

        response.Status = ResponseStatus.Cancelled;
        response.RejectionReason = reason;

        await _unitOfWork.DonorResponseRepository.UpdateAsync(response);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResponse<bool>.SuccessResponse(true, "تم إلغاء الاستجابة بنجاح");
    }

    #region Private Helpers

    /// <summary>
    /// التحقق من صحة الانتقال بين حالات الاستجابة
    /// </summary>
    private static string? ValidateStatusTransition(ResponseStatus current, ResponseStatus next)
    {
        // لا يمكن تغيير استجابة مكتملة أو ملغاة
        if (current == ResponseStatus.Donated)
            return "لا يمكن تغيير حالة استجابة تم التبرع بها مسبقاً";

        if (current == ResponseStatus.Cancelled)
            return "لا يمكن تغيير حالة استجابة ملغاة";

        // الانتقالات المسموحة
        var allowedTransitions = new Dictionary<ResponseStatus, HashSet<ResponseStatus>>
        {
            [ResponseStatus.Interested] = new() { ResponseStatus.Confirmed, ResponseStatus.Rejected, ResponseStatus.Cancelled },
            [ResponseStatus.Confirmed]  = new() { ResponseStatus.Donated, ResponseStatus.NoShow, ResponseStatus.Cancelled },
            [ResponseStatus.Rejected]   = new() { ResponseStatus.Cancelled },
            [ResponseStatus.NoShow]     = new() { ResponseStatus.Confirmed, ResponseStatus.Cancelled },
        };

        if (allowedTransitions.TryGetValue(current, out var allowed) && !allowed.Contains(next))
            return $"الانتقال من '{current}' إلى '{next}' غير مسموح";

        return null; // صحيح
    }

    /// <summary>
    /// تطبيق تفاصيل التحديث حسب الحالة الجديدة
    /// </summary>
    private async Task ApplyStatusUpdateAsync(DonorRequestResponse response, UpdateResponseStatusDto dto)
    {
        response.Status = dto.Status;
        if (!string.IsNullOrWhiteSpace(dto.Notes))
            response.Notes = dto.Notes;

        switch (dto.Status)
        {
            case ResponseStatus.Confirmed:
                response.ConfirmedAt = DateTime.UtcNow;
                break;

            case ResponseStatus.Donated:
                // التحقق من وجود معرف التبرع
                if (dto.DonationID == null)
                    throw new InvalidOperationException("يجب تحديد معرف التبرع عند تسجيل حالة 'تم التبرع'");

                var donation = await _unitOfWork.Donations.GetByIdAsync(dto.DonationID.Value);
                if (donation == null)
                    throw new InvalidOperationException($"التبرع برقم {dto.DonationID} غير موجود");

                response.DonationID = dto.DonationID;

                // تحديث حالة الطلب إذا اكتملت الكمية
                await UpdateRequestStatusAfterDonationAsync(response.RequestID);
                break;

            case ResponseStatus.Rejected:
                response.RejectionReason = dto.Notes;
                break;
        }
    }

    /// <summary>
    /// تحديث حالة الطلب بعد تسجيل تبرع فعلي
    /// </summary>
    private async Task UpdateRequestStatusAfterDonationAsync(int requestId)
    {
        var request = await _unitOfWork.BloodRequests.GetByIdAsync(requestId);
        if (request == null) return;

        // عدد الاستجابات المكتملة (Donated) لهذا الطلب
        var donatedResponses = await _unitOfWork.DonorResponseRepository
            .GetByRequestIdAsync(requestId);

        var donatedCount = donatedResponses.Count(r => r.Status == ResponseStatus.Donated);

        // تحديث الحالة
        if (donatedCount >= request.QuantityNeeded)
        {
            request.Status = RequestStatus.Fulfilled;
        }
        else if (donatedCount > 0)
        {
            request.Status = RequestStatus.PartiallyFulfilled;
        }

        await _unitOfWork.BloodRequests.UpdateAsync(request);
        await _unitOfWork.SaveChangesAsync();
    }

    #endregion
}
