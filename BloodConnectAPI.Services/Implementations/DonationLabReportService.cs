using AutoMapper;
using BloodConnectAPI.DataAccess.Repositories.Interfaces;
using BloodConnectAPI.Models;
using BloodConnectAPI.Models.DTOs;
using BloodConnectAPI.Models.Enums;
using BloodConnectAPI.Service.Common;
using BloodConnectAPI.Service.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BloodConnectAPI.Service.Implementations;

public class DonationLabReportService : IDonationLabReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;

    public DonationLabReportService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notificationService = notificationService;
    }

    public async Task<ServiceResponse<DonationLabReportDto>> UploadAsync(UploadDonationLabReportDto dto, string uploadsRootPath, string uploadedByUserId)
    {
        // 1. التحقق من وجود التبرع
        var donation = await _unitOfWork.Donations.GetByIdAsync(dto.DonationID);
        if (donation == null)
            return ServiceResponse<DonationLabReportDto>.FailureResponse("التبرع غير موجود");

        if (dto.File == null || dto.File.Length == 0)
            return ServiceResponse<DonationLabReportDto>.FailureResponse("لم يتم توفير ملف");

        // 2. التحقق من نوع الملف (pdf, images)
        var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
        var extension = Path.GetExtension(dto.File.FileName).ToLower();
        if (!allowedExtensions.Contains(extension))
        {
            return ServiceResponse<DonationLabReportDto>.FailureResponse("نوع الملف غير مسموح به. المسموح: PDF, JPG, PNG");
        }

        // 3. إنشاء مجلد wwwroot/uploads/lab-reports/ إذا لم يكن موجوداً
        var targetDirectory = Path.Combine(uploadsRootPath, "uploads", "lab-reports");
        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        // 4. إنشاء اسم فريد للملف
        var fileName = $"{dto.DonationID}_{dto.ReportType}_{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(targetDirectory, fileName);

        // 5. حفظ الملف على القرص
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await dto.File.CopyToAsync(stream);
        }

        // المسار النسبي لحفظه في قاعدة البيانات
        var relativeFilePath = $"/uploads/lab-reports/{fileName}";

        // 6. إنشاء سجل DonationLabReport
        var report = new DonationLabReport
        {
            DonationID = dto.DonationID,
            ReportType = dto.ReportType,
            FilePath = relativeFilePath,
            UploadedAt = DateTime.UtcNow,
            UploadedByUserId = uploadedByUserId,
            Notes = dto.Notes
        };

        await _unitOfWork.DonationLabReports.AddAsync(report);
        await _unitOfWork.SaveChangesAsync();

        // 7. إرسال إشعار للمدير بتقرير تحليل جديد
        try
        {
            await _notificationService.CreateForRoleAsync(
                role: "Admin",
                title: "تقرير تحليل مخبري جديد",
                message: $"تم رفع تقرير تحليل جديد '{dto.ReportType}' للتبرع رقم {dto.DonationID}",
                type: NotificationType.NewLabReport,
                relatedEntityType: "DonationLabReport",
                relatedEntityId: report.LabReportID);
        }
        catch (Exception)
        {
            // لا تعطل العملية إذا فشل إرسال الإشعار
        }

        var reportDto = _mapper.Map<DonationLabReportDto>(report);
        return ServiceResponse<DonationLabReportDto>.SuccessResponse(reportDto, "تم رفع تقرير التحليل بنجاح");
    }

    public async Task<ServiceResponse<IEnumerable<DonationLabReportDto>>> GetByDonationIdAsync(int donationId)
    {
        var donation = await _unitOfWork.Donations.GetByIdAsync(donationId);
        if (donation == null)
            return ServiceResponse<IEnumerable<DonationLabReportDto>>.FailureResponse("التبرع غير موجود");

        var reports = await _unitOfWork.DonationLabReports.FindAsync(r => r.DonationID == donationId);
        var dtos = _mapper.Map<IEnumerable<DonationLabReportDto>>(reports.OrderByDescending(r => r.UploadedAt));

        return ServiceResponse<IEnumerable<DonationLabReportDto>>.SuccessResponse(dtos);
    }

    public async Task<ServiceResponse<DonationLabReportDto>> GetByIdAsync(int reportId)
    {
        var report = await _unitOfWork.DonationLabReports.GetByIdAsync(reportId);
        if (report == null)
            return ServiceResponse<DonationLabReportDto>.FailureResponse("تقرير التحليل غير موجود");

        var dto = _mapper.Map<DonationLabReportDto>(report);
        return ServiceResponse<DonationLabReportDto>.SuccessResponse(dto);
    }

    public async Task<ServiceResponse<bool>> DeleteAsync(int reportId, string uploadsRootPath)
    {
        var report = await _unitOfWork.DonationLabReports.GetByIdAsync(reportId);
        if (report == null)
            return ServiceResponse<bool>.FailureResponse("تقرير التحليل غير موجود");

        // حذف الملف الفعلي من القرص
        if (!string.IsNullOrEmpty(report.FilePath))
        {
            var absolutePath = Path.Combine(uploadsRootPath, report.FilePath.TrimStart('/'));
            // تصحيح فواصل المسار للويندوز
            absolutePath = absolutePath.Replace("/", "\\");

            if (File.Exists(absolutePath))
            {
                File.Delete(absolutePath);
            }
        }

        await _unitOfWork.DonationLabReports.DeleteAsync(report);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResponse<bool>.SuccessResponse(true, "تم حذف تقرير التحليل بنجاح");
    }
}
