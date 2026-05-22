using System;

namespace BloodConnectAPI.Models;

/// <summary>
/// جدول تحاليل عينة التبرع المرفوعة
/// </summary>
public class DonationLabReport
{
    public int LabReportID { get; set; }
    public int DonationID { get; set; }

    // نوع التحليل (CBC, HBsAg, HIV, HCV, Syphilis, Other)
    public string ReportType { get; set; } = null!;

    // مسار الملف أو رابطه في wwwroot
    public string FilePath { get; set; } = null!;

    // معرف فني المختبر/المشرف الذي رفع الملف
    public string UploadedByUserId { get; set; } = null!;

    // تاريخ الرفع
    public DateTime UploadedAt { get; set; }

    // ملاحظات اختيارية
    public string? Notes { get; set; }

    // علاقة التنقل
    public Donation Donation { get; set; } = null!;
}
