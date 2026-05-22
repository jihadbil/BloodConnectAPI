using BloodConnectAPI.Models;

namespace BloodConnectAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// Repository لعمليات صرف الدم
/// </summary>
public interface IBloodDisbursementRepository : IGenericRepository<BloodDisbursement>
{
    /// <summary>
    /// جلب عمليات الصرف لطلب معين
    /// </summary>
    Task<IEnumerable<BloodDisbursement>> GetByRequestIdAsync(int requestId);

    /// <summary>
    /// جلب عمليات الصرف من تبرع معين
    /// </summary>
    Task<IEnumerable<BloodDisbursement>> GetByDonationIdAsync(int donationId);

    /// <summary>
    /// جلب عمليات الصرف حسب نطاق زمني
    /// </summary>
    Task<IEnumerable<BloodDisbursement>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// جلب عملية صرف مع تفاصيلها (Request + Donation)
    /// </summary>
    Task<BloodDisbursement?> GetWithDetailsAsync(int disbursementId);

    /// <summary>
    /// حساب إجمالي الكمية المصروفة لفصيلة دم معينة
    /// </summary>
    Task<int> GetTotalQuantityDisbursedAsync(int bloodTypeId, DateTime? from = null);

    /// <summary>
    /// جلب آخر عمليات الصرف
    /// </summary>
    Task<IEnumerable<BloodDisbursement>> GetRecentDisbursementsAsync(int count = 10);
}
