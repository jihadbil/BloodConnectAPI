using System;

using BloodConnectAPI.Models.Enums;

namespace BloodConnectAPI.Models;

/// <summary>
/// يمثل وحدة دم واحدة في المخزون مع تاريخ انتهاء الصلاحية
/// يتم إنشاء سجل جديد لكل تبرع مقبول
/// </summary>
public class BloodInventoryItem
{
    /// <summary>
    /// المفتاح الأساسي لوحدة المخزون
    /// </summary>
    public int InventoryItemID { get; set; }

    /// <summary>
    /// رقم المخزون المرتبط بفصيلة الدم
    /// </summary>
    public int InventoryID { get; set; }

    /// <summary>
    /// رقم التبرع المرتبط بهذه الوحدة
    /// </summary>
    public int DonationID { get; set; }

    /// <summary>
    /// الكمية بالوحدات
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// تاريخ انتهاء الصلاحية (3 أيام من تاريخ الإضافة للمخزون)
    /// ExpiryDate = AddedAt.AddDays(3)
    /// </summary>
    public DateTime ExpiryDate { get; set; }

    /// <summary>
    /// حالة وحدة الدم
    /// </summary>
    public BloodUnitStatus Status { get; set; } = BloodUnitStatus.Available;

    /// <summary>
    /// هل تم استخدام هذه الوحدة
    /// </summary>
    public bool IsUsed { get; set; }

    /// <summary>
    /// تاريخ الإضافة للمخزون
    /// </summary>
    public DateTime AddedAt { get; set; }

    /// <summary>
    /// تاريخ الاستخدام (عند الصرف)
    /// </summary>
    public DateTime? UsedAt { get; set; }

    // علاقات التنقل
    public BloodInventory Inventory { get; set; } = null!;
    public Donation Donation { get; set; } = null!;
}
