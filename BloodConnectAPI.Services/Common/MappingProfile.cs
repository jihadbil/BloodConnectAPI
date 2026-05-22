using AutoMapper;
using BloodConnectAPI.Models;
using BloodConnectAPI.Models.DTOs;

namespace BloodConnectAPI.Service.Common;

/// <summary>
/// تعريف جميع عمليات تحويل البيانات بين Models وDTOs
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ─────────────────────────────────────────
        // Donor Mappings
        // ─────────────────────────────────────────
        CreateMap<Donor, DonorDto>()
            .ForMember(dest => dest.BloodTypeName,
                opt => opt.MapFrom(src => src.BloodType != null ? src.BloodType.TypeName : string.Empty))
            .ForMember(dest => dest.Username,
                opt => opt.MapFrom(src => src.User != null ? src.User.UserName : null))
            .ForMember(dest => dest.UserEmail,
                opt => opt.MapFrom(src => src.User != null ? src.User.Email : null));

        CreateMap<CreateDonorDto, Donor>()
            .ForMember(dest => dest.DonorID,       opt => opt.Ignore())
            .ForMember(dest => dest.IsActive,       opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt,      opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt,      opt => opt.Ignore())
            .ForMember(dest => dest.LastDonationDate, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovalDate,   opt => opt.Ignore())
            .ForMember(dest => dest.ReviewedByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.RejectionReason, opt => opt.Ignore())
            .ForMember(dest => dest.BloodType,      opt => opt.Ignore())
            .ForMember(dest => dest.User,           opt => opt.Ignore())
            .ForMember(dest => dest.Donations,      opt => opt.Ignore())
            .ForMember(dest => dest.MedicalDocuments, opt => opt.Ignore())
            .ForMember(dest => dest.RequestResponses, opt => opt.Ignore());

        // ─────────────────────────────────────────
        // Patient Mappings
        // ─────────────────────────────────────────
        CreateMap<Patient, PatientDto>()
            .ForMember(dest => dest.BloodTypeName,
                opt => opt.MapFrom(src => src.BloodType != null ? src.BloodType.TypeName : string.Empty));

        CreateMap<CreatePatientDto, Patient>()
            .ForMember(dest => dest.PatientID,  opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt,  opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt,  opt => opt.Ignore())
            .ForMember(dest => dest.BloodType,  opt => opt.Ignore())
            .ForMember(dest => dest.BloodRequests, opt => opt.Ignore());

        // ─────────────────────────────────────────
        // Donation Mappings
        // ─────────────────────────────────────────
        CreateMap<Donation, DonationDto>()
            .ForMember(dest => dest.DonorName,
                opt => opt.MapFrom(src => src.Donor != null ? src.Donor.FullName : string.Empty))
            .ForMember(dest => dest.BloodTypeName,
                opt => opt.MapFrom(src => src.BloodType != null ? src.BloodType.TypeName : string.Empty));

        CreateMap<CreateDonationDto, Donation>()
            .ForMember(dest => dest.DonationID,         opt => opt.Ignore())
            .ForMember(dest => dest.TestResult,          opt => opt.Ignore())
            .ForMember(dest => dest.IsAddedToInventory,  opt => opt.Ignore())
            .ForMember(dest => dest.TestedAt,            opt => opt.Ignore())
            .ForMember(dest => dest.TestedByUserId,      opt => opt.Ignore())
            .ForMember(dest => dest.TestNotes,           opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt,           opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt,           opt => opt.Ignore())
            .ForMember(dest => dest.Donor,               opt => opt.Ignore())
            .ForMember(dest => dest.BloodType,           opt => opt.Ignore())
            .ForMember(dest => dest.BloodDisbursements,  opt => opt.Ignore());

        // ─────────────────────────────────────────
        // BloodRequest Mappings
        // ─────────────────────────────────────────
        CreateMap<BloodRequest, BloodRequestDto>()
            .ForMember(dest => dest.PatientName,
                opt => opt.MapFrom(src => src.Patient != null ? src.Patient.FullName : string.Empty))
            .ForMember(dest => dest.BloodTypeName,
                opt => opt.MapFrom(src => src.BloodType != null ? src.BloodType.TypeName : string.Empty));

        CreateMap<CreateBloodRequestDto, BloodRequest>()
            .ForMember(dest => dest.RequestID,          opt => opt.Ignore())
            .ForMember(dest => dest.RequestDate,        opt => opt.Ignore())
            .ForMember(dest => dest.Status,             opt => opt.Ignore())
            .ForMember(dest => dest.QuantityFulfilled,  opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt,          opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt,          opt => opt.Ignore())
            .ForMember(dest => dest.Patient,            opt => opt.Ignore())
            .ForMember(dest => dest.BloodType,          opt => opt.Ignore())
            .ForMember(dest => dest.DonorResponses,     opt => opt.Ignore());

        // ─────────────────────────────────────────
        // DonorRequestResponse Mappings
        // ─────────────────────────────────────────
        CreateMap<DonorRequestResponse, DonorResponseDto>()
            .ForMember(dest => dest.DonorName,
                opt => opt.MapFrom(src => src.Donor != null ? src.Donor.FullName : string.Empty))
            .ForMember(dest => dest.BloodTypeName,
                opt => opt.MapFrom(src =>
                    src.BloodRequest != null && src.BloodRequest.BloodType != null
                        ? src.BloodRequest.BloodType.TypeName
                        : string.Empty));

        // ─────────────────────────────────────────
        // BloodInventory Mappings
        // ─────────────────────────────────────────
        CreateMap<BloodInventory, BloodInventoryDto>()
            .ForMember(dest => dest.BloodTypeName,
                opt => opt.MapFrom(src => src.BloodType != null ? src.BloodType.TypeName : string.Empty))
            .ForMember(dest => dest.ItemsCount,
                opt => opt.MapFrom(src => src.InventoryItems != null ? src.InventoryItems.Count : 0));

        // ─────────────────────────────────────────
        // BloodInventoryItem Mappings
        // ─────────────────────────────────────────
        CreateMap<BloodInventoryItem, BloodInventoryItemDto>()
            .ForMember(dest => dest.DonorName,
                opt => opt.MapFrom(src =>
                    src.Donation != null && src.Donation.Donor != null
                        ? src.Donation.Donor.FullName
                        : string.Empty))
            .ForMember(dest => dest.BloodTypeName,
                opt => opt.MapFrom(src =>
                    src.Inventory != null && src.Inventory.BloodType != null
                        ? src.Inventory.BloodType.TypeName
                        : string.Empty));

        // ─────────────────────────────────────────
        // DonorMedicalDocument Mappings
        // ─────────────────────────────────────────
        CreateMap<DonorMedicalDocument, DonorMedicalDocumentDto>();

        // ─────────────────────────────────────────
        // Notification Mappings
        // ─────────────────────────────────────────
        CreateMap<Notification, NotificationDto>()
            .ForMember(dest => dest.Type,
                opt => opt.MapFrom(src => src.Type.ToString()));
    }
}
