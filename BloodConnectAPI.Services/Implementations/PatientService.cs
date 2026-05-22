using AutoMapper;
using BloodConnectAPI.DataAccess.Repositories.Interfaces;
using BloodConnectAPI.Models;
using BloodConnectAPI.Models.DTOs;
using BloodConnectAPI.Service.Common;
using BloodConnectAPI.Service.Interfaces;

namespace BloodConnectAPI.Service.Implementations;

/// <summary>
/// خدمة إدارة المرضى
/// </summary>
public class PatientService : IPatientService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PatientService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ServiceResponse<PatientDto>> GetByIdAsync(int id)
    {
        var patient = await _unitOfWork.Patients.GetByIdAsync(id);
        
        if (patient == null)
            return ServiceResponse<PatientDto>.FailureResponse("المريض غير موجود");

        var patientDto = _mapper.Map<PatientDto>(patient);
        return ServiceResponse<PatientDto>.SuccessResponse(patientDto);
    }

    public async Task<ServiceResponse<PagedResult<PatientDto>>> GetAllAsync(PaginationParams pagination)
    {
        var (patients, totalCount) = await _unitOfWork.Patients.GetPagedAsync(
            pagination.PageNumber,
            pagination.PageSize,
            filter: string.IsNullOrEmpty(pagination.SearchTerm) ? null :
                    p => p.FullName.Contains(pagination.SearchTerm) ||
                         p.NationalID.Contains(pagination.SearchTerm) ||
                         p.Phone.Contains(pagination.SearchTerm),
            orderBy: query => pagination.SortDescending ?
                     query.OrderByDescending(p => p.CreatedAt) :
                     query.OrderBy(p => p.CreatedAt)
        );

        var patientDtos = _mapper.Map<IEnumerable<PatientDto>>(patients);
        var result = new PagedResult<PatientDto>
        {
            Items = patientDtos,
            TotalCount = totalCount,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };

        return ServiceResponse<PagedResult<PatientDto>>.SuccessResponse(result);
    }

    public async Task<ServiceResponse<PatientDto>> CreateAsync(CreatePatientDto dto)
    {
        // Validation
        if (await _unitOfWork.Patients.ExistsAsync(p => p.NationalID == dto.NationalID))
            return ServiceResponse<PatientDto>.FailureResponse("الرقم الوطني مسجل مسبقاً");

        var patient = _mapper.Map<Patient>(dto);
        patient.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Patients.AddAsync(patient);
        await _unitOfWork.SaveChangesAsync();

        var patientDto = _mapper.Map<PatientDto>(patient);
        return ServiceResponse<PatientDto>.SuccessResponse(patientDto, "تم إضافة المريض بنجاح");
    }

    public async Task<ServiceResponse<PatientDto>> UpdateAsync(int id, UpdatePatientDto dto)
    {
        var existing = await _unitOfWork.Patients.GetByIdAsync(id);
        if (existing == null)
            return ServiceResponse<PatientDto>.FailureResponse("المريض غير موجود");

        // Update only non-null properties
        if (dto.FullName != null) existing.FullName = dto.FullName;
        if (dto.Phone != null) existing.Phone = dto.Phone;
        if (dto.City != null) existing.City = dto.City;
        existing.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Patients.UpdateAsync(existing);
        await _unitOfWork.SaveChangesAsync();

        var patientDto = _mapper.Map<PatientDto>(existing);
        return ServiceResponse<PatientDto>.SuccessResponse(patientDto, "تم تحديث البيانات بنجاح");
    }

    public async Task<ServiceResponse<bool>> DeleteAsync(int id)
    {
        var patient = await _unitOfWork.Patients.GetByIdAsync(id);
        if (patient == null)
            return ServiceResponse<bool>.FailureResponse("المريض غير موجود");

        await _unitOfWork.Patients.DeleteAsync(patient);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResponse<bool>.SuccessResponse(true, "تم حذف المريض بنجاح");
    }

    public async Task<ServiceResponse<PatientDto>> GetByNationalIdAsync(string nationalId)
    {
        var patient = await _unitOfWork.Patients.GetByNationalIdAsync(nationalId);
        
        if (patient == null)
            return ServiceResponse<PatientDto>.FailureResponse("المريض غير موجود");

        var patientDto = _mapper.Map<PatientDto>(patient);
        return ServiceResponse<PatientDto>.SuccessResponse(patientDto);
    }

    public async Task<ServiceResponse<PatientDto>> GetWithRequestsAsync(int id)
    {
        var patient = await _unitOfWork.Patients.GetWithRequestsAsync(id);
        
        if (patient == null)
            return ServiceResponse<PatientDto>.FailureResponse("المريض غير موجود");

        var patientDto = _mapper.Map<PatientDto>(patient);
        return ServiceResponse<PatientDto>.SuccessResponse(patientDto);
    }
}
