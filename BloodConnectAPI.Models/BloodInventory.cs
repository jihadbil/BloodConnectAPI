using System;
using System.Collections.Generic;
using System.Text;

namespace BloodConnectAPI.Models;

/// <summary>
/// يمتل المخزون و وحدات الدم
/// </summary>
public class BloodInventory
{


public int     InventoryID {get; set; }
 public int   BloodTypeID {get; set; }

    public BloodType BloodType { get; set; } = null!;

    /// <summary>
    /// الكمية المتاحة
    /// </summary>
    public int QuantityAvailable { get; set; }

    /// <summary>
    /// الكمية المحجوزة (للطلبات المعلقة)
    /// </summary>
    public int QuantityReserved { get; set; }

    /// <summary>
    /// تاريخ إنشاء السجل
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// تاريخ آخر تحديث (آخر عملية إضافة أو سحب)
    /// </summary>
    public DateTime LastUpdated { get; set; }

    // علاقات
    public ICollection<BloodInventoryItem> InventoryItems { get; set; } = new List<BloodInventoryItem>();


}
