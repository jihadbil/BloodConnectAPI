namespace BloodConnectAPI.Models.DTOs.Reports.Common;

/// <summary>
/// معاملات Pagination للتقارير
/// </summary>
public class PaginationParams
{
    private int _pageNumber = 1;
    private int _pageSize = 10;

    /// <summary>
    /// رقم الصفحة (يبدأ من 1)
    /// </summary>
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    /// <summary>
    /// عدد العناصر في الصفحة (الحد الأدنى 1، الحد الأقصى 100)
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 10 : (value > 100 ? 100 : value);
    }

    /// <summary>
    /// التحقق من صحة معاملات Pagination
    /// </summary>
    /// <returns>True إذا كانت المعاملات صحيحة، False إذا كانت غير صحيحة</returns>
    public bool IsValid()
    {
        return PageNumber >= 1 && PageSize >= 1 && PageSize <= 100;
    }

    /// <summary>
    /// التحقق من صحة معاملات Pagination مع رسالة خطأ
    /// </summary>
    /// <returns>Tuple يحتوي على حالة الصحة ورسالة الخطأ</returns>
    public (bool IsValid, string ErrorMessage) Validate()
    {
        if (PageNumber < 1)
        {
            return (false, "رقم الصفحة يجب أن يكون أكبر من صفر");
        }

        if (PageSize < 1)
        {
            return (false, "حجم الصفحة يجب أن يكون أكبر من صفر");
        }

        if (PageSize > 100)
        {
            return (false, "حجم الصفحة يجب أن لا يتجاوز 100");
        }

        return (true, string.Empty);
    }
}
