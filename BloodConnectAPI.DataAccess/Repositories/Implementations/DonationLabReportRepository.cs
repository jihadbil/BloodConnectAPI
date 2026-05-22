using BloodConnectAPI.DataAccess.Data;
using BloodConnectAPI.DataAccess.Repositories.Interfaces;
using BloodConnectAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BloodConnectAPI.DataAccess.Repositories.Implementations;

public class DonationLabReportRepository : GenericRepository<DonationLabReport>, IDonationLabReportRepository
{
    public DonationLabReportRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<DonationLabReport>> GetByDonationIdAsync(int donationId)
    {
        return await FindAsync(r => r.DonationID == donationId);
    }
}
