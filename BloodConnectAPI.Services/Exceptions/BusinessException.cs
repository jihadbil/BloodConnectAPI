namespace BloodConnectAPI.Service.Exceptions;

/// <summary>
/// Exception لأخطاء Business Logic
/// </summary>
public class BusinessException : Exception
{
    public BusinessException(string message) : base(message)
    {
    }

    public BusinessException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
