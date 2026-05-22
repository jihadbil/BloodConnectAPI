using BloodConnectAPI.DataAccess.Data;
using BloodConnectAPI.DataAccess.Repositories.Interfaces;
using BloodConnectAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BloodConnectAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// Repository Implementation لإدارة وحدات المخزون
/// </summary>
public class BloodInventoryItemRepository : GenericRepository<BloodInventoryItem>, IBloodInventoryItemRepository
{
    public BloodInventoryItemRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// الحصول على الوحدات المتاحة (غير المستخدمة) لفصيلة دم معينة
    /// مرتبة حسب تاريخ انتهاء الصلاحية (FIFO - First In, First Out)
    /// </summary>
    public async Task<IEnumerable<BloodInventoryItem>> GetAvailableItemsByBloodTypeAsync(int bloodTypeId)
    {
        var now = DateTime.UtcNow;

        return await _dbSet
            .Include(bi => bi.Inventory)
            .Include(bi => bi.Donation)
            .Where(bi => bi.Inventory.BloodTypeID == bloodTypeId &&
                        !bi.IsUsed &&
                        bi.Quantity > 0 &&
                        bi.ExpiryDate > now) // فقط الوحدات غير المنتهية
            .OrderBy(bi => bi.ExpiryDate) // FIFO - الأقدم أولاً
            .ToListAsync();
    }

    /// <summary>
    /// الحصول على الوحدات منتهية الصلاحية ولم تستخدم
    /// </summary>
    public async Task<IEnumerable<BloodInventoryItem>> GetExpiredItemsAsync()
    {
        var now = DateTime.UtcNow;

        return await _dbSet
            .Include(bi => bi.Inventory)
                .ThenInclude(i => i.BloodType)
            .Where(bi => !bi.IsUsed && bi.ExpiryDate <= now)
            .ToListAsync();
    }

    /// <summary>
    /// الحصول على الوحدات القريبة من انتهاء الصلاحية
    /// </summary>
    public async Task<IEnumerable<BloodInventoryItem>> GetExpiringItemsAsync(int daysThreshold = 7)
    {
        var now = DateTime.UtcNow;
        var thresholdDate = now.AddDays(daysThreshold);

        return await _dbSet
            .Include(bi => bi.Inventory)
                .ThenInclude(i => i.BloodType)
            .Include(bi => bi.Donation)
            .Where(bi => !bi.IsUsed &&
                        bi.ExpiryDate > now &&
                        bi.ExpiryDate <= thresholdDate)
            .OrderBy(bi => bi.ExpiryDate)
            .ToListAsync();
    }

    /// <summary>
    /// الحصول على وحدات مخزون معينة
    /// </summary>
    public async Task<IEnumerable<BloodInventoryItem>> GetByInventoryIdAsync(int inventoryId)
    {
        return await _dbSet
            .Include(bi => bi.Donation)
            .Where(bi => bi.InventoryID == inventoryId)
            .OrderByDescending(bi => bi.AddedAt)
            .ToListAsync();
    }
}
