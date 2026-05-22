namespace BloodConnectAPI.Service.Exceptions;

/// <summary>
/// Exception عندما لا يتم العثور على Entity
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string entityName, object key)
        : base($"{entityName} بمعرف '{key}' غير موجود")
    {
    }
}
