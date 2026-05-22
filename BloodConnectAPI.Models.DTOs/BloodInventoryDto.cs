namespace BloodConnectAPI.Models.DTOs;

/// <summary>
/// DTO لعرض بيانات المخزون
/// </summary>
public class BloodInventoryDto
{
    public int InventoryID { get; set; }
    public int BloodTypeID { get; set; }
    public string BloodTypeName { get; set; } = string.Empty;
    public int QuantityAvailable { get; set; }
    public int QuantityReserved { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdated { get; set; }

    /// <summary>
    /// عدد الوحدات الفردية في هذا المخزون
    /// </summary>
    public int ItemsCount { get; set; }
}

/// <summary>
/// DTO لإنشاء سجل مخزون جديد
/// </summary>
public class CreateBloodInventoryDto
{
    public int BloodTypeID { get; set; }
    public int QuantityAvailable { get; set; }
    public int QuantityReserved { get; set; }
}

/// <summary>
/// DTO لتحديث بيانات المخزون
/// </summary>
public class UpdateBloodInventoryDto
{
    public int? QuantityAvailable { get; set; }
    public int? QuantityReserved { get; set; }
}
