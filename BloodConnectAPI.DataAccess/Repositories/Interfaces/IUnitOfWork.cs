using BloodConnectAPI.DataAccess.Repositories.Interfaces;

namespace BloodConnectAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// Unit of Work Pattern - يضم جميع Repositories ويحكم Transactions
/// </summary>
public interface IUnitOfWork : IDisposable
{
    #region Repositories

    // Repositories الخاصة بالمصادقة (Users, Roles, UserRoles) محذوفة
    // حيث أصبحت تدار عبر Identity (UserManager, RoleManager)

    /// <summary>
    /// Repository المتبرعين
    /// </summary>
    IDonorRepository Donors { get; }

    /// <summary>
    /// Repository المرضى
    /// </summary>
    IPatientRepository Patients { get; }

    /// <summary>
    /// Repository فصائل الدم
    /// </summary>
    IBloodTypeRepository BloodTypes { get; }

    /// <summary>
    /// Repository التبرعات
    /// </summary>
    IDonationRepository Donations { get; }

    /// <summary>
    /// Repository طلبات الدم
    /// </summary>
    IBloodRequestRepository BloodRequests { get; }

    /// <summary>
    /// Repository المخزون
    /// </summary>
    IBloodInventoryRepository BloodInventories { get; }

    /// <summary>
    /// Repository عناصر المخزون
    /// </summary>
    IBloodInventoryItemRepository BloodInventoryItems { get; }

    /// <summary>
    /// Repository عمليات صرف الدم
    /// </summary>
    IBloodDisbursementRepository BloodDisbursements { get; }

    /// <summary>
    /// Repository استجابات المتبرعين لطلبات الدم
    /// </summary>
    IDonorRequestResponseRepository DonorResponseRepository { get; }

    /// <summary>
    /// Repository الوثائق الطبية للمتبرعين
    /// </summary>
    IDonorMedicalDocumentRepository DonorMedicalDocuments { get; }

    /// <summary>
    /// Repository الإشعارات
    /// </summary>
    INotificationRepository Notifications { get; }

    #endregion

    #region Operations

    /// <summary>
    /// حفظ جميع التغييرات في قاعدة البيانات
    /// </summary>
    Task<int> SaveChangesAsync();

    /// <summary>
    /// بدء Transaction
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    /// تأكيد Transaction
    /// </summary>
    Task CommitTransactionAsync();

    /// <summary>
    /// التراجع عن Transaction
    /// </summary>
    Task RollbackTransactionAsync();

    #endregion
}
