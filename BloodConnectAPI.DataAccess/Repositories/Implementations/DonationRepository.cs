using BloodConnectAPI.DataAccess.Data;
using BloodConnectAPI.DataAccess.Repositories.Interfaces;
using BloodConnectAPI.Models;
using BloodConnectAPI.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BloodConnectAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// Repository Implementation للتبرعات
/// </summary>
public class DonationRepository : GenericRepository<Donation>, IDonationRepository
{
    public DonationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Donation>> GetByDonorIdAsync(int donorId)
    {
        return await _dbSet
            .Where(d => d.DonorID == donorId)
            .Include(d => d.BloodType)
            .OrderByDescending(d => d.DonationDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Donation>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(d => d.DonationDate >= startDate && d.DonationDate <= endDate)
            .Include(d => d.Donor)
            .Include(d => d.BloodType)
            .ToListAsync();
    }

    public async Task<IEnumerable<Donation>> GetApprovedDonationsAsync()
    {
        return await _dbSet
            .Where(d => d.TestResult == TestResult.Accepted)
            .Include(d => d.Donor)
            .Include(d => d.BloodType)
            .ToListAsync();
    }

    public async Task<IEnumerable<Donation>> GetByBloodTypeAsync(int bloodTypeId)
    {
        return await _dbSet
            .Where(d => d.BloodTypeID == bloodTypeId)
            .Include(d => d.Donor)
            .ToListAsync();
    }

    public async Task<IEnumerable<Donation>> GetByTestResultAsync(TestResult testResult)
    {
        return await _dbSet
            .Where(d => d.TestResult == testResult)
            .Include(d => d.Donor)
            .Include(d => d.BloodType)
            .ToListAsync();
    }

    public async Task<Donation?> GetLastDonationByDonorAsync(int donorId)
    {
        return await _dbSet
            .Where(d => d.DonorID == donorId)
            .OrderByDescending(d => d.DonationDate)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Donation>> GetPendingTestsAsync()
    {
        return await _dbSet
            .Include(d => d.Donor)
            .Include(d => d.BloodType)
            .Where(d => d.TestResult == BloodConnectAPI.Models.Enums.TestResult.Pending)
            .ToListAsync();
    }

    public async Task<IEnumerable<Donation>> GetAcceptedNotInventoriedAsync()
    {
        return await _dbSet
            .Include(d => d.Donor)
            .Include(d => d.BloodType)
            .Where(d => d.TestResult == BloodConnectAPI.Models.Enums.TestResult.Accepted && !d.IsAddedToInventory)
            .ToListAsync();
    }
}
