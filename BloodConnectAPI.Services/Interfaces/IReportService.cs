using BloodConnectAPI.Models.DTOs.Reports.Common;
using BloodConnectAPI.Models.DTOs.Reports.Dashboard;
using BloodConnectAPI.Models.DTOs.Reports.Donations;
using BloodConnectAPI.Models.DTOs.Reports.Donors;
using BloodConnectAPI.Models.DTOs.Reports.Inventory;
using BloodConnectAPI.Models.DTOs.Reports.Patients;
using BloodConnectAPI.Models.DTOs.Reports.Requests;
using BloodConnectAPI.Service.Common;

namespace BloodConnectAPI.Service.Interfaces;

/// <summary>
/// خدمة التقارير الشاملة لنظام BloodConnect
/// Comprehensive reporting service for BloodConnect system
/// </summary>
public interface IReportService
{
    #region Donor Reports

    /// <summary>
    /// الحصول على إحصائيات المتبرعين حسب فصيلة الدم
    /// Get donor statistics grouped by blood type
    /// </summary>
    /// <returns>Statistics for all 8 blood types including zero counts</returns>
    Task<ServiceResponse<DonorStatisticsDto>> GetDonorStatisticsByBloodTypeAsync();

    /// <summary>
    /// الحصول على مقارنة بين المتبرعين النشطين وغير النشطين
    /// Get comparison between active and inactive donors
    /// </summary>
    /// <returns>Active vs inactive donor counts with percentages</returns>
    Task<ServiceResponse<ActiveVsInactiveDonorsDto>> GetActiveVsInactiveDonorsAsync();

    /// <summary>
    /// الحصول على توزيع المتبرعين حسب المدينة مع دعم الترقيم
    /// Get donor distribution by city with pagination support
    /// </summary>
    /// <param name="pagination">Pagination parameters (page number and page size)</param>
    /// <returns>Paginated list of cities with donor counts, sorted by count descending</returns>
    Task<ServiceResponse<PagedResult<DonorsByCityDto>>> GetDonorsByCityAsync(BloodConnectAPI.Models.DTOs.Reports.Common.PaginationParams pagination);

    /// <summary>
    /// الحصول على المتبرعين المؤهلين للتبرع حسب فصيلة الدم
    /// Get eligible donors by blood type (active donors who can donate now)
    /// </summary>
    /// <returns>Count of eligible donors for each blood type (56+ days since last donation)</returns>
    Task<ServiceResponse<List<EligibleDonorsByBloodTypeDto>>> GetEligibleDonorsByBloodTypeAsync();

    #endregion

    #region Donation Reports

    /// <summary>
    /// الحصول على التبرعات حسب الفترة الزمنية
    /// Get donations grouped by time period (day, week, month, or year)
    /// </summary>
    /// <param name="filter">Date range filter (optional)</param>
    /// <param name="groupBy">Grouping period: "day", "week", "month", or "year"</param>
    /// <returns>Donation counts for each period within the date range</returns>
    Task<ServiceResponse<List<DonationsByPeriodDto>>> GetDonationsByTimePeriodAsync(DateFilterDto? filter, string groupBy);

    /// <summary>
    /// الحصول على كميات الدم المتبرع بها حسب فصيلة الدم
    /// Get total blood quantities donated by blood type
    /// </summary>
    /// <param name="filter">Date range filter (optional)</param>
    /// <returns>Total quantity in milliliters for each blood type with grand total</returns>
    Task<ServiceResponse<List<BloodQuantityDto>>> GetBloodQuantityDonatedAsync(DateFilterDto? filter);

    /// <summary>
    /// الحصول على نتائج فحوصات التبرعات
    /// Get donation test results statistics
    /// </summary>
    /// <param name="filter">Date range filter (optional)</param>
    /// <returns>Counts for Pending, Accepted, and Rejected test results with acceptance/rejection rates</returns>
    Task<ServiceResponse<DonationTestResultsDto>> GetDonationTestResultsAsync(DateFilterDto? filter);

    /// <summary>
    /// الحصول على التبرعات حسب فصيلة الدم
    /// Get donations grouped by blood type
    /// </summary>
    /// <param name="filter">Date range filter (optional)</param>
    /// <returns>Donation counts for all blood types with percentages, sorted descending</returns>
    Task<ServiceResponse<List<DonationsByBloodTypeDto>>> GetDonationsByBloodTypeAsync(DateFilterDto? filter);

    /// <summary>
    /// الحصول على أكثر المتبرعين نشاطاً
    /// Get most active donors ranked by donation count
    /// </summary>
    /// <param name="filter">Date range filter (optional)</param>
    /// <param name="limit">Maximum number of donors to return (default: 10)</param>
    /// <returns>Top donors with their donation counts, sorted by count descending</returns>
    Task<ServiceResponse<List<MostActiveDonorDto>>> GetMostActiveDonorsAsync(DateFilterDto? filter, int limit);

    #endregion

    #region Inventory Reports

    /// <summary>
    /// الحصول على توفر المخزون لجميع فصائل الدم
    /// Get inventory availability for all blood types
    /// </summary>
    /// <returns>Available, reserved, and total available quantities for all 8 blood types</returns>
    Task<ServiceResponse<List<InventoryAvailabilityDto>>> GetInventoryAvailabilityAsync();

