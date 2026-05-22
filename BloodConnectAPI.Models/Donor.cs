using System;
using System.Collections.Generic;
using System.Text;
using BloodConnectAPI.Models.Enums;

namespace BloodConnectAPI.Models;

/// <summary>
/// يمثل المتبرعين المسجلين في بنك الدم
/// </summary>
public class Donor
{

        /// <summary>
        /// المفتاح الأساسي للمتبرع
        /// </summary>
        public int DonorID { get; set; }

        /// <summary>
        /// الاسم الكامل للمتبرع
        /// </summary>
        public string FullName { get; set; } = null!;

        /// <summary>
        /// الرقم الوطني للمتبرع
        /// </summary>
        public string NationalID { get; set; } = null!;

        /// <summary>
        /// جنس المتبرع
        /// </summary>
        public Gender Gender { get; set; }

        /// <summary>
        /// تاريخ الميلاد
        /// </summary>
        public DateTime DateOfBirth { get; set; }

        /// <summary>
        /// رقم الهاتف
        /// </summary>
        public string Phone { get; set; } = null!;

        /// <summary>
        /// مفتاح فصيلة الدم
        /// </summary>
        public int BloodTypeID { get; set; }

        /// <summary>
        /// المدينة
        /// </summary>
        public string City { get; set; } = null!;

        /// <summary>
        /// تاريخ آخر تبرع
        /// </summary>
        public DateTime? LastDonationDate { get; set; }

        /// <summary>
        /// هل المتبرع نشط أم لا
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// تاريخ إنشاء السجل
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// تاريخ آخر تحديث
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// حالة الموافقة
        /// </summary>
        public DonorApprovalStatus ApprovalStatus { get; set; } = DonorApprovalStatus.PendingDocuments;

        /// <summary>
        /// سبب الرفض أو الرد (اختياري)
        /// </summary>
        public string? RejectionReason { get; set; }

        /// <summary>
        /// تاريخ الموافقة أو الرفض
        /// </summary>
        public DateTime? ApprovalDate { get; set; }

        /// <summary>
        /// معرف الموظف الذي راجع الطلب
        /// </summary>
        public string? ReviewedByUserId { get; set; }

        /// <summary>
        /// التحاليل المرفوعة
        /// </summary>
        public ICollection<DonorMedicalDocument> MedicalDocuments { get; set; } = new List<DonorMedicalDocument>();

        /// <summary>
        /// معرف المستخدم المرتبط بالمتبرع (اختياري)
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// المستخدم المرتبط بالمتبرع
        /// </summary>
        public ApplicationUser? User { get; set; }

        // علاقات
        public BloodType BloodType { get; set; } = null!;
        public ICollection<Donation> Donations { get; set; } = new List<Donation>();

        /// <summary>
        /// استجابات هذا المتبرع لطلبات الدم
        /// </summary>
        public ICollection<DonorRequestResponse> RequestResponses { get; set; } = new List<DonorRequestResponse>();
    }





