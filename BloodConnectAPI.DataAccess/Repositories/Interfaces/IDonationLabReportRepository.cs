using BloodConnectAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BloodConnectAPI.DataAccess.Repositories.Interfaces;

public interface IDonationLabReportRepository : IGenericRepository<DonationLabReport>
{
    Task<IEnumerable<DonationLabReport>> GetByDonationIdAsync(int donationId);
}