    /// <summary>
    /// الحصول على وحدات الدم القريبة من انتهاء الصلاحية
    /// Get blood units expiring within the next 7 days
    /// </summary>
    /// <returns>Expiring units grouped by blood type with days until expiry</returns>
    Task<ServiceResponse<List<ExpiringBloodUnitsDto>>> GetExpiringBloodUnitsAsync();

    /// <summary>
    /// الحصول على وحدات الدم منتهية الصلاحية
    /// Get expired blood units within a date range
    /// </summary>
    /// <param name="filter">Date range filter (optional)</param>
    /// <returns>Expired units grouped by blood type with total quantity wasted</returns>
    Task<ServiceResponse<List<ExpiredBloodUnitsDto>>> GetExpiredBloodUnitsAsync(DateFilterDto? filter);

    /// <summary>
    /// الحصول على معدل استهلاك المخزون
    /// Get inventory consumption rate and stockout projections
    /// </summary>
    /// <param name="filter">Date range filter (optional)</param>
    /// <returns>Consumption statistics with average daily rate and projected days until stockout</returns>
    Task<ServiceResponse<List<ConsumptionRateDto>>> GetInventoryConsumptionRateAsync(DateFilterDto? filter);

    /// <summary>
    /// الحصول على تنبيهات المخزون المنخفض
    /// Get low inventory alerts for blood types below threshold
    /// </summary>
    /// <param name="threshold">Minimum quantity threshold (default: 10 units)</param>
    /// <returns>Blood types with quantities below threshold, sorted by severity</returns>
    Task<ServiceResponse<List<LowInventoryAlertDto>>> GetLowInventoryAlertsAsync(int threshold);

    #endregion

    #region Blood Request Reports

    /// <summary>
    /// الحصول على الطلبات حسب الحالة
    /// Get blood requests grouped by status
    /// </summary>
    /// <param name="filter">Date range filter (optional)</param>
    /// <returns>Counts for Pending, Fulfilled, and Cancelled requests with fulfillment/cancellation rates</returns>
    Task<ServiceResponse<RequestsByStatusDto>> GetRequestsByStatusAsync(DateFilterDto? filter);

    /// <summary>
    /// الحصول على الطلبات حسب مستوى الاستعجال
    /// Get blood requests grouped by urgency level
    /// </summary>
    /// <param name="filter">Date range filter (optional)</param>
    /// <returns>Counts for Normal, Urgent, and Emergency requests, sorted by urgency</returns>
    Task<ServiceResponse<List<RequestsByUrgencyDto>>> GetRequestsByUrgencyAsync(DateFilterDto? filter);

    /// <summary>
    /// الحصول على معدل تلبية الطلبات حسب فصيلة الدم
    /// Get request fulfillment rate by blood type
    /// </summary>
    /// <param name="filter">Date range filter (optional)</param>
    /// <returns>Fulfillment rates for each blood type</returns>
    Task<ServiceResponse<List<FulfillmentRateDto>>> GetRequestFulfillmentRateAsync(DateFilterDto? filter);

    /// <summary>
    /// الحصول على الطلبات حسب فصيلة الدم
    /// Get blood requests grouped by blood type
    /// </summary>
    /// <param name="filter">Date range filter (optional)</param>
    /// <returns>Request counts for all blood types with percentages, sorted descending</returns>
    Task<ServiceResponse<List<RequestsByBloodTypeDto>>> GetRequestsByBloodTypeAsync(DateFilterDto? filter);

    /// <summary>
    /// الحصول على متوسط وقت تلبية الطلبات
    /// Get average fulfillment time grouped by urgency level
    /// </summary>
    /// <param name="filter">Date range filter (optional)</param>
    /// <returns>Average fulfillment time in hours for each urgency level</returns>
    Task<ServiceResponse<List<AvgFulfillmentTimeDto>>> GetAverageFulfillmentTimeAsync(DateFilterDto? filter);

    #endregion

    #region Patient Reports

    /// <summary>
    /// الحصول على عدد المرضى المسجلين
    /// Get total count of registered patients
    /// </summary>
    /// <param name="filter">Date range filter (optional)</param>
    /// <returns>Total patient count with monthly trends if date range spans multiple months</returns>
    Task<ServiceResponse<PatientCountDto>> GetRegisteredPatientsCountAsync(DateFilterDto? filter);

    /// <summary>
    /// الحصول على المرضى حسب فصيلة الدم
    /// Get patients grouped by blood type
    /// </summary>
    /// <returns>Patient counts for all blood types with percentages, sorted descending</returns>
    Task<ServiceResponse<List<PatientsByBloodTypeDto>>> GetPatientsByBloodTypeAsync();

    /// <summary>
    /// الحصول على المرضى الذين لديهم طلبات نشطة
    /// Get patients with active blood requests
    /// </summary>
    /// <returns>Patients with pending requests, including highest urgency level, sorted by urgency and request count</returns>
    Task<ServiceResponse<List<PatientsWithActiveRequestsDto>>> GetPatientsWithActiveRequestsAsync();

    #endregion

    #region Dashboard

    /// <summary>
    /// الحصول على ملخص شامل للوحة المعلومات
    /// Get comprehensive dashboard summary with key statistics
    /// </summary>
    /// <returns>Dashboard summary including donor, donation, request, inventory, and alert statistics</returns>
    Task<ServiceResponse<DashboardSummaryDto>> GetDashboardSummaryAsync();

    #endregion
}
