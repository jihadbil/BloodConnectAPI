using BloodConnectAPI.DataAccess.Data;
using BloodConnectAPI.DataAccess.Repositories.Interfaces;
using BloodConnectAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BloodConnectAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// Repository Implementation لفصائل الدم
/// </summary>
public class BloodTypeRepository : GenericRepository<BloodType>, IBloodTypeRepository
{
    public BloodTypeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<BloodType?> GetByNameAsync(string typeName)
    {
        return await _dbSet
            .FirstOrDefaultAsync(bt => bt.TypeName == typeName);
    }

    public async Task<BloodType?> GetWithInventoryAsync(int bloodTypeId)
    {
        return await _dbSet
            .Include(bt => bt.BloodInventories)
            .FirstOrDefaultAsync(bt => bt.BloodTypeID == bloodTypeId);
    }

    public async Task<IEnumerable<(BloodType BloodType, int QuantityAvailable)>> GetAllWithInventoryAsync()
    {
        return await _dbSet
            .Include(bt => bt.BloodInventories)
            .Select(bt => new ValueTuple<BloodType, int>(
                bt,
                bt.BloodInventories.FirstOrDefault() != null ? bt.BloodInventories.FirstOrDefault()!.QuantityAvailable : 0
            ))
            .ToListAsync();
    }
}
