using AutoMapper;
using Microsoft.AspNetCore.Identity;
using BloodConnectAPI.DataAccess.Repositories.Interfaces;
using BloodConnectAPI.Models;
using BloodConnectAPI.Models.DTOs;
using BloodConnectAPI.Models.Enums;
using BloodConnectAPI.Service.Common;
using BloodConnectAPI.Service.Interfaces;

namespace BloodConnectAPI.Service.Implementations;

/// <summary>
/// خدمة إدارة المتبرعين
/// </summary>
public class DonorService : IDonorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;

    public DonorService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _userManager = userManager;
    }

    public async Task<ServiceResponse<DonorDto>> GetByIdAsync(int id)
    {
        var donor = await _unitOfWork.Donors.GetByIdAsync(id);
        
        if (donor == null)
            return ServiceResponse<DonorDto>.FailureResponse("المتبرع غير موجود");

        var donorDto = _mapper.Map<DonorDto>(donor);
        return ServiceResponse<DonorDto>.SuccessResponse(donorDto);
    }

    public async Task<ServiceResponse<DonorDto>> GetByUserIdAsync(string userId)
    {
        var donors = await _unitOfWork.Donors.FindAsync(d => d.UserId == userId);
        var donor = donors.FirstOrDefault();

        if (donor == null)
            return ServiceResponse<DonorDto>.FailureResponse("لا يوجد سجل متبرع مرتبط بهذا المستخدم");

        var donorDto = _mapper.Map<DonorDto>(donor);
        return ServiceResponse<DonorDto>.SuccessResponse(donorDto);
    }

    public async Task<ServiceResponse<PagedResult<DonorDto>>> GetAllAsync(PaginationParams pagination)
    {
        var (donors, totalCount) = await _unitOfWork.Donors.GetPagedAsync(
            pagination.PageNumber,
            pagination.PageSize,
            filter: string.IsNullOrEmpty(pagination.SearchTerm) ? null :
                    d => d.FullName.Contains(pagination.SearchTerm) ||
                         d.NationalID.Contains(pagination.SearchTerm) ||
                         d.Phone.Contains(pagination.SearchTerm),
            orderBy: query => pagination.SortDescending ?
                     query.OrderByDescending(d => d.CreatedAt) :
                     query.OrderBy(d => d.CreatedAt)
        );

        var donorDtos = _mapper.Map<IEnumerable<DonorDto>>(donors);
        var result = new PagedResult<DonorDto>
        {
            Items = donorDtos,
            TotalCount = totalCount,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };

        return ServiceResponse<PagedResult<DonorDto>>.SuccessResponse(result);
    }

    public async Task<ServiceResponse<DonorDto>> CreateAsync(CreateDonorDto dto)
    {
        var errors = new List<string>();

        if (await _unitOfWork.Donors.ExistsAsync(d => d.NationalID == dto.NationalID))
            errors.Add("الرقم الوطني مسجل مسبقاً");

        var ageValidation = DonationBusinessRules.ValidateDonorAge(dto.DateOfBirth);
        if (!ageValidation.IsValid)
            errors.Add(ageValidation.ErrorMessage);

        if (!string.IsNullOrEmpty(dto.UserId))
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
                errors.Add("المستخدم غير موجود");
            else
            {
                var existingLink = await _unitOfWork.Donors.FirstOrDefaultAsync(d => d.UserId == dto.UserId);
                if (existingLink != null)
                    errors.Add("هذا المستخدم مرتبط بمتبرع آخر");
            }
        }

        if (errors.Any())
            return ServiceResponse<DonorDto>.FailureResponse("فشل التحقق من البيانات", errors);

        var donor = _mapper.Map<Donor>(dto);
        donor.ApprovalStatus = dto.ApprovalStatus;
        donor.IsActive = (dto.ApprovalStatus == DonorApprovalStatus.RequestMoreDocs);
        donor.CreatedAt = DateTime.UtcNow;

        try
        {
            await _unitOfWork.Donors.AddAsync(donor);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
        {
            if (ex.InnerException?.Message.Contains("UNIQUE") == true)
                return ServiceResponse<DonorDto>.FailureResponse("هذا المستخدم مرتبط بمتبرع آخر");
            if (ex.InnerException?.Message.Contains("FOREIGN KEY") == true)
                return ServiceResponse<DonorDto>.FailureResponse("المستخدم غير موجود");
            throw;
        }

        var donorDto = _mapper.Map<DonorDto>(donor);
        return ServiceResponse<DonorDto>.SuccessResponse(donorDto, "تم إضافة المتبرع بنجاح");
    }

    public async Task<ServiceResponse<DonorDto>> UpdateAsync(int id, UpdateDonorDto dto)
    {
        var existing = await _unitOfWork.Donors.GetByIdAsync(id);
        if (existing == null)
            return ServiceResponse<DonorDto>.FailureResponse("المتبرع غير موجود");

        var errors = new List<string>();
        
        if (!string.IsNullOrEmpty(dto.UserId) && dto.UserId != existing.UserId)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
                errors.Add("المستخدم غير موجود");
            else
            {
                var existingLink = await _unitOfWork.Donors.FirstOrDefaultAsync(d => d.UserId == dto.UserId && d.DonorID != id);
                if (existingLink != null)
                    errors.Add("هذا المستخدم مرتبط بمتبرع آخر");
            }
        }
        
        if (errors.Any())
            return ServiceResponse<DonorDto>.FailureResponse("فشل التحقق من البيانات", errors);

        if (dto.FullName != null) existing.FullName = dto.FullName;
        if (dto.NationalID != null) existing.NationalID = dto.NationalID;
        if (dto.Gender.HasValue) existing.Gender = dto.Gender.Value;
        if (dto.DateOfBirth.HasValue) existing.DateOfBirth = dto.DateOfBirth.Value;
        if (dto.Phone != null) existing.Phone = dto.Phone;
        if (dto.BloodTypeID.HasValue) existing.BloodTypeID = dto.BloodTypeID.Value;
        if (dto.City != null) existing.City = dto.City;
        if (dto.IsActive.HasValue) existing.IsActive = dto.IsActive.Value;
        if (dto.ApprovalStatus.HasValue) existing.ApprovalStatus = dto.ApprovalStatus.Value;
        
        if (dto.UserId != null && dto.UserId != existing.UserId)
        {
            existing.UserId = dto.UserId;
        }
        
        existing.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _unitOfWork.Donors.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
        {
            if (ex.InnerException?.Message.Contains("UNIQUE") == true)
                return ServiceResponse<DonorDto>.FailureResponse("هذا المستخدم مرتبط بمتبرع آخر");
            if (ex.InnerException?.Message.Contains("FOREIGN KEY") == true)
                return ServiceResponse<DonorDto>.FailureResponse("المستخدم غير موجود");
            throw;
        }

        var donorDto = _mapper.Map<DonorDto>(existing);
        return ServiceResponse<DonorDto>.SuccessResponse(donorDto, "تم تحديث البيانات بنجاح");
    }

    public async Task<ServiceResponse<bool>> DeleteAsync(int id)
    {
        var donor = await _unitOfWork.Donors.GetByIdAsync(id);
        if (donor == null)
            return ServiceResponse<bool>.FailureResponse("المتبرع غير موجود");

        await _unitOfWork.Donors.DeleteAsync(donor);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResponse<bool>.SuccessResponse(true, "تم حذف المتبرع بنجاح");
    }

    public async Task<ServiceResponse<DonorDto>> GetByNationalIdAsync(string nationalId)
    {
        var donor = await _unitOfWork.Donors.GetByNationalIdAsync(nationalId);
        
        if (donor == null)
            return ServiceResponse<DonorDto>.FailureResponse("المتبرع غير موجود");

        var donorDto = _mapper.Map<DonorDto>(donor);
        return ServiceResponse<DonorDto>.SuccessResponse(donorDto);
    }

    public async Task<ServiceResponse<IEnumerable<DonorDto>>> GetEligibleDonorsAsync(int? bloodTypeId = null)
    {
        var donors = await _unitOfWork.Donors.GetEligibleDonorsAsync();

        if (bloodTypeId.HasValue)
            donors = donors.Where(d => d.BloodTypeID == bloodTypeId.Value);

        var donorDtos = _mapper.Map<IEnumerable<DonorDto>>(donors);
        return ServiceResponse<IEnumerable<DonorDto>>.SuccessResponse(
            donorDtos,
            $"تم جلب {donorDtos.Count()} متبرع مؤهل"
        );
    }

    public async Task<ServiceResponse<bool>> CheckEligibilityAsync(int donorId)
    {
        var donor = await _unitOfWork.Donors.GetByIdAsync(donorId);
        if (donor == null)
            return ServiceResponse<bool>.FailureResponse("المتبرع غير موجود");

        if (!donor.IsActive || donor.ApprovalStatus != DonorApprovalStatus.Approved)
            return ServiceResponse<bool>.FailureResponse("المتبرع غير نشط أو لم تتم الموافقة عليه");

        var lastDonation = (await _unitOfWork.Donations.FindAsync(d => d.DonorID == donorId))
            .OrderByDescending(d => d.DonationDate)
            .FirstOrDefault();

        if (lastDonation != null && lastDonation.DonationDate.AddMonths(3) > DateTime.UtcNow)
        {
            var nextEligibleDate = lastDonation.DonationDate.AddMonths(3);
            return ServiceResponse<bool>.FailureResponse(
                $"لا يمكن التبرع قبل {nextEligibleDate:yyyy-MM-dd}"
            );
        }

        return ServiceResponse<bool>.SuccessResponse(true, "المتبرع مؤهل للتبرع");
    }

    public async Task<ServiceResponse<DonorDto>> GetWithDonationsAsync(int id)
    {
        var donor = await _unitOfWork.Donors.GetWithDonationsAsync(id);
        
        if (donor == null)
            return ServiceResponse<DonorDto>.FailureResponse("المتبرع غير موجود");

        var donorDto = _mapper.Map<DonorDto>(donor);
        return ServiceResponse<DonorDto>.SuccessResponse(donorDto);
    }

    public async Task<ServiceResponse<DonorDto>> LinkDonorToUserAsync(int donorId, string userId)
    {
        var donor = await _unitOfWork.Donors.GetByIdAsync(donorId);
        if (donor == null)
            return ServiceResponse<DonorDto>.FailureResponse("المتبرع غير موجود");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return ServiceResponse<DonorDto>.FailureResponse("المستخدم غير موجود");

        var existingLink = await _unitOfWork.Donors.FirstOrDefaultAsync(d => d.UserId == userId);
        if (existingLink != null && existingLink.DonorID != donorId)
            return ServiceResponse<DonorDto>.FailureResponse("هذا المستخدم مرتبط بمتبرع آخر");

        donor.UserId = userId;
        donor.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _unitOfWork.Donors.UpdateAsync(donor);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
        {
            if (ex.InnerException?.Message.Contains("UNIQUE") == true)
                return ServiceResponse<DonorDto>.FailureResponse("هذا المستخدم مرتبط بمتبرع آخر");
            if (ex.InnerException?.Message.Contains("FOREIGN KEY") == true)
                return ServiceResponse<DonorDto>.FailureResponse("المستخدم غير موجود");
            throw;
        }

        var updatedDonor = await _unitOfWork.Donors.GetWithUserAsync(donorId);
        var donorDto = _mapper.Map<DonorDto>(updatedDonor);

        return ServiceResponse<DonorDto>.SuccessResponse(donorDto, "تم ربط المتبرع بالمستخدم بنجاح");
    }

    public async Task<ServiceResponse<DonorDto>> UnlinkDonorFromUserAsync(int donorId)
    {
        var donor = await _unitOfWork.Donors.GetByIdAsync(donorId);
        if (donor == null)
            return ServiceResponse<DonorDto>.FailureResponse("المتبرع غير موجود");

        if (donor.UserId == null)
            return ServiceResponse<DonorDto>.FailureResponse("المتبرع غير مرتبط بأي مستخدم");

        donor.UserId = null;
        donor.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Donors.UpdateAsync(donor);
        await _unitOfWork.SaveChangesAsync();

        var donorDto = _mapper.Map<DonorDto>(donor);
        return ServiceResponse<DonorDto>.SuccessResponse(donorDto, "تم فك ربط المتبرع من المستخدم بنجاح");
    }

    public async Task<ServiceResponse<DonorDto>> GetWithUserAsync(int donorId)
    {
        var donor = await _unitOfWork.Donors.GetWithUserAsync(donorId);
        
        if (donor == null)
            return ServiceResponse<DonorDto>.FailureResponse("المتبرع غير موجود");

        var donorDto = _mapper.Map<DonorDto>(donor);
        return ServiceResponse<DonorDto>.SuccessResponse(donorDto);
    }

    // Approval Workflow
    public async Task<ServiceResponse<DonorDto>> ApproveDonorAsync(int donorId, ApproveDonorDto dto, string reviewerUserId)
    {
        var donor = await _unitOfWork.Donors.GetByIdAsync(donorId);
        if (donor == null)
            return ServiceResponse<DonorDto>.FailureResponse("المتبرع غير موجود");

        donor.ApprovalStatus = dto.NewStatus;
        
        if (dto.NewStatus == DonorApprovalStatus.Rejected || dto.NewStatus == DonorApprovalStatus.RequestMoreDocs)
        {
            donor.RejectionReason = dto.RejectionReason;
        }

        if (dto.NewStatus == DonorApprovalStatus.Approved || dto.NewStatus == DonorApprovalStatus.Rejected)
        {
            donor.ApprovalDate = DateTime.UtcNow;
            if (dto.NewStatus == DonorApprovalStatus.Approved)
            {
                donor.IsActive = true;
            }
        }

        donor.ReviewedByUserId = reviewerUserId;
        donor.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Donors.UpdateAsync(donor);
        await _unitOfWork.SaveChangesAsync();

        var donorDto = _mapper.Map<DonorDto>(donor);
        return ServiceResponse<DonorDto>.SuccessResponse(donorDto, "تم تحديث حالة الموافقة بنجاح");
    }

    public async Task<ServiceResponse<PagedResult<DonorDto>>> GetByApprovalStatusAsync(DonorApprovalStatus status, PaginationParams pagination)
    {
        var (donors, totalCount) = await _unitOfWork.Donors.GetPagedAsync(
            pagination.PageNumber,
            pagination.PageSize,
            filter: d => d.ApprovalStatus == status,
            orderBy: query => pagination.SortDescending ?
                     query.OrderByDescending(d => d.CreatedAt) :
                     query.OrderBy(d => d.CreatedAt)
        );

        var donorDtos = _mapper.Map<IEnumerable<DonorDto>>(donors);
        var result = new PagedResult<DonorDto>
        {
            Items = donorDtos,
            TotalCount = totalCount,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };

        return ServiceResponse<PagedResult<DonorDto>>.SuccessResponse(result);
    }

    public async Task<ServiceResponse<IEnumerable<DonorMedicalDocumentDto>>> GetMedicalDocumentsAsync(int donorId)
    {
        var donor = await _unitOfWork.Donors.GetByIdAsync(donorId);
        if (donor == null)
            return ServiceResponse<IEnumerable<DonorMedicalDocumentDto>>.FailureResponse("المتبرع غير موجود");

        var documents = await _unitOfWork.DonorMedicalDocuments.FindAsync(d => d.DonorID == donorId);
        var dtos = _mapper.Map<IEnumerable<DonorMedicalDocumentDto>>(documents);
        
        return ServiceResponse<IEnumerable<DonorMedicalDocumentDto>>.SuccessResponse(dtos);
    }
}
