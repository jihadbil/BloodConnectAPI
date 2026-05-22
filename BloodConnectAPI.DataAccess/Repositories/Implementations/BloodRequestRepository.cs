using System.Linq.Expressions;
using BloodConnectAPI.DataAccess.Data;
using BloodConnectAPI.DataAccess.Repositories.Interfaces;
using BloodConnectAPI.Models;
using BloodConnectAPI.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BloodConnectAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// Repository Implementation لطلبات الدم
/// </summary>
public class BloodRequestRepository : GenericRepository<BloodRequest>, IBloodRequestRepository
{
    public BloodRequestRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<(IEnumerable<BloodRequest> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<BloodRequest, bool>>? filter = null,
        Func<IQueryable<BloodRequest>, IOrderedQueryable<BloodRequest>>? orderBy = null)
    {
        IQueryable<BloodRequest> query = _dbSet
            .Include(br => br.Patient)
            .Include(br => br.BloodType);

        // تطبيق الفلتر
        if (filter != null)
        {
            query = query.Where(filter);
        }

        // حساب العدد الإجمالي
        var totalCount = await query.CountAsync();

        // تطبيق الترتيب
        if (orderBy != null)
        {
            query = orderBy(query);
        }

        // تطبيق Pagination
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<BloodRequest>> GetPendingRequestsAsync()
    {
        return await _dbSet
            .Where(br => br.Status == RequestStatus.Pending)
            .Include(br => br.Patient)
            .Include(br => br.BloodType)
            .OrderByDescending(br => br.UrgencyLevel)
            .ThenBy(br => br.RequiredDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<BloodRequest>> GetUrgentRequestsAsync()
    {
        return await _dbSet
            .Where(br => br.UrgencyLevel == UrgencyLevel.Urgent && 
                        br.Status == RequestStatus.Pending)
            .Include(br => br.Patient)
            .Include(br => br.BloodType)
            .OrderBy(br => br.RequiredDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<BloodRequest>> GetRequestsByPatientAsync(int patientId)
    {
        return await _dbSet
            .Where(br => br.PatientID == patientId)
            .Include(br => br.BloodType)
            .OrderByDescending(br => br.RequestDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<BloodRequest>> GetRequestsByBloodTypeAsync(int bloodTypeId)
    {
        return await _dbSet
            .Where(br => br.BloodTypeID == bloodTypeId)
            .Include(br => br.Patient)
            .ToListAsync();
    }

    public async Task<BloodRequest?> GetWithDetailsAsync(int requestId)
    {
        return await _dbSet
            .Include(br => br.Patient)
            .Include(br => br.BloodType)
            .Include(br => br.BloodDisbursements)
            .FirstOrDefaultAsync(br => br.RequestID == requestId);
    }

    public async Task<IEnumerable<BloodRequest>> GetByStatusAsync(RequestStatus status)
    {
        return await _dbSet
            .Where(br => br.Status == status)
            .Include(br => br.Patient)
            .Include(br => br.BloodType)
            .ToListAsync();
    }

    public async Task<IEnumerable<BloodRequest>> GetByUrgencyLevelAsync(UrgencyLevel urgencyLevel)
    {
        return await _dbSet
            .Where(br => br.UrgencyLevel == urgencyLevel)
            .Include(br => br.Patient)
            .Include(br => br.BloodType)
            .ToListAsync();
    }

    public async Task<IEnumerable<BloodRequest>> GetPartiallyFulfilledAsync()
    {
        return await _dbSet
            .Where(br => br.Status == RequestStatus.Pending || br.Status == RequestStatus.PartiallyFulfilled)
            .Include(br => br.Patient)
            .Include(br => br.BloodType)
            .ToListAsync();
    }

    public async Task UpdateFulfillmentAsync(int requestId, int quantityToAdd)
    {
        var request = await _dbSet.FirstOrDefaultAsync(r => r.RequestID == requestId);
        if (request != null)
        {
            request.QuantityFulfilled += quantityToAdd;
            
            if (request.QuantityFulfilled >= request.QuantityNeeded)
            {
                request.Status = RequestStatus.Fulfilled;
            }
            else if (request.QuantityFulfilled > 0)
            {
                request.Status = RequestStatus.PartiallyFulfilled;
            }
            // DbContext.SaveChanges is expected to be called by UnitOfWork
        }
    }
}
