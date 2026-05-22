namespace BloodConnectAPI.Models.DTOs.Reports.Common;

/// <summary>
/// فلتر التاريخ للتقارير
/// </summary>
public class DateFilterDto
{
    /// <summary>
    /// تاريخ البداية
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// تاريخ النهاية
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// التحقق من صحة التواريخ
    /// </summary>
    /// <returns>True إذا كانت التواريخ صحيحة، False إذا كانت غير صحيحة</returns>
    public bool IsValid()
    {
        if (StartDate.HasValue && EndDate.HasValue)
        {
            return StartDate.Value <= EndDate.Value;
        }
        return true;
    }

    /// <summary>
    /// التحقق من صحة التواريخ مع رسالة خطأ
    /// </summary>
    /// <returns>Tuple يحتوي على حالة الصحة ورسالة الخطأ</returns>
    public (bool IsValid, string ErrorMessage) Validate()
    {
        if (StartDate.HasValue && EndDate.HasValue && StartDate > EndDate)
        {
            return (false, "تاريخ البداية يجب أن يكون قبل تاريخ النهاية");
        }

        if (StartDate.HasValue && StartDate > DateTime.Now)
        {
            return (false, "تاريخ البداية لا يمكن أن يكون في المستقبل");
        }

        return (true, string.Empty);
    }
}
