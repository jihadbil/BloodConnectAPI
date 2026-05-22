using System;
using System.Collections.Generic;
using System.Text;

namespace BloodConnectAPI.Models;
/// <summary>
    /// يربط بين طلب الدم والتبرعات التي لبّت هذا الطلب
    /// </summary>
public class BloodDisbursement
{

 
        public int DisbursementID { get; set; }

        public int RequestID { get; set; }
        public int DonationID { get; set; }

        /// <summary>
        /// الكمية المصروفة من وحدة الدم
        /// </summary>
        public int QuantityUsed { get; set; }

        public DateTime DisbursementDate { get; set; }

        /// <summary>
        /// تاريخ إنشاء السجل
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// تاريخ آخر تحديث
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // علاقات
        public BloodRequest BloodRequest { get; set; } = null!;
        public Donation Donation { get; set; } = null!;
    }


