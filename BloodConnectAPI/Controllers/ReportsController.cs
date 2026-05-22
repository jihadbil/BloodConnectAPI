using BloodConnectAPI.Models.DTOs.Reports.Common;
using BloodConnectAPI.Models.DTOs.Reports.Dashboard;
using BloodConnectAPI.Models.DTOs.Reports.Donations;
using BloodConnectAPI.Models.DTOs.Reports.Donors;
using BloodConnectAPI.Models.DTOs.Reports.Inventory;
using BloodConnectAPI.Models.DTOs.Reports.Patients;
using BloodConnectAPI.Models.DTOs.Reports.Requests;
using BloodConnectAPI.Service.Common;
using BloodConnectAPI.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BloodConnectAPI.Controllers;

/// <summary>
/// وحدة التحكم في التقارير الشاملة لنظام BloodConnect
/// Comprehensive reports controller for BloodConnect system
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Reports - التقارير")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    #region Donor Reports

    /// <summary>
    /// الحصول على إحصائيات المتبرعين حسب فصيلة الدم
    /// Get donor statistics grouped by blood type
    /// </summary>
    /// <returns>Statistics for all 8 blood types including zero counts</returns>
    [HttpGet("donors/statistics")]
    [ProducesResponseType(typeof(ServiceResponse<DonorStatisticsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDonorStatisticsByBloodType()
    {
        try
        {
            var response = await _reportService.GetDonorStatisticsByBloodTypeAsync();
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating donor statistics report");
            return StatusCode(500, ServiceResponse<object>.FailureResponse("حدث خطأ أثناء توليد التقرير"));
        }
    }

    /// <summary>
    /// الحصول على مقارنة بين المتبرعين النشطين وغير النشطين
    /// Get comparison between active and inactive donors
    /// </summary>
    /// <returns>Active vs inactive donor counts with percentages</returns>
    [HttpGet("donors/active-vs-inactive")]
    [ProducesResponseType(typeof(ServiceResponse<ActiveVsInactiveDonorsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetActiveVsInactiveDonors()
    {
        try
        {
            var response = await _reportService.GetActiveVsInactiveDonorsAsync();
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating active vs inactive donors report");
            return StatusCode(500, ServiceResponse<object>.FailureResponse("حدث خطأ أثناء توليد التقرير"));
        }
    }

    /// <summary>
    /// الحصول على توزيع المتبرعين حسب المدينة مع دعم الترقيم
    /// Get donor distribution by city with pagination support
    /// </summary>
    /// <param name="pagination">Pagination parameters (page number and page size)</param>
    /// <returns>Paginated list of cities with donor counts, sorted by count descending</returns>
    [HttpGet("donors/by-city")]
    [ProducesResponseType(typeof(ServiceResponse<PagedResult<DonorsByCityDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDonorsByCity([FromQuery] BloodConnectAPI.Models.DTOs.Reports.Common.PaginationParams pagination)
    {
        try
        {
            var response = await _reportService.GetDonorsByCityAsync(pagination);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating donors by city report");
            return StatusCode(500, ServiceResponse<object>.FailureResponse("حدث خطأ أثناء توليد التقرير"));
        }
    }

    /// <summary>
    /// الحصول على المتبرعين المؤهلين للتبرع حسب فصيلة الدم
    /// Get eligible donors by blood type (active donors who can donate now)
    /// </summary>
    /// <returns>Count of eligible donors for each blood type (56+ days since last donation)</returns>
    [HttpGet("donors/eligible")]
    [ProducesResponseType(typeof(ServiceResponse<List<EligibleDonorsByBloodTypeDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetEligibleDonorsByBloodType()
    {
        try
        {
            var response = await _reportService.GetEligibleDonorsByBloodTypeAsync();
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating eligible donors report");
            return StatusCode(500, ServiceResponse<object>.FailureResponse("حدث خطأ أثناء توليد التقرير"));
        }
    }

    #endregion

    #region Donation Reports

    /// <summary>
    /// الحصول على التبرعات حسب الفترة الزمنية
    /// Get donations grouped by time period (day, week, month, or year)
    /// </summary>
    /// <param name="filter">Date range filter (optional)</param>
    /// <param name="groupBy">Grouping period: "day", "week", "month", or "year"</param>
    /// <returns>Donation counts for each period within the date range</returns>
    [HttpGet("donations/by-period")]
    [ProducesResponseType(typeof(ServiceResponse<List<DonationsByPeriodDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDonationsByTimePeriod([FromQuery] DateFilterDto filter, [FromQuery] string groupBy = "month")
    {
        try
        {
            if (filter != null && !filter.IsValid())
            {
                return BadRequest(ServiceResponse<object>.FailureResponse("تاريخ البداية يجب أن يكون قبل تاريخ النهاية"));
            }

            var response = await _reportService.GetDonationsByTimePeriodAsync(filter, groupBy);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating donations by time period report");
            return StatusCode(500, ServiceResponse<object>.FailureResponse("حدث خطأ أثناء توليد التقرير"));
        }
    }

    /// <summary>
    /// الحصول على كميات الدم المتبرع بها حسب فصيلة الدم
    /// Get total blood quantities donated by blood type
    /// </summary>
    /// <param name="filter">Date range filter (optional)</param>
    /// <returns>Total quantity in milliliters for each blood type with grand total</returns>
    [HttpGet("donations/quantity")]
    [ProducesResponseType(typeof(ServiceResponse<List<BloodQuantityDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBloodQuantityDonated([FromQuery] DateFilterDto filter)
    {
        try
        {
            if (filter != null && !filter.IsValid())
            {
                return BadRequest(ServiceResponse<object>.FailureResponse("تاريخ البداية يجب أن يكون قبل تاريخ النهاية"));
            }

            var response = await _reportService.GetBloodQuantityDonatedAsync(filter);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating blood quantity donated report");
            return StatusCode(500, ServiceResponse<object>.FailureResponse("حدث خطأ أثناء توليد التقرير"));
        }
    }

    /// <summary>
    /// الحصول على نتائج فحوصات التبرعات
    /// Get donation test results statistics
    /// </summary>
    /// <param name="filter">Date range filter (optional)</param>
    /// <returns>Counts for Pending, Accepted, and Rejected test results with acceptance/rejection rates</returns>
    [HttpGet("donations/test-results")]
    [ProducesResponseType(typeof(ServiceResponse<DonationTestResultsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDonationTestResults([FromQuery] DateFilterDto filter)
    {
        try
        {
            if (filter != null && !filter.IsValid())
            {
                return BadRequest(ServiceResponse<object>.FailureResponse("تاريخ البداية يجب أن يكون قبل تاريخ النهاية"));
            }

            var response = await _reportService.GetDonationTestResultsAsync(filter);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating donation test results report");
            return StatusCode(500, ServiceResponse<object>.FailureResponse("حدث خطأ أثناء توليد التقرير"));
        }
    }

    /// <summary>
    /// الحصول على التبرعات حسب فصيلة الدم
    /// Get donations grouped by blood type
    /// </summary>
    /// <param name="filter">Date range filter (optional)</param>
    /// <returns>Donation counts for all blood types with percentages, sorted descending</returns>
    [HttpGet("donations/by-blood-type")]
    [ProducesResponseType(typeof(ServiceResponse<List<DonationsByBloodTypeDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDonationsByBloodType([FromQuery] DateFilterDto filter)
    {
        try
        {
            if (filter != null && !filter.IsValid())
            {
                return BadRequest(ServiceResponse<object>.FailureResponse("تاريخ البداية يجب أن يكون قبل تاريخ النهاية"));
            }

            var response = await _reportService.GetDonationsByBloodTypeAsync(filter);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating donations by blood type report");
            return StatusCode(500, ServiceResponse<object>.FailureResponse("حدث خطأ أثناء توليد التقرير"));
        }
    }

    /// <summary>
    /// الحصول على أكثر المتبرعين نشاطاً
    /// Get most active donors ranked by donation count
    /// </summary>
    /// <param name="filter">Date range filter (optional)</param>
    /// <param name="limit">Maximum number of donors to return (default: 10)</param>
    /// <returns>Top donors with their donation counts, sorted by count descending</returns>
    [HttpGet("donations/most-active-donors")]
    [ProducesResponseType(typeof(ServiceResponse<List<MostActiveDonorDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMostActiveDonors([FromQuery] DateFilterDto filter, [FromQuery] int limit = 10)
    {
        try
        {
            if (filter != null && !filter.IsValid())
            {
                return BadRequest(ServiceResponse<object>.FailureResponse("تاريخ البداية يجب أن يكون قبل تاريخ النهاية"));
            }

            var response = await _reportService.GetMostActiveDonorsAsync(filter, limit);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating most active donors report");
            return StatusCode(500, ServiceResponse<object>.FailureResponse("حدث خطأ أثناء توليد التقرير"));
        }
    }

    #endregion

    #region Inventory Reports

    /// <summary>
    /// الحصول على توفر المخزون لجميع فصائل الدم
    /// Get inventory availability for all blood types
    /// </summary>
    /// <returns>Available, reserved, and total available quantities for all 8 blood types</returns>
    [HttpGet("inventory/availability")]
    [ProducesResponseType(typeof(ServiceResponse<List<InventoryAvailabilityDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetInventoryAvailability()
    {
        try
        {
            var response = await _reportService.GetInventoryAvailabilityAsync();
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating inventory availability report");
            return StatusCode(500, ServiceResponse<object>.FailureResponse("حدث خطأ أثناء توليد التقرير"));
        }
    }

    /// <summary>
    /// الحصول على وحدات الدم القريبة من انتهاء الصلاحية
    /// Get blood units expiring within the next 7 days
    /// </summary>
    /// <returns>Expiring units grouped by blood type with days until expiry</returns>
    [HttpGet("inventory/expiring")]
    [ProducesResponseType(typeof(ServiceResponse<List<ExpiringBloodUnitsDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetExpiringBloodUnits()
    {
        try
        {
            var response = await _reportService.GetExpiringBloodUnitsAsync();
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating expiring blood units report");
            return StatusCode(500, ServiceResponse<object>.FailureResponse("حدث خطأ أثناء توليد التقرير"));
        }
    }

    /// <summary>
    /// الحصول على وحدات الدم منتهية الصلاحية
    /// Get expired blood units within a date range
    /// </summary>
    /// <param name="filter">Date range filter (optional)</param>
    /// <returns>Expired units grouped by blood type with total quantity wasted</returns>
    [HttpGet("inventory/expired")]
    [ProducesResponseType(typeof(ServiceResponse<List<ExpiredBloodUnitsDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetExpiredBloodUnits([FromQuery] DateFilterDto filter)
    {
        try
        {
            if (filter != null && !filter.IsValid())
            {
                return BadRequest(ServiceResponse<object>.FailureResponse("تاريخ البداية يجب أن يكون قبل تاريخ النهاية"));
            }

            var response = await _reportService.GetExpiredBloodUnitsAsync(filter);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating expired blood units report");
            return StatusCode(500, ServiceResponse<object>.FailureResponse("حدث خطأ أثناء توليد التقرير"));
        }
    }

    /// <summary>
    /// الحصول على معدل استهلاك المخزون
    /// Get inventory consumption rate and stockout projections
    /// </summary>
    /// <param name="filter">Date range filter (optional)</param>
    /// <returns>Consumption statistics with average daily rate and projected days until stockout</returns>
    [HttpGet("inventory/consumption-rate")]
    [ProducesResponseType(typeof(ServiceResponse<List<ConsumptionRateDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetInventoryConsumptionRate([FromQuery] DateFilterDto filter)
    {
        try
        {
            if (filter != null && !filter.IsValid())
            {
                return BadRequest(ServiceResponse<object>.FailureResponse("تاريخ البداية يجب أن يكون قبل تاريخ النهاية"));
            }

            var response = await _reportService.GetInventoryConsumptionRateAsync(filter);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating inventory consumption rate report");
            return StatusCode(500, ServiceResponse<object>.FailureResponse("حدث خطأ أثناء توليد التقرير"));
        }
    }

    /// <summary>
    /// الحصول على تنبيهات المخزون المنخفض
    /// Get low inventory alerts for blood types below threshold
    /// </summary>
    /// <param name="threshold">Minimum quantity threshold (default: 10 units)</param>
    /// <returns>Blood types with quantities below threshold, sorted by severity</returns>
    [HttpGet("inventory/low-stock-alerts")]
    [ProducesResponseType(typeof(ServiceResponse<List<LowInventoryAlertDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLowInventoryAlerts([FromQuery] int threshold = 10)
    {
        try
        {
            var response = await _reportService.GetLowInventoryAlertsAsync(threshold);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating low inventory alerts report");
            return StatusCode(500, ServiceResponse<object>.FailureResponse("حدث خطأ أثناء توليد التقرير"));
        }
    }

    #endregion

    #region Blood Request Reports

    /// <summary>
    /// الحصول على الطلبات حسب الحالة
    /// Get blood requests grouped by status
    /// </summary>
    /// <param name="filter">Date range filter (optional)</param>
    /// <returns>Counts for Pending, Fulfilled, and Cancelled requests with fulfillment/cancellation rates</returns>
    [HttpGet("requests/by-status")]
    [ProducesResponseType(typeof(ServiceResponse<RequestsByStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRequestsByStatus([FromQuery] DateFilterDto filter)
    {
        try
        {
            if (filter != null && !filter.IsValid())
            {
                return BadRequest(ServiceResponse<object>.FailureResponse("تاريخ البداية يجب أن يكون قبل تاريخ النهاية"));
            }

            var response = await _reportService.GetRequestsByStatusAsync(filter);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating requests by status report");
            return StatusCode(500, ServiceResponse<object>.FailureResponse("حدث خطأ أثناء توليد التقرير"));
        }
    }

    /// <summary>
    /// الحصول على الطلبات حسب مستوى الاستعجال
    /// Get blood requests grouped by urgency level
    /// </summary>
    /// <param name="filter">Date range filter (optional)</param>
    /// <returns>Counts for Normal, Urgent, and Emergency requests, sorted by urgency</returns>
    [HttpGet("requests/by-urgency")]
    [ProducesResponseType(typeof(ServiceResponse<List<RequestsByUrgencyDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRequestsByUrgency([FromQuery] DateFilterDto filter)
    {
        try
        {
            if (filter != null && !filter.IsValid())
            {
                return BadRequest(ServiceResponse<object>.FailureResponse("تاريخ البداية يجب أن يكون قبل تاريخ النهاية"));
            }

            var response = await _reportService.GetRequestsByUrgencyAsync(filter);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating requests by urgency report");
            return StatusCode(500, ServiceResponse<object>.FailureResponse("حدث خطأ أثناء توليد التقرير"));
        }
    }

    /// <summary>
    /// الحصول على معدل تلبية الطلبات حسب فصيلة الدم
    /// Get request fulfillment rate by blood type
    /// </summary>
    /// <param name="filter">Date range filter (optional)</param>
    /// <returns>Fulfillment rates for each blood type</returns>
    [HttpGet("requests/fulfillment-rate")]
    [ProducesResponseType(typeof(ServiceResponse<List<FulfillmentRateDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRequestFulfillmentRate([FromQuery] DateFilterDto filter)
    {
        try
        {
            if (filter != null && !filter.IsValid())
            {
                return BadRequest(ServiceResponse<object>.FailureResponse("تاريخ البداية يجب أن يكون قبل تاريخ النهاية"));
            }

            var response = await _reportService.GetRequestFulfillmentRateAsync(filter);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating request fulfillment rate report");
            return StatusCode(500, ServiceResponse<object>.FailureResponse("حدث خطأ أثناء توليد التقرير"));
        }
    }

    /// <summary>
    /// الحصول على الطلبات حسب فصيلة الدم
    /// Get blood requests grouped by blood type
    /// </summary>
    /// <param name="filter">Date range filter (optional)</param>
    /// <returns>Request counts for all blood types with percentages, sorted descending</returns>
    [HttpGet("requests/by-blood-type")]
    [ProducesResponseType(typeof(ServiceResponse<List<RequestsByBloodTypeDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRequestsByBloodType([FromQuery] DateFilterDto filter)
    {
        try
        {
            if (filter != null && !filter.IsValid())
            {
                return BadRequest(ServiceResponse<object>.FailureResponse("تاريخ البداية يجب أن يكون قبل تاريخ النهاية"));
            }

            var response = await _reportService.GetRequestsByBloodTypeAsync(filter);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating requests by blood type report");
            return StatusCode(500, ServiceResponse<object>.FailureResponse("حدث خطأ أثناء توليد التقرير"));
        }
    }

    /// <summary>
    /// الحصول على متوسط وقت تلبية الطلبات
    /// Get average fulfillment time grouped by urgency level
    /// </summary>
    /// <param name="filter">Date range filter (optional)</param>
    /// <returns>Average fulfillment time in hours for each urgency level</returns>
    [HttpGet("requests/avg-fulfillment-time")]
    [ProducesResponseType(typeof(ServiceResponse<List<AvgFulfillmentTimeDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAverageFulfillmentTime([FromQuery] DateFilterDto filter)
    {
        try
        {
            if (filter != null && !filter.IsValid())
            {
                return BadRequest(ServiceResponse<object>.FailureResponse("تاريخ البداية يجب أن يكون قبل تاريخ النهاية"));
            }

            var response = await _reportService.GetAverageFulfillmentTimeAsync(filter);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating average fulfillment time report");
            return StatusCode(500, ServiceResponse<object>.FailureResponse("حدث خطأ أثناء توليد التقرير"));
        }
    }

    #endregion

    #region Patient Reports

    /// <summary>
    /// الحصول على عدد المرضى المسجلين
    /// Get total count of registered patients
    /// </summary>
    /// <param name="filter">Date range filter (optional)</param>
    /// <returns>Total patient count with monthly trends if date range spans multiple months</returns>
    [HttpGet("patients/count")]
    [ProducesResponseType(typeof(ServiceResponse<PatientCountDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRegisteredPatientsCount([FromQuery] DateFilterDto filter)
    {
        try
        {
            if (filter != null && !filter.IsValid())
            {
                return BadRequest(ServiceResponse<object>.FailureResponse("تاريخ البداية يجب أن يكون قبل تاريخ النهاية"));
            }

            var response = await _reportService.GetRegisteredPatientsCountAsync(filter);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating registered patients count report");
            return StatusCode(500, ServiceResponse<object>.FailureResponse("حدث خطأ أثناء توليد التقرير"));
        }
    }

    /// <summary>
    /// الحصول على المرضى حسب فصيلة الدم
    /// Get patients grouped by blood type
    /// </summary>
    /// <returns>Patient counts for all blood types with percentages, sorted descending</returns>
    [HttpGet("patients/by-blood-type")]
    [ProducesResponseType(typeof(ServiceResponse<List<PatientsByBloodTypeDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPatientsByBloodType()
    {
        try
        {
            var response = await _reportService.GetPatientsByBloodTypeAsync();
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating patients by blood type report");
            return StatusCode(500, ServiceResponse<object>.FailureResponse("حدث خطأ أثناء توليد التقرير"));
        }
    }

    /// <summary>
    /// الحصول على المرضى الذين لديهم طلبات نشطة
    /// Get patients with active blood requests
    /// </summary>
    /// <returns>Patients with pending requests, including highest urgency level, sorted by urgency and request count</returns>
    [HttpGet("patients/with-active-requests")]
    [ProducesResponseType(typeof(ServiceResponse<List<PatientsWithActiveRequestsDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPatientsWithActiveRequests()
    {
        try
        {
            var response = await _reportService.GetPatientsWithActiveRequestsAsync();
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating patients with active requests report");
            return StatusCode(500, ServiceResponse<object>.FailureResponse("حدث خطأ أثناء توليد التقرير"));
        }
    }

    #endregion

    #region Dashboard

    /// <summary>
    /// الحصول على ملخص شامل للوحة المعلومات
    /// Get comprehensive dashboard summary with key statistics
    /// </summary>
    /// <returns>Dashboard summary including donor, donation, request, inventory, and alert statistics</returns>
    [HttpGet("dashboard/summary")]
    [ProducesResponseType(typeof(ServiceResponse<DashboardSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDashboardSummary()
    {
        try
        {
            var response = await _reportService.GetDashboardSummaryAsync();
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating dashboard summary report");
            return StatusCode(500, ServiceResponse<object>.FailureResponse("حدث خطأ أثناء توليد التقرير"));
        }
    }

    #endregion
}
