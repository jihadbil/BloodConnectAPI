using BloodConnectAPI.Models;

namespace BloodConnectAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// Repository لمخزون الدم
/// </summary>
public interface IBloodInventoryRepository : IGenericRepository<BloodInventory>
{
    /// <summary>
    /// جلب مخزون فصيلة دم معينة
    /// </summary>
    Task<BloodInventory?> GetByBloodTypeAsync(int bloodTypeId);

    /// <summary>
    /// جلب الفصائل ذات المخزون المنخفض (أقل من حد معين)
    /// </summary>
    Task<IEnumerable<BloodInventory>> GetLowStockAsync(int threshold = 5);

    /// <summary>
    /// تحديث كمية المخزون
    /// </summary>
    Task UpdateQuantityAsync(int bloodTypeId, int quantityChange);

    /// <summary>
    /// جلب جميع المخزون مع تفاصيل فصائل الدم
    /// </summary>
    Task<IEnumerable<BloodInventory>> GetAllWithBloodTypesAsync();
}
