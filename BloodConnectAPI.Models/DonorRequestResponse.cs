using BloodConnectAPI.Models.Enums;

namespace BloodConnectAPI.Models;

/// <summary>
/// يُسجّل استجابة متبرع لطلب دم معين، ويتتبع مراحلها حتى التبرع الفعلي
/// </summary>
public class DonorRequestResponse
{
    /// <summary>
    /// المفتاح الأساسي للاستجابة
    /// </summary>
    public int ResponseID { get; set; }

    /// <summary>
    /// المتبرع الذي استجاب للطلب
    /// </summary>
    public int DonorID { get; set; }

    /// <summary>
    /// طلب الدم الذي جرى الاستجابة له
    /// </summary>
    public int RequestID { get; set; }

    /// <summary>
    /// حالة الاستجابة الحالية
    /// </summary>
    public ResponseStatus Status { get; set; }

    /// <summary>
    /// ملاحظات المتبرع أو الموظف
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// سبب الرفض أو الإلغاء (إن وجد)
    /// </summary>
    public string? RejectionReason { get; set; }

    /// <summary>
    /// تاريخ الاستجابة الأولية (Interested)
    /// </summary>
    public DateTime ResponseDate { get; set; }

    /// <summary>
    /// تاريخ التأكيد من قِبل الموظف
    /// </summary>
    public DateTime? ConfirmedAt { get; set; }

    /// <summary>
    /// معرف التبرع الفعلي المرتبط بهذه الاستجابة (يُملأ عند Status = Donated)
    /// </summary>
    public int? DonationID { get; set; }

    /// <summary>
    /// تاريخ إنشاء السجل
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// تاريخ آخر تحديث
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    public Donor Donor { get; set; } = null!;
    public BloodRequest BloodRequest { get; set; } = null!;
    public Donation? Donation { get; set; }
}
