using BloodConnectAPI.DataAccess.Data;
using BloodConnectAPI.DataAccess.Repositories.Interfaces;
using BloodConnectAPI.Models;

namespace BloodConnectAPI.DataAccess.Repositories.Implementations;

public class DonorMedicalDocumentRepository : GenericRepository<DonorMedicalDocument>, IDonorMedicalDocumentRepository
{
    public DonorMedicalDocumentRepository(ApplicationDbContext context) : base(context)
    {
    }
}
