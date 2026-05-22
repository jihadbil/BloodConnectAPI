using BloodConnectAPI.Models;
using BloodConnectAPI.Models.Enums;

namespace BloodConnectAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// Repository لإدارة استجابات المتبرعين لطلبات الدم
/// </summary>
public interface IDonorRequestResponseRepository : IGenericRepository<DonorRequestResponse>
{
    /// <summary>
    /// جلب جميع الاستجابات لطلب دم معين مع تفاصيل المتبرع
    /// </summary>
    Task<IEnumerable<DonorRequestResponse>> GetByRequestIdAsync(int requestId);

    /// <summary>
    /// جلب جميع استجابات متبرع معين مع تفاصيل الطلب
    /// </summary>
    Task<IEnumerable<DonorRequestResponse>> GetByDonorIdAsync(int donorId);

    /// <summary>
    /// جلب الاستجابة النشطة لمتبرع على طلب محدد (Interested أو Confirmed)
    /// </summary>
    Task<DonorRequestResponse?> GetActiveResponseAsync(int donorId, int requestId);

    /// <summary>
    /// جلب الاستجابات حسب الحالة
    /// </summary>
    Task<IEnumerable<DonorRequestResponse>> GetByStatusAsync(ResponseStatus status);

    /// <summary>
    /// جلب استجابة واحدة مع تفاصيل المتبرع والطلب والمريض وفصيلة الدم
    /// </summary>
    Task<DonorRequestResponse?> GetWithDetailsAsync(int responseId);

    /// <summary>
    /// التحقق من وجود استجابة نشطة سابقة من نفس المتبرع لنفس الطلب
    /// </summary>
    Task<bool> HasActiveResponseAsync(int donorId, int requestId);
}
