using BloodConnectAPI.DataAccess.Data;
using BloodConnectAPI.DataAccess.Repositories.Interfaces;
using BloodConnectAPI.Models;
using BloodConnectAPI.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BloodConnectAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// Repository Implementation لاستجابات المتبرعين
/// </summary>
public class DonorRequestResponseRepository : GenericRepository<DonorRequestResponse>, IDonorRequestResponseRepository
{
    public DonorRequestResponseRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<DonorRequestResponse>> GetByRequestIdAsync(int requestId)
    {
        return await _dbSet
            .Where(r => r.RequestID == requestId)
            .Include(r => r.Donor)
                .ThenInclude(d => d.BloodType)
            .Include(r => r.BloodRequest)
                .ThenInclude(br => br.Patient)
            .OrderByDescending(r => r.ResponseDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<DonorRequestResponse>> GetByDonorIdAsync(int donorId)
    {
        return await _dbSet
            .Where(r => r.DonorID == donorId)
            .Include(r => r.BloodRequest)
                .ThenInclude(br => br.Patient)
            .Include(r => r.BloodRequest)
                .ThenInclude(br => br.BloodType)
            .OrderByDescending(r => r.ResponseDate)
            .ToListAsync();
    }

    public async Task<DonorRequestResponse?> GetActiveResponseAsync(int donorId, int requestId)
    {
        return await _dbSet
            .Where(r => r.DonorID == donorId
                     && r.RequestID == requestId
                     && (r.Status == ResponseStatus.Interested || r.Status == ResponseStatus.Confirmed))
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<DonorRequestResponse>> GetByStatusAsync(ResponseStatus status)
    {
        return await _dbSet
            .Where(r => r.Status == status)
            .Include(r => r.Donor)
                .ThenInclude(d => d.BloodType)
            .Include(r => r.BloodRequest)
                .ThenInclude(br => br.Patient)
            .OrderByDescending(r => r.ResponseDate)
            .ToListAsync();
    }

    public async Task<DonorRequestResponse?> GetWithDetailsAsync(int responseId)
    {
        return await _dbSet
            .Include(r => r.Donor)
                .ThenInclude(d => d.BloodType)
            .Include(r => r.BloodRequest)
                .ThenInclude(br => br.Patient)
            .Include(r => r.BloodRequest)
                .ThenInclude(br => br.BloodType)
            .Include(r => r.Donation)
            .FirstOrDefaultAsync(r => r.ResponseID == responseId);
    }

    public async Task<bool> HasActiveResponseAsync(int donorId, int requestId)
    {
        return await _dbSet
            .AnyAsync(r => r.DonorID == donorId
                        && r.RequestID == requestId
                        && (r.Status == ResponseStatus.Interested || r.Status == ResponseStatus.Confirmed));
    }
}
