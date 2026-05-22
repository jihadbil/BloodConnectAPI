using System.Linq.Expressions;

namespace BloodConnectAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// Generic Repository Interface للعمليات المشتركة
/// </summary>
/// <typeparam name="T">نوع الـ Entity</typeparam>
public interface IGenericRepository<T> where T : class
{
    #region القراءة

    /// <summary>
    /// جلب Entity حسب المفتاح الأساسي
    /// </summary>
    Task<T?> GetByIdAsync(int id);

    /// <summary>
    /// جلب جميع الـ Entities
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// البحث باستخدام LINQ Expression
    /// </summary>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// جلب أول Entity يطابق الشرط
    /// </summary>
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

    #endregion

    #region Pagination

    /// <summary>
    /// جلب البيانات مع Pagination
    /// </summary>
    /// <param name="pageNumber">رقم الصفحة (يبدأ من 1)</param>
    /// <param name="pageSize">عدد العناصر في الصفحة</param>
    /// <param name="filter">فلتر اختياري</param>
    /// <param name="orderBy">ترتيب اختياري</param>
    /// <returns>العناصر + العدد الإجمالي</returns>
    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);

    #endregion

    #region الإضافة

    /// <summary>
    /// إضافة Entity جديد
    /// </summary>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// إضافة مجموعة من الـ Entities
    /// </summary>
    Task AddRangeAsync(IEnumerable<T> entities);

    #endregion

    #region التحديث

    /// <summary>
    /// تحديث Entity
    /// </summary>
    Task UpdateAsync(T entity);

    /// <summary>
    /// تحديث مجموعة من الـ Entities
    /// </summary>
    Task UpdateRangeAsync(IEnumerable<T> entities);

    #endregion

    #region الحذف

    /// <summary>
    /// حذف Entity
    /// </summary>
    Task DeleteAsync(T entity);

    /// <summary>
    /// حذف مجموعة من الـ Entities
    /// </summary>
    Task DeleteRangeAsync(IEnumerable<T> entities);

    #endregion

    #region عمليات إضافية

    /// <summary>
    /// التحقق من وجود Entity يطابق الشرط
    /// </summary>
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// عد الـ Entities
    /// </summary>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

    #endregion
}
