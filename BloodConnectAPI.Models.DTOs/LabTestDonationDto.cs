using BloodConnectAPI.Models.Enums;

namespace BloodConnectAPI.Models.DTOs;

/// <summary>
/// DTO لتسجيل نتيجة الفحص المختبري للتبرع
/// يستخدمه Admin بعد إجراء الفحوصات
/// </summary>
public class LabTestDonationDto
{
    public TestResult TestResult { get; set; }              // Accepted أو Rejected
    public string? TestNotes { get; set; }
    public bool AddToInventoryIfAccepted { get; set; } = true;  // إضافة تلقائية عند القبول
}
