namespace BloodConnectAPI.Models.DTOs;

/// <summary>
/// DTO لعرض بيانات فصيلة الدم
/// </summary>
public class BloodTypeDto
{
    public int BloodTypeID { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO لإنشاء فصيلة دم جديدة
/// </summary>
public class CreateBloodTypeDto
{
    public string TypeName { get; set; } = string.Empty;
}

/// <summary>
/// DTO لتحديث بيانات فصيلة دم
/// </summary>
public class UpdateBloodTypeDto
{
    public string TypeName { get; set; } = string.Empty;
}
