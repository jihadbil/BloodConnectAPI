using BloodConnectAPI.Models;

namespace BloodConnectAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// Repository للمتبرعين
/// </summary>
public interface IDonorRepository : IGenericRepository<Donor>
{
    /// <summary>
    /// جلب متبرع حسب الرقم الوطني
    /// </summary>
    Task<Donor?> GetByNationalIdAsync(string nationalId);

    /// <summary>
    /// جلب متبرع مع تبرعاته
    /// </summary>
    Task<Donor?> GetWithDonationsAsync(int donorId);

    /// <summary>
    /// جلب المتبرعين النشطين
    /// </summary>
    Task<IEnumerable<Donor>> GetActiveDonorsAsync();

    /// <summary>
    /// جلب متبرعين حسب فصيلة الدم
    /// </summary>
    Task<IEnumerable<Donor>> GetDonorsByBloodTypeAsync(int bloodTypeId);

    /// <summary>
    /// جلب المتبرعين المؤهلين للتبرع (لم يتبرعوا منذ 3 أشهر)
    /// </summary>
    Task<IEnumerable<Donor>> GetEligibleDonorsAsync();

    /// <summary>
    /// جلب متبرع مع معلومات المستخدم المرتبط
    /// </summary>
    Task<Donor?> GetWithUserAsync(int donorId);

    /// <summary>
    /// جلب المتبرعين في انتظار الموافقة
    /// </summary>
    Task<IEnumerable<Donor>> GetPendingApprovalAsync();

    /// <summary>
    /// جلب المتبرعين حسب حالة الموافقة
    /// </summary>
    Task<IEnumerable<Donor>> GetByApprovalStatusAsync(BloodConnectAPI.Models.Enums.DonorApprovalStatus status);

    /// <summary>
    /// جلب المتبرع مع الوثائق الطبية المرفوعة
    /// </summary>
    Task<Donor?> GetWithMedicalDocumentsAsync(int donorId);
}
