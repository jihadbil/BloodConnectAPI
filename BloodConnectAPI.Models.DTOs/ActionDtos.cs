namespace BloodConnectAPI.Models.DTOs;

public class CancelRequestDto
{
    public string Reason { get; set; } = string.Empty;
}

public class UpdateTestResultDto
{
    public BloodConnectAPI.Models.Enums.TestResult TestResult { get; set; }
}

public class UpdateQuantityDto
{
    public int QuantityChange { get; set; }
}
