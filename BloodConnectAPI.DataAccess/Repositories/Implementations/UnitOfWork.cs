using BloodConnectAPI.DataAccess.Data;
using BloodConnectAPI.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace BloodConnectAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// Unit of Work Implementation
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    // Repositories - Lazy Loading
    private IDonorRepository? _donors;
    private IPatientRepository? _patients;
    private IBloodTypeRepository? _bloodTypes;
    private IDonationRepository? _donations;
    private IBloodRequestRepository? _bloodRequests;
    private IBloodInventoryRepository? _bloodInventories;
    private IBloodInventoryItemRepository? _bloodInventoryItems;
    private IBloodDisbursementRepository? _bloodDisbursements;
    private IDonorRequestResponseRepository? _donorResponseRepository;
    private IDonorMedicalDocumentRepository? _donorMedicalDocuments;
    private INotificationRepository? _notifications;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Repositories Properties

    public IDonorRepository Donors
    {
        get { return _donors ??= new DonorRepository(_context); }
    }

    public IPatientRepository Patients
    {
        get { return _patients ??= new PatientRepository(_context); }
    }

    public IBloodTypeRepository BloodTypes
    {
        get { return _bloodTypes ??= new BloodTypeRepository(_context); }
    }

    public IDonationRepository Donations
    {
        get { return _donations ??= new DonationRepository(_context); }
    }

    public IBloodRequestRepository BloodRequests
    {
        get { return _bloodRequests ??= new BloodRequestRepository(_context); }
    }

    public IBloodInventoryRepository BloodInventories
    {
        get { return _bloodInventories ??= new BloodInventoryRepository(_context); }
    }

    public IBloodInventoryItemRepository BloodInventoryItems
    {
        get { return _bloodInventoryItems ??= new BloodInventoryItemRepository(_context); }
    }

    public IBloodDisbursementRepository BloodDisbursements
    {
        get { return _bloodDisbursements ??= new BloodDisbursementRepository(_context); }
    }

    public IDonorRequestResponseRepository DonorResponseRepository
    {
        get { return _donorResponseRepository ??= new DonorRequestResponseRepository(_context); }
    }

    public IDonorMedicalDocumentRepository DonorMedicalDocuments
    {
        get { return _donorMedicalDocuments ??= new DonorMedicalDocumentRepository(_context); }
    }

    public INotificationRepository Notifications
    {
        get { return _notifications ??= new NotificationRepository(_context); }
    }

    #endregion

    #region Operations

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await _context.SaveChangesAsync();

            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    #endregion

    #region Dispose

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }

    #endregion
}
