using BloodConnectAPI.Models.DTOs;
using BloodConnectAPI.Service.Common;
using BloodConnectAPI.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodConnectAPI.Controllers;

/// <summary>
/// Controller لإدارة استجابات المتبرعين لطلبات الدم
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DonorResponsesController : ControllerBase
{
    private readonly IDonorResponseService _donorResponseService;
    private readonly ILogger<DonorResponsesController> _logger;

    public DonorResponsesController(IDonorResponseService donorResponseService, ILogger<DonorResponsesController> logger)
    {
        _donorResponseService = donorResponseService;
        _logger = logger;
    }

    /// <summary>
    /// تسجيل اهتمام متبرع بطلب دم — الخطوة الأولى في رحلة الاستجابة
    /// </summary>
    /// <param name="dto">بيانات المتبرع وطلب الدم</param>
    [HttpPost]
    [ProducesResponseType(typeof(ServiceResponse<DonorResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RespondToRequest([FromBody] CreateDonorResponseDto dto)
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

        _logger.LogInformation("تسجيل استجابة من المتبرع {DonorId} لطلب الدم {RequestId}", dto.DonorID, dto.RequestID);

        var response = await _donorResponseService.RespondToRequestAsync(dto);

        if (!response.IsSuccess)
            return BadRequest(response);

        _logger.LogInformation("تم تسجيل الاستجابة بنجاح — معرف الاستجابة: {ResponseId}", response.Data?.ResponseID);
        return CreatedAtAction(nameof(GetById), new { id = response.Data?.ResponseID }, response);
    }

    /// <summary>
    /// الحصول على تفاصيل استجابة واحدة
    /// </summary>
    /// <param name="id">معرف الاستجابة</param>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ServiceResponse<DonorResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await _donorResponseService.GetResponseByIdAsync(id);

        if (!response.IsSuccess)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>
    /// الحصول على جميع الاستجابات لطلب دم معين
    /// </summary>
    /// <param name="requestId">معرف طلب الدم</param>
    [HttpGet("request/{requestId}")]
    [ProducesResponseType(typeof(ServiceResponse<IEnumerable<DonorResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByRequest(int requestId)
    {
        var response = await _donorResponseService.GetResponsesByRequestAsync(requestId);

        if (!response.IsSuccess)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>
    /// الحصول على جميع استجابات متبرع معين
    /// </summary>
    /// <param name="donorId">معرف المتبرع</param>
    [HttpGet("donor/{donorId}")]
    [ProducesResponseType(typeof(ServiceResponse<IEnumerable<DonorResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByDonor(int donorId)
    {
        var response = await _donorResponseService.GetResponsesByDonorAsync(donorId);

        if (!response.IsSuccess)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>
    /// تحديث حالة الاستجابة — للموظف (Confirm / Reject / Donate / NoShow)
    /// </summary>
    /// <param name="id">معرف الاستجابة</param>
    /// <param name="dto">الحالة الجديدة والتفاصيل</param>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServiceResponse<DonorResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateResponseStatusDto dto)
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

        _logger.LogInformation("تحديث حالة الاستجابة {ResponseId} إلى {Status}", id, dto.Status);

        var response = await _donorResponseService.UpdateResponseStatusAsync(id, dto);

        if (!response.IsSuccess)
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>
    /// إلغاء استجابة مع ذكر السبب
    /// </summary>
    /// <param name="id">معرف الاستجابة</param>
    /// <param name="reason">سبب الإلغاء</param>
    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel(int id, [FromBody] string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return BadRequest(new ServiceResponse<object>
            {
                IsSuccess = false,
                Message = "يجب ذكر سبب الإلغاء"
            });
        }

        _logger.LogInformation("إلغاء الاستجابة {ResponseId}", id);

        var response = await _donorResponseService.CancelResponseAsync(id, reason);

        if (!response.IsSuccess)
            return BadRequest(response);

        return Ok(response);
    }
}
