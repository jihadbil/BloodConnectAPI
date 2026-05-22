namespace BloodConnectAPI.Service.Exceptions;

/// <summary>
/// Exception عند فشل التحقق من البيانات
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// قائمة الأخطاء
    /// </summary>
    public List<string> Errors { get; }

    public ValidationException(List<string> errors) : base("فشل التحقق من البيانات")
    {
        Errors = errors;
    }

    public ValidationException(string error) : base("فشل التحقق من البيانات")
    {
        Errors = new List<string> { error };
    }
}
