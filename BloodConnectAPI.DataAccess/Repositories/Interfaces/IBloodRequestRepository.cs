using BloodConnectAPI.Models;
using BloodConnectAPI.Models.Enums;

namespace BloodConnectAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// Repository لطلبات الدم
/// </summary>
public interface IBloodRequestRepository : IGenericRepository<BloodRequest>
{
    /// <summary>
    /// جلب الطلبات المعلقة (Pending)
    /// </summary>
    Task<IEnumerable<BloodRequest>> GetPendingRequestsAsync();

    /// <summary>
    /// جلب الطلبات العاجلة
    /// </summary>
    Task<IEnumerable<BloodRequest>> GetUrgentRequestsAsync();

    /// <summary>
    /// جلب طلبات مريض معين
    /// </summary>
    Task<IEnumerable<BloodRequest>> GetRequestsByPatientAsync(int patientId);

    /// <summary>
    /// جلب طلبات حسب فصيلة الدم
    /// </summary>
    Task<IEnumerable<BloodRequest>> GetRequestsByBloodTypeAsync(int bloodTypeId);

    /// <summary>
    /// جلب طلب مع تفاصيله (Patient + BloodType)
    /// </summary>
    Task<BloodRequest?> GetWithDetailsAsync(int requestId);

    /// <summary>
    /// جلب طلبات حسب الحالة
    /// </summary>
    Task<IEnumerable<BloodRequest>> GetByStatusAsync(RequestStatus status);

    /// <summary>
    /// جلب طلبات حسب مستوى الاستعجال
    /// </summary>
    Task<IEnumerable<BloodRequest>> GetByUrgencyLevelAsync(UrgencyLevel urgencyLevel);

    /// <summary>
    /// جلب الطلبات التي لم تكتمل بعد
    /// </summary>
    Task<IEnumerable<BloodRequest>> GetPartiallyFulfilledAsync();

    /// <summary>
    /// تحديث الكمية المُلباة للطلب
    /// </summary>
    Task UpdateFulfillmentAsync(int requestId, int quantityToAdd);
}
