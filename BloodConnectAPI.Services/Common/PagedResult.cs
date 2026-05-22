namespace BloodConnectAPI.Service.Common;

/// <summary>
/// نتيجة مقسمة على صفحات
/// </summary>
/// <typeparam name="T">نوع العناصر</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// العناصر في الصفحة الحالية
    /// </summary>
    public IEnumerable<T> Items { get; set; } = new List<T>();

    /// <summary>
    /// إجمالي عدد العناصر
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// رقم الصفحة الحالية
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// عدد العناصر في الصفحة
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// إجمالي عدد الصفحات
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// هل توجد صفحة سابقة؟
    /// </summary>
    public bool HasPrevious => PageNumber > 1;

    /// <summary>
    /// هل توجد صفحة تالية؟
    /// </summary>
    public bool HasNext => PageNumber < TotalPages;
}
