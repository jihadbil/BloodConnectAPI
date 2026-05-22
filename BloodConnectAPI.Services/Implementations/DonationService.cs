using AutoMapper;
using BloodConnectAPI.DataAccess.Repositories.Interfaces;
using BloodConnectAPI.Models;
using BloodConnectAPI.Models.DTOs;
using BloodConnectAPI.Models.Enums;
using BloodConnectAPI.Service.Common;
using BloodConnectAPI.Service.Interfaces;

namespace BloodConnectAPI.Service.Implementations;

/// <summary>
/// خدمة إدارة التبرعات
/// </summary>
public class DonationService : IDonationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IInventoryService _inventoryService;
    private readonly IMapper _mapper;

    public DonationService(IUnitOfWork unitOfWork, IInventoryService inventoryService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _inventoryService = inventoryService;
        _mapper = mapper;
    }

    public async Task<ServiceResponse<DonationDto>> GetByIdAsync(int id)
    {
        var donation = await _unitOfWork.Donations.GetByIdAsync(id);
        
        if (donation == null)
            return ServiceResponse<DonationDto>.FailureResponse("التبرع غير موجود");

        var donationDto = _mapper.Map<DonationDto>(donation);
        return ServiceResponse<DonationDto>.SuccessResponse(donationDto);
    }

    public async Task<ServiceResponse<PagedResult<DonationDto>>> GetAllAsync(PaginationParams pagination)
    {
        var (donations, totalCount) = await _unitOfWork.Donations.GetPagedAsync(
            pagination.PageNumber,
            pagination.PageSize,
            orderBy: query => pagination.SortDescending ?
                     query.OrderByDescending(d => d.DonationDate) :
                     query.OrderBy(d => d.DonationDate)
        );

        var donationDtos = _mapper.Map<IEnumerable<DonationDto>>(donations);
        var result = new PagedResult<DonationDto>
        {
            Items = donationDtos,
            TotalCount = totalCount,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };

        return ServiceResponse<PagedResult<DonationDto>>.SuccessResponse(result);
    }

    public async Task<ServiceResponse<DonationDto>> CreateAsync(CreateDonationDto dto)
    {
        // 1. التحقق من وجود المتبرع
        var donor = await _unitOfWork.Donors.GetByIdAsync(dto.DonorID);
        if (donor == null)
            return ServiceResponse<DonationDto>.FailureResponse("المتبرع غير موجود");

        if (!donor.IsActive)
            return ServiceResponse<DonationDto>.FailureResponse("المتبرع غير نشط");

        // 2. التحقق من العمر
        var ageValidation = DonationBusinessRules.ValidateDonorAge(donor.DateOfBirth);
        if (!ageValidation.IsValid)
            return ServiceResponse<DonationDto>.FailureResponse(ageValidation.ErrorMessage);

        // 3. التحقق من الفترة بين التبرعات (مع مراعاة الجنس)
        var intervalValidation = DonationBusinessRules.ValidateDonationInterval(
            donor.LastDonationDate,
            donor.Gender
        );
        if (!intervalValidation.IsValid)
            return ServiceResponse<DonationDto>.FailureResponse(intervalValidation.ErrorMessage);

        // 4. التحقق من الكمية
        var quantityValidation = DonationBusinessRules.ValidateDonationQuantity(dto.Quantity);
        if (!quantityValidation.IsValid)
            return ServiceResponse<DonationDto>.FailureResponse(quantityValidation.ErrorMessage);

        // 5. استخدام Transaction لضمان سلامة البيانات
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // إنشاء التبرع
            var donation = _mapper.Map<Donation>(dto);
            donation.DonationDate = dto.DonationDate != default ? dto.DonationDate : DateTime.UtcNow;
            donation.TestResult = TestResult.Pending;
            donation.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Donations.AddAsync(donation);

            // تحديث آخر تاريخ تبرع للمتبرع
            donor.LastDonationDate = DateTime.UtcNow;
            await _unitOfWork.Donors.UpdateAsync(donor);

            await _unitOfWork.CommitTransactionAsync();

            var donationDto = _mapper.Map<DonationDto>(donation);
            return ServiceResponse<DonationDto>.SuccessResponse(
                donationDto,
                "تم تسجيل التبرع بنجاح. في انتظار نتيجة الفحص"
            );
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<ServiceResponse<DonationDto>> UpdateAsync(int id, UpdateDonationDto dto)
    {
        var existing = await _unitOfWork.Donations.GetByIdAsync(id);
        if (existing == null)
            return ServiceResponse<DonationDto>.FailureResponse("التبرع غير موجود");

        // Update only non-null properties
        if (dto.DonationDate.HasValue) existing.DonationDate = dto.DonationDate.Value;
        if (dto.Quantity.HasValue) existing.Quantity = dto.Quantity.Value;
        if (dto.TestResult.HasValue) existing.TestResult = dto.TestResult.Value;
        if (dto.Notes != null) existing.Notes = dto.Notes;
        
        if (dto.TestNotes != null) existing.TestNotes = dto.TestNotes;
        if (dto.IsAddedToInventory.HasValue) existing.IsAddedToInventory = dto.IsAddedToInventory.Value;
        
        existing.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Donations.UpdateAsync(existing);
        await _unitOfWork.SaveChangesAsync();

        var donationDto = _mapper.Map<DonationDto>(existing);
        return ServiceResponse<DonationDto>.SuccessResponse(donationDto, "تم تحديث البيانات بنجاح");
    }

    public async Task<ServiceResponse<DonationDto>> UpdateTestResultAsync(int id, TestResult testResult)
    {
        var donation = await _unitOfWork.Donations.GetByIdAsync(id);
        if (donation == null)
            return ServiceResponse<DonationDto>.FailureResponse("التبرع غير موجود");

        donation.TestResult = testResult;
        donation.TestedAt = DateTime.UtcNow;
        donation.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Donations.UpdateAsync(donation);
        await _unitOfWork.SaveChangesAsync();

        // إذا كانت النتيجة مقبولة، إضافة للمخزون مع تتبع الصلاحية
        if (testResult == TestResult.Accepted)
        {
            var inventoryResult = await _inventoryService.AddToInventoryAsync(
                donation.DonationID,
                donation.BloodTypeID,
                donation.Quantity
            );

            if (!inventoryResult.IsSuccess)
            {
                return ServiceResponse<DonationDto>.FailureResponse(
                    $"تم قبول التبرع لكن فشل إضافته للمخزون: {inventoryResult.Message}"
                );
            }
            
            donation.IsAddedToInventory = true;
            await _unitOfWork.Donations.UpdateAsync(donation);
            await _unitOfWork.SaveChangesAsync();
        }

        var donationDto = _mapper.Map<DonationDto>(donation);
        return ServiceResponse<DonationDto>.SuccessResponse(
            donationDto,
            $"تم تحديث نتيجة الفحص إلى {testResult}"
        );
    }

    public async Task<ServiceResponse<bool>> DeleteAsync(int id)
    {
        var donation = await _unitOfWork.Donations.GetByIdAsync(id);
        if (donation == null)
            return ServiceResponse<bool>.FailureResponse("التبرع غير موجود");

        await _unitOfWork.Donations.DeleteAsync(donation);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResponse<bool>.SuccessResponse(true, "تم حذف التبرع بنجاح");
    }

    public async Task<ServiceResponse<IEnumerable<DonationDto>>> GetByDonorIdAsync(int donorId)
    {
        var donations = await _unitOfWork.Donations.GetByDonorIdAsync(donorId);
        var donationDtos = _mapper.Map<IEnumerable<DonationDto>>(donations);
        return ServiceResponse<IEnumerable<DonationDto>>.SuccessResponse(donationDtos);
    }

    public async Task<ServiceResponse<IEnumerable<DonationDto>>> GetRecentDonationsAsync(int days = 30)
    {
        var donations = await _unitOfWork.Donations.FindAsync(d => d.DonationDate >= DateTime.UtcNow.AddDays(-days));
        var donationDtos = _mapper.Map<IEnumerable<DonationDto>>(donations.OrderByDescending(d => d.DonationDate));
        return ServiceResponse<IEnumerable<DonationDto>>.SuccessResponse(donationDtos);
    }

    public async Task<ServiceResponse<DonationDto>> LabTestDonationAsync(int donationId, LabTestDonationDto dto, string testedByUserId)
    {
        var donation = await _unitOfWork.Donations.GetByIdAsync(donationId);
        if (donation == null)
            return ServiceResponse<DonationDto>.FailureResponse("التبرع غير موجود");

        if (donation.TestResult != TestResult.Pending)
            return ServiceResponse<DonationDto>.FailureResponse("تم فحص هذا التبرع مسبقاً");

        donation.TestResult = dto.TestResult;
        donation.TestNotes = dto.TestNotes;
        donation.TestedAt = DateTime.UtcNow;
        donation.TestedByUserId = testedByUserId;
        donation.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Donations.UpdateAsync(donation);
        await _unitOfWork.SaveChangesAsync();

        if (dto.TestResult == TestResult.Accepted && dto.AddToInventoryIfAccepted)
        {
            var inventoryResult = await _inventoryService.AddToInventoryAsync(
                donation.DonationID,
                donation.BloodTypeID,
                donation.Quantity
            );

            if (!inventoryResult.IsSuccess)
            {
                return ServiceResponse<DonationDto>.FailureResponse(
                    $"تم الفحص كصالح لكن فشل إضافته للمخزون: {inventoryResult.Message}"
                );
            }
            
            donation.IsAddedToInventory = true;
            await _unitOfWork.Donations.UpdateAsync(donation);
            await _unitOfWork.SaveChangesAsync();
        }

        var donationDto = _mapper.Map<DonationDto>(donation);
        return ServiceResponse<DonationDto>.SuccessResponse(
            donationDto,
            dto.TestResult == TestResult.Accepted ? "تم تسجيل النتائج (مقبول وتم الإضافة للمخزون)" : "تم تسجيل النتائج (مرفوض)"
        );
    }
}
