using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace BloodConnectAPI.Models.DTOs;

/// <summary>
/// DTO لعرض بيانات تقرير تحليل التبرع
/// </summary>
public class DonationLabReportDto
{
    public int LabReportID { get; set; }
    public int DonationID { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string UploadedByUserId { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO لرفع تقرير تحليل تبرع جديد
/// </summary>
public class UploadDonationLabReportDto
{
    [Required(ErrorMessage = "يجب تحديد رقم التبرع")]
    public int DonationID { get; set; }

    [Required(ErrorMessage = "يجب تحديد نوع التحليل")]
    public string ReportType { get; set; } = string.Empty;

    [Required(ErrorMessage = "يجب تحميل الملف")]
    public IFormFile File { get; set; } = null!;

    public string? Notes { get; set; }
}
