using BloodConnectAPI.DataAccess.Data;
using BloodConnectAPI.DataAccess.Repositories.Interfaces;
using BloodConnectAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BloodConnectAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// Repository Implementation لمخزون الدم
/// </summary>
public class BloodInventoryRepository : GenericRepository<BloodInventory>, IBloodInventoryRepository
{
    public BloodInventoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<BloodInventory?> GetByBloodTypeAsync(int bloodTypeId)
    {
        return await _dbSet
            .Include(bi => bi.BloodType)
            .FirstOrDefaultAsync(bi => bi.BloodTypeID == bloodTypeId);
    }

    public async Task<IEnumerable<BloodInventory>> GetLowStockAsync(int threshold = 5)
    {
        return await _dbSet
            .Where(bi => bi.QuantityAvailable < threshold)
            .Include(bi => bi.BloodType)
            .ToListAsync();
    }

    public async Task UpdateQuantityAsync(int bloodTypeId, int quantityChange)
    {
        var inventory = await GetByBloodTypeAsync(bloodTypeId);

        if (inventory != null)
        {
            inventory.QuantityAvailable += quantityChange;
            inventory.LastUpdated = DateTime.UtcNow;
            await UpdateAsync(inventory);
        }
    }

    public async Task<IEnumerable<BloodInventory>> GetAllWithBloodTypesAsync()
    {
        return await _dbSet
            .Include(bi => bi.BloodType)
            .ToListAsync();
    }
}
