using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace BloodConnectAPI.Models;

/// <summary>
/// يمثل فصيلة الدم (A+, O- ... إلخ)
/// جدول مرجعي ثابت داخل النظام
/// </summary>
public class BloodType
{


    /// <summary>
    /// المفتاح الأساسي لفصيلة الدم
    /// </summary>
    public int BloodTypeID { get; set; }

    /// <summary>
    /// اسم فصيلة الدم مثل A+ أو O-
    /// </summary>
    public string TypeName { get; set; } = null!;

    /// <summary>
    /// تاريخ إنشاء السجل
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// تاريخ آخر تحديث
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    // علاقات التنقل
    public ICollection<Donor> Donors { get; set; } = new List<Donor>();
    public ICollection<Patient> Patients { get; set; } = new List<Patient>();
    public ICollection<BloodRequest> BloodRequests { get; set; } = new List<BloodRequest>();
    public ICollection<Donation> Donations { get; set; } = new List<Donation>();
    public ICollection<BloodInventory> BloodInventories { get; set; } = new List<BloodInventory>();

}
