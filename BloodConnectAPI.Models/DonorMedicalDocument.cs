using System;

namespace BloodConnectAPI.Models;

/// <summary>
/// جدول التحاليل المرفوعة
/// </summary>
public class DonorMedicalDocument
{
    public int DocumentID { get; set; }
    public int DonorID { get; set; }

    // نوع الوثيقة (CBC, HBsAg, HIV, HCV, Syphilis...)
    public string DocumentType { get; set; } = null!;

    // مسار الملف أو رابطه
    public string FilePath { get; set; } = null!;

    // تاريخ الرفع
    public DateTime UploadedAt { get; set; }

    // هل تم التحقق منها من قبل الإدارة
    public bool IsVerified { get; set; }

    public string? Notes { get; set; }

    // علاقة التنقل
    public Donor Donor { get; set; } = null!;
}
