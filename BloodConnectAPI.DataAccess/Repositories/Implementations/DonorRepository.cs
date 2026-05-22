using BloodConnectAPI.DataAccess.Data;
using BloodConnectAPI.DataAccess.Repositories.Interfaces;
using BloodConnectAPI.Models;
using BloodConnectAPI.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BloodConnectAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// Repository Implementation للمتبرعين
/// </summary>
public class DonorRepository : GenericRepository<Donor>, IDonorRepository
{
    public DonorRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// جلب متبرع حسب ID مع معلومات المستخدم المرتبط
    /// </summary>
    public override async Task<Donor?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(d => d.User)
            .Include(d => d.BloodType)
            .FirstOrDefaultAsync(d => d.DonorID == id);
    }

    public async Task<Donor?> GetByNationalIdAsync(string nationalId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(d => d.NationalID == nationalId);
    }

    public async Task<Donor?> GetWithDonationsAsync(int donorId)
    {
        return await _dbSet
            .Include(d => d.Donations)
                .ThenInclude(dn => dn.BloodType)
            .FirstOrDefaultAsync(d => d.DonorID == donorId);
    }

    public async Task<IEnumerable<Donor>> GetActiveDonorsAsync()
    {
        return await _dbSet
            .Where(d => d.IsActive)
            .Include(d => d.BloodType)
            .ToListAsync();
    }

    public async Task<IEnumerable<Donor>> GetDonorsByBloodTypeAsync(int bloodTypeId)
    {
        return await _dbSet
            .Where(d => d.BloodTypeID == bloodTypeId && d.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<Donor>> GetEligibleDonorsAsync()
    {
        var now = DateTime.UtcNow;

        return await _dbSet
            .Where(d => d.IsActive &&
                       (d.LastDonationDate == null ||
                        (d.Gender == Gender.Male &&
                         d.LastDonationDate <= now.AddDays(-90)) || // 3 months for males
                        (d.Gender == Gender.Female &&
                         d.LastDonationDate <= now.AddDays(-120)))) // 4 months for females
            .Include(d => d.BloodType)
            .ToListAsync();
    }

    public async Task<Donor?> GetWithUserAsync(int donorId)
    {
        return await _dbSet
            .Include(d => d.User)
            .Include(d => d.BloodType)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DonorID == donorId);
    }

    public async Task<IEnumerable<Donor>> GetPendingApprovalAsync()
    {
        return await _dbSet
            .Where(d => d.ApprovalStatus == DonorApprovalStatus.PendingApproval)
            .Include(d => d.BloodType)
            .ToListAsync();
    }

    public async Task<IEnumerable<Donor>> GetByApprovalStatusAsync(DonorApprovalStatus status)
    {
        return await _dbSet
            .Where(d => d.ApprovalStatus == status)
            .Include(d => d.BloodType)
            .ToListAsync();
    }

    public async Task<Donor?> GetWithMedicalDocumentsAsync(int donorId)
    {
        return await _dbSet
            .Include(d => d.MedicalDocuments)
            .Include(d => d.BloodType)
            .FirstOrDefaultAsync(d => d.DonorID == donorId);
    }
}
