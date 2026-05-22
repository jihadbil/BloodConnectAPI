using AutoMapper;
using BloodConnectAPI.DataAccess.Repositories.Interfaces;
using BloodConnectAPI.Models;
using BloodConnectAPI.Models.DTOs;
using BloodConnectAPI.Models.Enums;
using BloodConnectAPI.Service.Common;
using BloodConnectAPI.Service.Interfaces;
using Microsoft.AspNetCore.Http;

namespace BloodConnectAPI.Service.Implementations;

public class DonorMedicalDocumentService : IDonorMedicalDocumentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;

    public DonorMedicalDocumentService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService)
    {
        _unitOfWork          = unitOfWork;
        _mapper              = mapper;
        _notificationService = notificationService;
    }

    public async Task<ServiceResponse<DonorMedicalDocumentDto>> UploadDocumentAsync(SubmitMedicalDocumentDto dto, string uploadsRootPath, string uploadedByUserId)
    {
        var donor = await _unitOfWork.Donors.GetByIdAsync(dto.DonorID);
        if (donor == null)
            return ServiceResponse<DonorMedicalDocumentDto>.FailureResponse("المتبرع غير موجود");

        if (dto.File == null || dto.File.Length == 0)
            return ServiceResponse<DonorMedicalDocumentDto>.FailureResponse("لم يتم توفير ملف");

        // Validate file type (e.g. PDF, Images)
        var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
        var extension = Path.GetExtension(dto.File.FileName).ToLower();
        if (!allowedExtensions.Contains(extension))
        {
            return ServiceResponse<DonorMedicalDocumentDto>.FailureResponse("نوع الملف غير مسموح به. المسموح: PDF, JPG, PNG");
        }

        // إنشاء مجلد wwwroot/uploads/medical-documents/ إذا لم يكن موجوداً
        var targetDirectory = Path.Combine(uploadsRootPath, "uploads", "medical-documents");
        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        // إنشاء اسم فريد
        var fileName = $"{dto.DonorID}_{dto.DocumentType}_{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(targetDirectory, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await dto.File.CopyToAsync(stream);
        }

        // المسار النسبي لحفظه في قاعدة البيانات
        var relativeFilePath = $"/uploads/medical-documents/{fileName}";

        var document = new DonorMedicalDocument
        {
            DonorID = dto.DonorID,
            DocumentType = dto.DocumentType,
            FilePath = relativeFilePath,
            UploadedAt = DateTime.UtcNow,
            IsVerified = false,
            Notes = null
        };

        await _unitOfWork.DonorMedicalDocuments.AddAsync(document);
        
        // إذا كانت donor.ApprovalStatus == PendingDocuments → تحديثها إلى PendingApproval
        if (donor.ApprovalStatus == DonorApprovalStatus.PendingDocuments)
        {
            donor.ApprovalStatus = DonorApprovalStatus.PendingApproval;
            await _unitOfWork.Donors.UpdateAsync(donor);
        }
        
        await _unitOfWork.SaveChangesAsync();

        // إشعار المدير بوثيقة طبية جديدة بحاجة إلى مراجعة
        await _notificationService.CreateForRoleAsync(
            role: "Admin",
            title: "وثيقة طبية جديدة",
            message: $"رفع المتبرع '{donor.FullName}' وثيقة '{dto.DocumentType}' بحاجة إلى مراجعة",
            type: NotificationType.NewMedicalDocument,
            relatedEntityType: "DonorMedicalDocument",
            relatedEntityId: document.DocumentID);

        var documentDto = _mapper.Map<DonorMedicalDocumentDto>(document);
        return ServiceResponse<DonorMedicalDocumentDto>.SuccessResponse(documentDto, "تم رفع الوثيقة بنجاح");
    }

    public async Task<ServiceResponse<DonorMedicalDocumentDto>> GetByIdAsync(int documentId)
    {
        var document = await _unitOfWork.DonorMedicalDocuments.GetByIdAsync(documentId);
        if (document == null)
            return ServiceResponse<DonorMedicalDocumentDto>.FailureResponse("الوثيقة غير موجودة");

        var dto = _mapper.Map<DonorMedicalDocumentDto>(document);
        return ServiceResponse<DonorMedicalDocumentDto>.SuccessResponse(dto);
    }

    public async Task<ServiceResponse<IEnumerable<DonorMedicalDocumentDto>>> GetByDonorIdAsync(int donorId)
    {
        var donor = await _unitOfWork.Donors.GetByIdAsync(donorId);
        if (donor == null)
            return ServiceResponse<IEnumerable<DonorMedicalDocumentDto>>.FailureResponse("المتبرع غير موجود");

        var documents = await _unitOfWork.DonorMedicalDocuments.FindAsync(d => d.DonorID == donorId);
        var dtos = _mapper.Map<IEnumerable<DonorMedicalDocumentDto>>(documents.OrderByDescending(d => d.UploadedAt));

        return ServiceResponse<IEnumerable<DonorMedicalDocumentDto>>.SuccessResponse(dtos);
    }

    public async Task<ServiceResponse<DonorMedicalDocumentDto>> VerifyDocumentAsync(int documentId, VerifyMedicalDocumentDto dto, string verifiedByUserId)
    {
        var document = await _unitOfWork.DonorMedicalDocuments.GetByIdAsync(documentId);
        if (document == null)
            return ServiceResponse<DonorMedicalDocumentDto>.FailureResponse("الوثيقة غير موجودة");

        document.IsVerified = dto.IsVerified;
        document.Notes = dto.Notes;
        
        await _unitOfWork.DonorMedicalDocuments.UpdateAsync(document);
        await _unitOfWork.SaveChangesAsync();

        var documentDto = _mapper.Map<DonorMedicalDocumentDto>(document);
        return ServiceResponse<DonorMedicalDocumentDto>.SuccessResponse(documentDto, "تم التحقق من الوثيقة بنجاح");
    }

    public async Task<ServiceResponse<bool>> DeleteAsync(int documentId, string uploadsRootPath)
    {
        var document = await _unitOfWork.DonorMedicalDocuments.GetByIdAsync(documentId);
        if (document == null)
            return ServiceResponse<bool>.FailureResponse("الوثيقة غير موجودة");

        // حذف الملف الفعلي من القرص
        if (!string.IsNullOrEmpty(document.FilePath))
        {
            var absolutePath = Path.Combine(uploadsRootPath, document.FilePath.TrimStart('/'));
            // تصحيح فواصل المسار للويندوز
            absolutePath = absolutePath.Replace("/", "\\");
            
            if (File.Exists(absolutePath))
            {
                File.Delete(absolutePath);
            }
        }

        await _unitOfWork.DonorMedicalDocuments.DeleteAsync(document);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResponse<bool>.SuccessResponse(true, "تم حذف الوثيقة بنجاح");
    }
}
