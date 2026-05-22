using System;
using System.Collections.Generic;
using System.Text;
using BloodConnectAPI.Models.Enums;

namespace BloodConnectAPI.Models;
/// <summary>
    /// يمثل المرضى الذين يتم طلب الدم لهم
    /// </summary>
public class Patient
{



    
  
        public int PatientID { get; set; }

        public string FullName { get; set; } = null!;
        public string NationalID { get; set; } = null!;
        
        /// <summary>
        /// جنس المريض
        /// </summary>
        public Gender Gender { get; set; }
        
        public DateTime DateOfBirth { get; set; }
        public string Phone { get; set; } = null!;
        public string City { get; set; } = null!;

        /// <summary>
        /// فصيلة دم المريض
        /// </summary>
        public int BloodTypeID { get; set; }

        /// <summary>
        /// تاريخ التسجيل
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// تاريخ آخر تحديث
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // علاقات
        public BloodType BloodType { get; set; } = null!;
        public ICollection<BloodRequest> BloodRequests { get; set; } = new List<BloodRequest>();
    }

