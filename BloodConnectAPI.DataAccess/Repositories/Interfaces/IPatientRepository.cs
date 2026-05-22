using BloodConnectAPI.Models;

namespace BloodConnectAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// Repository للمرضى
/// </summary>
public interface IPatientRepository : IGenericRepository<Patient>
{
    /// <summary>
    /// جلب مريض حسب الرقم الوطني
    /// </summary>
    Task<Patient?> GetByNationalIdAsync(string nationalId);

    /// <summary>
    /// جلب مريض مع طلباته
    /// </summary>
    Task<Patient?> GetWithRequestsAsync(int patientId);

    /// <summary>
    /// جلب المرضى الجدد (آخر شهر)
    /// </summary>
    Task<IEnumerable<Patient>> GetRecentPatientsAsync(int days = 30);

    /// <summary>
    /// جلب مرضى حسب فصيلة الدم
    /// </summary>
    Task<IEnumerable<Patient>> GetPatientsByBloodTypeAsync(int bloodTypeId);
}
