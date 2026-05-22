using System;
using System.Collections.Generic;
using System.Text;
using BloodConnectAPI.Models.Enums;

namespace BloodConnectAPI.Models;

    /// <summary>
    /// يمثل عملية تبرع بالدم
    /// </summary>
public class Donation
{


   
        public int DonationID { get; set; }

        public int DonorID { get; set; }
        public int BloodTypeID { get; set; }

        public DateTime DonationDate { get; set; }

        /// <summary>
        /// كمية الدم المتبرع بها
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// نتيجة فحص الدم
        /// </summary>
        public TestResult TestResult { get; set; }

        public string? Notes { get; set; }

        /// <summary>
        /// تاريخ إجراء الفحص
        /// </summary>
        public DateTime? TestedAt { get; set; }

        /// <summary>
        /// معرف فني المختبر الذي أجرى الفحص
        /// </summary>
        public string? TestedByUserId { get; set; }

        /// <summary>
        /// ملاحظات الفحص
        /// </summary>
        public string? TestNotes { get; set; }

        /// <summary>
        /// هل تم إضافة هذا التبرع للمخزون
        /// </summary>
        public bool IsAddedToInventory { get; set; } = false;

        /// <summary>
        /// تاريخ إنشاء السجل
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// تاريخ آخر تحديث
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // علاقات
        public Donor Donor { get; set; } = null!;
        public BloodType BloodType { get; set; } = null!;
        public ICollection<BloodDisbursement> BloodDisbursements { get; set; } = new List<BloodDisbursement>();
        public ICollection<DonationLabReport> LabReports { get; set; } = new List<DonationLabReport>();
    }


