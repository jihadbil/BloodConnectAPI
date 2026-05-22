using BloodConnectAPI.DataAccess.Data;
using BloodConnectAPI.DataAccess.Repositories.Interfaces;
using BloodConnectAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BloodConnectAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// Repository Implementation لعمليات صرف الدم
/// </summary>
public class BloodDisbursementRepository : GenericRepository<BloodDisbursement>, IBloodDisbursementRepository
{
    public BloodDisbursementRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<BloodDisbursement>> GetByRequestIdAsync(int requestId)
    {
        return await _dbSet
            .Where(bd => bd.RequestID == requestId)
            .Include(bd => bd.Donation)
                .ThenInclude(d => d.BloodType)
            .OrderByDescending(bd => bd.DisbursementDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<BloodDisbursement>> GetByDonationIdAsync(int donationId)
    {
        return await _dbSet
            .Where(bd => bd.DonationID == donationId)
            .Include(bd => bd.BloodRequest)
                .ThenInclude(r => r.Patient)
            .ToListAsync();
    }

    public async Task<IEnumerable<BloodDisbursement>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(bd => bd.DisbursementDate >= startDate && bd.DisbursementDate <= endDate)
            .Include(bd => bd.BloodRequest)
                .ThenInclude(r => r.Patient)
            .Include(bd => bd.Donation)
                .ThenInclude(d => d.BloodType)
            .ToListAsync();
    }

    public async Task<BloodDisbursement?> GetWithDetailsAsync(int disbursementId)
    {
        return await _dbSet
            .Include(bd => bd.BloodRequest)
                .ThenInclude(r => r.Patient)
            .Include(bd => bd.BloodRequest.BloodType)
            .Include(bd => bd.Donation)
                .ThenInclude(d => d.Donor)
            .FirstOrDefaultAsync(bd => bd.DisbursementID == disbursementId);
    }

    public async Task<int> GetTotalQuantityDisbursedAsync(int bloodTypeId, DateTime? from = null)
    {
        var query = _dbSet
            .Include(bd => bd.Donation)
            .Where(bd => bd.Donation.BloodTypeID == bloodTypeId);

        if (from.HasValue)
        {
            query = query.Where(bd => bd.DisbursementDate >= from.Value);
        }

        return await query.SumAsync(bd => bd.QuantityUsed);
    }

    public async Task<IEnumerable<BloodDisbursement>> GetRecentDisbursementsAsync(int count = 10)
    {
        return await _dbSet
            .Include(bd => bd.BloodRequest)
                .ThenInclude(r => r.Patient)
            .Include(bd => bd.Donation)
                .ThenInclude(d => d.BloodType)
            .OrderByDescending(bd => bd.DisbursementDate)
            .Take(count)
            .ToListAsync();
    }
}
