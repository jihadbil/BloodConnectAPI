using BloodConnectAPI.Models.DTOs;
using BloodConnectAPI.Service.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BloodConnectAPI.Service.Interfaces;

/// <summary>
/// خدمة إدارة تقارير تحاليل عينات التبرع
/// </summary>
public interface IDonationLabReportService
{
    Task<ServiceResponse<DonationLabReportDto>> UploadAsync(UploadDonationLabReportDto dto, string uploadsRootPath, string uploadedByUserId);
    Task<ServiceResponse<IEnumerable<DonationLabReportDto>>> GetByDonationIdAsync(int donationId);
    Task<ServiceResponse<DonationLabReportDto>> GetByIdAsync(int reportId);
    Task<ServiceResponse<bool>> DeleteAsync(int reportId, string uploadsRootPath);
}
