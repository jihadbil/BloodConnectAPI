using BloodConnectAPI.DataAccess.Data;
using BloodConnectAPI.DataAccess.Repositories.Interfaces;
using BloodConnectAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BloodConnectAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// Repository Implementation للمرضى
/// </summary>
public class PatientRepository : GenericRepository<Patient>, IPatientRepository
{
    public PatientRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Patient?> GetByNationalIdAsync(string nationalId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.NationalID == nationalId);
    }

    public async Task<Patient?> GetWithRequestsAsync(int patientId)
    {
        return await _dbSet
            .Include(p => p.BloodRequests)
                .ThenInclude(br => br.BloodType)
            .FirstOrDefaultAsync(p => p.PatientID == patientId);
    }

    public async Task<IEnumerable<Patient>> GetRecentPatientsAsync(int days = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);

        return await _dbSet
            .Where(p => p.CreatedAt >= cutoffDate)
            .Include(p => p.BloodType)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Patient>> GetPatientsByBloodTypeAsync(int bloodTypeId)
    {
        return await _dbSet
            .Where(p => p.BloodTypeID == bloodTypeId)
            .ToListAsync();
    }
}
