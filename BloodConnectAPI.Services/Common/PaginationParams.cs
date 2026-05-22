namespace BloodConnectAPI.Service.Common;

/// <summary>
/// معاملات Pagination
/// </summary>
public class PaginationParams
{
    /// <summary>
    /// رقم الصفحة (يبدأ من 1)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// عدد العناصر في الصفحة
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// مصطلح البحث (اختياري)
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// الترتيب حسب (اختياري)
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// ترتيب تنازلي؟
    /// </summary>
    public bool SortDescending { get; set; } = false;
}
