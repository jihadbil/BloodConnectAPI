using BloodConnectAPI.Models.Enums;

namespace BloodConnectAPI.Models.DTOs;

/// <summary>
/// DTO لعرض بيانات وحدة دم في المخزون
/// </summary>
public class BloodInventoryItemDto
{
    public int InventoryItemID { get; set; }
    public int InventoryID { get; set; }
    public int DonationID { get; set; }
    public string DonorName { get; set; } = string.Empty;
    public string BloodTypeName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime ExpiryDate { get; set; }
    public BloodUnitStatus Status { get; set; }
    public bool IsUsed { get; set; }
    public DateTime AddedAt { get; set; }
    public DateTime? UsedAt { get; set; }

    /// <summary>
    /// هل الوحدة منتهية الصلاحية
    /// </summary>
    public bool IsExpired => DateTime.UtcNow > ExpiryDate;
}

/// <summary>
/// DTO لإنشاء وحدة مخزون جديدة
/// </summary>
public class CreateBloodInventoryItemDto
{
    public int InventoryID { get; set; }
    public int DonationID { get; set; }
    public int Quantity { get; set; }
}

/// <summary>
/// DTO لتحديث بيانات وحدة المخزون
/// </summary>
public class UpdateBloodInventoryItemDto
{
    public int? Quantity { get; set; }
    public BloodUnitStatus? Status { get; set; }
}
