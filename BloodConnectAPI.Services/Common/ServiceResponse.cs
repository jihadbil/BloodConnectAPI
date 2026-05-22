namespace BloodConnectAPI.Service.Common;

/// <summary>
/// Generic Response Wrapper للاستجابات
/// </summary>
/// <typeparam name="T">نوع البيانات</typeparam>
public class ServiceResponse<T>
{
    /// <summary>
    /// هل العملية نجحت؟
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// رسالة توضيحية
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// البيانات المرتجعة
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// قائمة الأخطاء (إن وجدت)
    /// </summary>
    public List<string>? Errors { get; set; }

    /// <summary>
    /// إنشاء استجابة ناجحة
    /// </summary>
    public static ServiceResponse<T> SuccessResponse(T data, string message = "تمت العملية بنجاح")
    {
        return new ServiceResponse<T>
        {
            IsSuccess = true,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// إنشاء استجابة فاشلة
    /// </summary>
    public static ServiceResponse<T> FailureResponse(string message, List<string>? errors = null)
    {
        return new ServiceResponse<T>
        {
            IsSuccess = false,
            Message = message,
            Errors = errors
        };
    }
}
