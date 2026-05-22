using System;
using System.Collections.Generic;
using System.Text;
using BloodConnectAPI.Models.Enums;

namespace BloodConnectAPI.Models;

    /// <summary>
    /// يمثل طلب دم لمريض معين
    /// </summary>
public class BloodRequest
{



 
        public int RequestID { get; set; }

        /// <summary>
        /// المريض الذي أُنشئ الطلب من أجله
        /// </summary>
        public int PatientID { get; set; }

        /// <summary>
        /// فصيلة الدم المطلوبة
        /// </summary>
        public int BloodTypeID { get; set; }

        /// <summary>
        /// عدد أكياس الدم المطلوبة
        /// </summary>
        public int QuantityNeeded { get; set; }

        /// <summary>
        /// درجة الاستعجال (عادي - عاجل - طارئ)
        /// </summary>
        public UrgencyLevel UrgencyLevel { get; set; }

        /// <summary>
        /// تاريخ إنشاء الطلب
        /// </summary>
        public DateTime RequestDate { get; set; }

        /// <summary>
        /// التاريخ المطلوب لتوفير الدم
        /// </summary>
        public DateTime RequiredDate { get; set; }

        /// <summary>
        /// حالة الطلب
        /// تُحسب من الكمية المُلباة مقارنة بالكمية المطلوبة
        /// </summary>
        public RequestStatus Status { get; set; }

        /// <summary>
        /// الكمية التي تم توفيرها حتى الآن
        /// يتم تحديثها تلقائياً عند كل عملية صرف
        /// </summary>
        public int QuantityFulfilled { get; set; } = 0;

        public string? Notes { get; set; }

        /// <summary>
        /// تاريخ إنشاء السجل
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// تاريخ آخر تحديث
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // علاقات
        public Patient Patient { get; set; } = null!;
        public BloodType BloodType { get; set; } = null!;
        public ICollection<BloodDisbursement> BloodDisbursements { get; set; } = new List<BloodDisbursement>();

        /// <summary>
        /// استجابات المتبرعين لهذا الطلب
        /// </summary>
        public ICollection<DonorRequestResponse> DonorResponses { get; set; } = new List<DonorRequestResponse>();
    }


