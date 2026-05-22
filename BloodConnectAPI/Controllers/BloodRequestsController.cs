using BloodConnectAPI.Models.DTOs;
using BloodConnectAPI.Models.Enums;
using BloodConnectAPI.Service.Common;
using BloodConnectAPI.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BloodConnectAPI.Controllers;

/// <summary>
/// Controller لإدارة طلبات الدم
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class BloodRequestsController : ControllerBase
{
    private readonly IBloodRequestService _requestService;
    private readonly IDonorResponseService _donorResponseService;
    private readonly ILogger<BloodRequestsController> _logger;

    public BloodRequestsController(
        IBloodRequestService requestService,
        IDonorResponseService donorResponseService,
        ILogger<BloodRequestsController> logger)
    {
        _requestService = requestService;
        _donorResponseService = donorResponseService;
        _logger = logger;
    }

    /// <summary>
    /// الحصول على جميع طلبات الدم (مع Pagination)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ServiceResponse<PagedResult<BloodRequestDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
    {
        var response = await _requestService.GetAllAsync(pagination);
        return Ok(response);
    }

    /// <summary>
    /// الحصول على طلب حسب ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ServiceResponse<BloodRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await _requestService.GetByIdAsync(id);
        
        if (!response.IsSuccess)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>
    /// الحصول على طلب مع التفاصيل الكاملة
    /// </summary>
    [HttpGet("{id}/details")]
    [ProducesResponseType(typeof(ServiceResponse<BloodRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWithDetails(int id)
    {
        var response = await _requestService.GetWithDetailsAsync(id);
        
        if (!response.IsSuccess)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>
    /// الحصول على الطلبات المعلقة
    /// </summary>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(ServiceResponse<IEnumerable<BloodRequestDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPending()
    {
        var response = await _requestService.GetPendingRequestsAsync();
        return Ok(response);
    }

    /// <summary>
    /// الحصول على الطلبات العاجلة
    /// </summary>
    [HttpGet("urgent")]
    [ProducesResponseType(typeof(ServiceResponse<IEnumerable<BloodRequestDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUrgent()
    {
        var response = await _requestService.GetUrgentRequestsAsync();
        return Ok(response);
    }

    /// <summary>
    /// إنشاء طلب دم جديد
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ServiceResponse<BloodRequestDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateBloodRequestDto request)
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

        _logger.LogInformation("محاولة إنشاء طلب دم جديد للمريض: {PatientId}", request.PatientID);

        var response = await _requestService.CreateAsync(request);
        
        if (!response.IsSuccess)
            return BadRequest(response);

        _logger.LogInformation("تم إنشاء طلب دم جديد: {RequestId}", response.Data?.RequestID);
        return CreatedAtAction(nameof(GetById), new { id = response.Data?.RequestID }, response);
    }

    /// <summary>
    /// تحديث حالة طلب
    /// </summary>
    [HttpPut("{id}/status")]
    [ProducesResponseType(typeof(ServiceResponse<BloodRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromQuery] BloodConnectAPI.Models.Enums.RequestStatus status,
        [FromQuery] string? notes = null)
    {
        var response = await _requestService.UpdateStatusAsync(id, status, notes);
        
        if (!response.IsSuccess)
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>
    /// تنفيذ طلب دم (Fulfill)
    /// </summary>
    [HttpPost("{id}/fulfill")]
    [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FulfillRequest(int id, [FromBody] FulfillBloodRequestDto dto)
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

        _logger.LogInformation("محاولة تنفيذ طلب دم: {RequestId}", id);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        
        var response = await _requestService.FulfillRequestAsync(id, dto, userId);
        
        if (!response.IsSuccess)
            return BadRequest(response);

        _logger.LogInformation("تم تنفيذ طلب الدم بنجاح: {RequestId}", id);
        return Ok(response);
    }

    /// <summary>
    /// إلغاء طلب
    /// </summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelRequestDto dto)
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

        _logger.LogInformation("محاولة إلغاء طلب دم: {RequestId}", id);

        var response = await _requestService.CancelAsync(id, dto.Reason);
        
        if (!response.IsSuccess)
            return BadRequest(response);

        _logger.LogInformation("تم إلغاء طلب الدم: {RequestId}", id);
        return Ok(response);
    }

    /// <summary>
    /// جلب جميع استجابات المتبرعين لطلب دم معين
    /// </summary>
    /// <param name="id">معرف طلب الدم</param>
    [HttpGet("{id}/responses")]
    [ProducesResponseType(typeof(ServiceResponse<IEnumerable<DonorResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetResponses(int id)
    {
        var response = await _donorResponseService.GetResponsesByRequestAsync(id);

        if (!response.IsSuccess)
            return NotFound(response);

        return Ok(response);
    }
}
