using BloodConnectAPI.Models.DTOs;
using BloodConnectAPI.Models.Enums;
using BloodConnectAPI.Service.Common;
using BloodConnectAPI.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BloodConnectAPI.Controllers;

/// <summary>
/// Controller لإدارة التبرعات
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DonationsController : ControllerBase
{
    private readonly IDonationService _donationService;
    private readonly ILogger<DonationsController> _logger;

    public DonationsController(IDonationService donationService, ILogger<DonationsController> logger)
    {
        _donationService = donationService;
        _logger = logger;
    }

    /// <summary>
    /// الحصول على جميع التبرعات (مع Pagination)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ServiceResponse<PagedResult<DonationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
    {
        var response = await _donationService.GetAllAsync(pagination);
        return Ok(response);
    }

    /// <summary>
    /// الحصول على تبرع حسب ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ServiceResponse<DonationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await _donationService.GetByIdAsync(id);
        
        if (!response.IsSuccess)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>
    /// الحصول على تبرعات متبرع معين
    /// </summary>
    [HttpGet("donor/{donorId}")]
    [ProducesResponseType(typeof(ServiceResponse<IEnumerable<DonationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByDonor(int donorId)
    {
        var response = await _donationService.GetByDonorIdAsync(donorId);
        return Ok(response);
    }

    /// <summary>
    /// الحصول على التبرعات الحديثة
    /// </summary>
    [HttpGet("recent")]
    [ProducesResponseType(typeof(ServiceResponse<IEnumerable<BloodConnectAPI.Models.DTOs.DonationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecent([FromQuery] int days = 30)
    {
        var response = await _donationService.GetRecentDonationsAsync(days);
        return Ok(response);
    }

    /// <summary>
    /// إنشاء تبرع جديد
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ServiceResponse<BloodConnectAPI.Models.DTOs.DonationDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateDonationDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ServiceResponse<object>
            {
                IsSuccess = false,
                Message = "بيانات غير صحيحة",
                Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
            });
        }

        _logger.LogInformation("محاولة تسجيل تبرع جديد للمتبرع: {DonorId}", request.DonorID);

        var response = await _donationService.CreateAsync(request);
        
        if (!response.IsSuccess)
            return BadRequest(response);

        _logger.LogInformation("تم تسجيل تبرع جديد: {DonationId}", response.Data?.DonationID);
        return CreatedAtAction(nameof(GetById), new { id = response.Data?.DonationID }, response);
    }

    /// <summary>
    /// تحديث تبرع
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServiceResponse<DonationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDonationDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var response = await _donationService.UpdateAsync(id, dto);
        if (!response.IsSuccess) return BadRequest(response);
        return Ok(response);
    }

    /// <summary>
    /// فحص التبرع مختبريا
    /// </summary>
    [HttpPost("{id}/lab-test")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServiceResponse<DonationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LabTest(int id, [FromBody] LabTestDonationDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var testedBy = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        _logger.LogInformation("تسجيل نتيجة فحص التبرع: {DonationId}", id);
        var response = await _donationService.LabTestDonationAsync(id, dto, testedBy);
        if (!response.IsSuccess) return BadRequest(response);
        return Ok(response);
    }

    /// <summary>
    /// تحديث نتيجة الفحص
    /// </summary>
    [HttpPut("{id}/test-result")]
    [ProducesResponseType(typeof(ServiceResponse<DonationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateTestResult(int id, [FromBody] UpdateTestResultDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ServiceResponse<object>
            {
                IsSuccess = false,
                Message = "بيانات غير صحيحة",
                Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
            });
        }

        _logger.LogInformation("محاولة تحديث نتيجة فحص التبرع: {DonationId}", id);

        var response = await _donationService.UpdateTestResultAsync(id, dto.TestResult);
        
        if (!response.IsSuccess)
            return BadRequest(response);

        _logger.LogInformation("تم تحديث نتيجة الفحص للتبرع: {DonationId} - {TestResult}", id, dto.TestResult);
        return Ok(response);
    }

    /// <summary>
    /// حذف تبرع
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var response = await _donationService.DeleteAsync(id);
        
        if (!response.IsSuccess)
            return NotFound(response);

        return Ok(response);
    }
}
