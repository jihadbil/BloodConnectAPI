namespace BloodConnectAPI.Service.Exceptions;

/// <summary>
/// Exception عند عدم وجود صلاحية
/// </summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "غير مصرح لك بهذا الإجراء")
        : base(message)
    {
    }
}
