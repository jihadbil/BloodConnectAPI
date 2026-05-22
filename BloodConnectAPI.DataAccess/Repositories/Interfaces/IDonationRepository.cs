using BloodConnectAPI.Models;
using BloodConnectAPI.Models.Enums;

namespace BloodConnectAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// Repository للتبرعات
/// </summary>
public interface IDonationRepository : IGenericRepository<Donation>
{
    /// <summary>
    /// جلب تبرعات متبرع معين
    /// </summary>
    Task<IEnumerable<Donation>> GetByDonorIdAsync(int donorId);

    /// <summary>
    /// جلب تبرعات حسب نطاق زمني
    /// </summary>
    Task<IEnumerable<Donation>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// جلب التبرعات المعتمدة (اجتازت الفحوصات)
    /// </summary>
    Task<IEnumerable<Donation>> GetApprovedDonationsAsync();

    /// <summary>
    /// جلب تبرعات حسب فصيلة الدم
    /// </summary>
    Task<IEnumerable<Donation>> GetByBloodTypeAsync(int bloodTypeId);

    /// <summary>
    /// جلب تبرعات حسب نتيجة الفحص
    /// </summary>
    Task<IEnumerable<Donation>> GetByTestResultAsync(TestResult testResult);

    /// <summary>
    /// جلب آخر تبرع لمتبرع
    /// </summary>
    Task<Donation?> GetLastDonationByDonorAsync(int donorId);

    /// <summary>
    /// جلب التبرعات في انتظار الفحص
    /// </summary>
    Task<IEnumerable<Donation>> GetPendingTestsAsync();

    /// <summary>
    /// جلب التبرعات المقبولة ولم تضف للمخزون
    /// </summary>
    Task<IEnumerable<Donation>> GetAcceptedNotInventoriedAsync();
}
