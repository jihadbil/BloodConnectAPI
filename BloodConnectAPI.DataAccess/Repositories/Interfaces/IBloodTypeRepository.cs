using BloodConnectAPI.Models;

namespace BloodConnectAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// Repository لفصائل الدم
/// </summary>
public interface IBloodTypeRepository : IGenericRepository<BloodType>
{
    /// <summary>
    /// جلب فصيلة دم حسب الاسم (مثل A+)
    /// </summary>
    Task<BloodType?> GetByNameAsync(string typeName);

    /// <summary>
    /// جلب فصيلة دم مع المخزون
    /// </summary>
    Task<BloodType?> GetWithInventoryAsync(int bloodTypeId);

    /// <summary>
    /// جلب فصائل الدم مع كميات المخزون
    /// </summary>
    Task<IEnumerable<(BloodType BloodType, int QuantityAvailable)>> GetAllWithInventoryAsync();
}
