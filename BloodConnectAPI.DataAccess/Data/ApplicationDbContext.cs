using BloodConnectAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BloodConnectAPI.DataAccess.Data;

/// <summary>
/// سياق قاعدة البيانات الرئيسي للتطبيق
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    #region DbSets
    
    // DbSet<User>, DbSet<Role>, DbSet<UserRole> removed for Identity

    /// <summary>
    /// المتبرعين
    /// </summary>
    public DbSet<Donor> Donors { get; set; }

    /// <summary>
    /// المرضى
    /// </summary>
    public DbSet<Patient> Patients { get; set; }

    /// <summary>
    /// فصائل الدم
    /// </summary>
    public DbSet<BloodType> BloodTypes { get; set; }

    /// <summary>
    /// التبرعات
    /// </summary>
    public DbSet<Donation> Donations { get; set; }

    /// <summary>
    /// طلبات الدم
    /// </summary>
    public DbSet<BloodRequest> BloodRequests { get; set; }

    /// <summary>
    /// المخزون
    /// </summary>
    public DbSet<BloodInventory> BloodInventories { get; set; }

    /// <summary>
    /// وحدات المخزون الفردية (مع تتبع الصلاحية)
    /// </summary>
    public DbSet<BloodInventoryItem> BloodInventoryItems { get; set; }

    /// <summary>
    /// عمليات صرف الدم
    /// </summary>
    public DbSet<BloodDisbursement> BloodDisbursements { get; set; }

    /// <summary>
    /// استجابات المتبرعين لطلبات الدم
    /// </summary>
    public DbSet<DonorRequestResponse> DonorRequestResponses { get; set; }

    /// <summary>
    /// الوثائق الطبية للمتبرعين
    /// </summary>
    public DbSet<DonorMedicalDocument> DonorMedicalDocuments { get; set; }

    /// <summary>
    /// الإشعارات
    /// </summary>
    public DbSet<Notification> Notifications { get; set; }

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Donor>().HasKey(e => e.DonorID);
        modelBuilder.Entity<Patient>().HasKey(e => e.PatientID);
        modelBuilder.Entity<BloodType>().HasKey(e => e.BloodTypeID);
        modelBuilder.Entity<Donation>().HasKey(e => e.DonationID);
        modelBuilder.Entity<BloodRequest>().HasKey(e => e.RequestID);
        modelBuilder.Entity<BloodInventory>().HasKey(e => e.InventoryID);
        modelBuilder.Entity<BloodInventoryItem>().HasKey(e => e.InventoryItemID);
        modelBuilder.Entity<BloodDisbursement>().HasKey(e => e.DisbursementID);
        modelBuilder.Entity<DonorRequestResponse>().HasKey(e => e.ResponseID);
        modelBuilder.Entity<DonorMedicalDocument>().HasKey(e => e.DocumentID);
        modelBuilder.Entity<Notification>().HasKey(e => e.NotificationID);

        // إعداد علاقة Notification مع ApplicationUser
        modelBuilder.Entity<Notification>()
            .HasOne(n => n.Recipient)
            .WithMany()
            .HasForeignKey(n => n.RecipientUserId)
            .OnDelete(DeleteBehavior.Cascade);

        // فهرس لتسريع الاستعلام عن إشعارات مستخدم معين
        modelBuilder.Entity<Notification>()
            .HasIndex(n => new { n.RecipientUserId, n.CreatedAt });

        // تعطيل الحذف التلقائي (Cascade Delete) لتجنب مشكلة المسارات المتعددة في SQL Server
        var cascadeFKs = modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetForeignKeys())
            .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

        foreach (var fk in cascadeFKs)
            fk.DeleteBehavior = DeleteBehavior.Restrict;

        // تطبيق جميع الـ Configurations من الـ Assembly الحالي
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    /// <summary>
    /// تحديث حقول التتبع الزمني تلقائياً
    /// </summary>
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    /// <summary>
    /// تحديث حقول التتبع الزمني تلقائياً (Async)
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// تحديث CreatedAt و UpdatedAt تلقائياً
    /// </summary>
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        var now = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            // عند الإضافة: تعيين CreatedAt أو AssignedAt
            if (entry.State == EntityState.Added)
            {
                var createdAtProperty = entry.Entity.GetType().GetProperty("CreatedAt");
                if (createdAtProperty != null && createdAtProperty.PropertyType == typeof(DateTime))
                {
                    createdAtProperty.SetValue(entry.Entity, now);
                }

                var assignedAtProperty = entry.Entity.GetType().GetProperty("AssignedAt");
                if (assignedAtProperty != null && assignedAtProperty.PropertyType == typeof(DateTime))
                {
                    assignedAtProperty.SetValue(entry.Entity, now);
                }
            }

            // عند التعديل: تحديث UpdatedAt أو LastUpdated
            if (entry.State == EntityState.Modified)
            {
                var updatedAtProperty = entry.Entity.GetType().GetProperty("UpdatedAt");
                if (updatedAtProperty != null && updatedAtProperty.PropertyType == typeof(DateTime?))
                {
                    updatedAtProperty.SetValue(entry.Entity, now);
                }

                var lastUpdatedProperty = entry.Entity.GetType().GetProperty("LastUpdated");
                if (lastUpdatedProperty != null && lastUpdatedProperty.PropertyType == typeof(DateTime))
                {
                    lastUpdatedProperty.SetValue(entry.Entity, now);
                }
            }
        }
    }
}
