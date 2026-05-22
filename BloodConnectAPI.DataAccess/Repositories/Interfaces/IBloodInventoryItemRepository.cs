using BloodConnectAPI.Models;
using BloodConnectAPI.DataAccess.Repositories.Interfaces;

namespace BloodConnectAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// Repository Interface لإدارة وحدات المخزون (BloodInventoryItem)
/// </summary>
public interface IBloodInventoryItemRepository : IGenericRepository<BloodInventoryItem>
{
    /// <summary>
    /// الحصول على الوحدات المتاحة (غير المستخدمة) لفصيلة دم معينة
    /// مرتبة حسب تاريخ انتهاء الصلاحية (FIFO)
    /// </summary>
    Task<IEnumerable<BloodInventoryItem>> GetAvailableItemsByBloodTypeAsync(int bloodTypeId);

    /// <summary>
    /// الحصول على الوحدات منتهية الصلاحية ولم تستخدم
    /// </summary>
    Task<IEnumerable<BloodInventoryItem>> GetExpiredItemsAsync();

    /// <summary>
    /// الحصول على الوحدات القريبة من انتهاء الصلاحية
    /// </summary>
    /// <param name="daysThreshold">عدد الأيام (مثلاً 7 أيام)</param>
    Task<IEnumerable<BloodInventoryItem>> GetExpiringItemsAsync(int daysThreshold = 7);

    /// <summary>
    /// الحصول على وحدات مخزون معينة
    /// </summary>
    Task<IEnumerable<BloodInventoryItem>> GetByInventoryIdAsync(int inventoryId);
}
