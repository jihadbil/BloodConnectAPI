using BloodConnectAPI.Models.DTOs;
using BloodConnectAPI.Models.Enums;
using BloodConnectAPI.Service.Common;

namespace BloodConnectAPI.Service.Interfaces;

/// <summary>
/// خدمة إدارة استجابات المتبرعين لطلبات الدم
/// </summary>
public interface IDonorResponseService
{
    /// <summary>
    /// تسجيل اهتمام متبرع بطلب دم (Interested)
    /// </summary>
    Task<ServiceResponse<DonorResponseDto>> RespondToRequestAsync(CreateDonorResponseDto dto);

    /// <summary>
    /// تحديث حالة الاستجابة (Confirm / Reject / Donate / NoShow / Cancel)
    /// </summary>
    Task<ServiceResponse<DonorResponseDto>> UpdateResponseStatusAsync(int responseId, UpdateResponseStatusDto dto);

    /// <summary>
    /// جلب جميع المستجيبين لطلب دم معين
    /// </summary>
    Task<ServiceResponse<IEnumerable<DonorResponseDto>>> GetResponsesByRequestAsync(int requestId);

    /// <summary>
    /// جلب جميع استجابات متبرع معين
    /// </summary>
    Task<ServiceResponse<IEnumerable<DonorResponseDto>>> GetResponsesByDonorAsync(int donorId);

    /// <summary>
    /// جلب تفاصيل استجابة واحدة
    /// </summary>
    Task<ServiceResponse<DonorResponseDto>> GetResponseByIdAsync(int responseId);

    /// <summary>
    /// إلغاء استجابة مع ذكر السبب
    /// </summary>
    Task<ServiceResponse<bool>> CancelResponseAsync(int responseId, string reason);
}
