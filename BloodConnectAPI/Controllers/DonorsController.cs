using BloodConnectAPI.Models.DTOs;
using BloodConnectAPI.Models.Enums;
using BloodConnectAPI.Service.Common;
using BloodConnectAPI.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BloodConnectAPI.Controllers;

/// <summary>
/// Controller لإدارة المتبرعين
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DonorsController : ControllerBase
{
    private readonly IDonorService _donorService;
    private readonly ILogger<DonorsController> _logger;

    public DonorsController(IDonorService donorService, ILogger<DonorsController> logger)
    {
        _donorService = donorService;
        _logger = logger;
    }

    /// <summary>
    /// الحصول على جميع المتبرعين (مع Pagination)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ServiceResponse<PagedResult<DonorDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
    {
        var response = await _donorService.GetAllAsync(pagination);
        return Ok(response);
    }

    /// <summary>
    /// الحصول على متبرع حسب ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ServiceResponse<DonorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await _donorService.GetByIdAsync(id);
        
        if (!response.IsSuccess)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>
    /// الحصول على متبرع حسب الرقم الوطني
    /// </summary>
    [HttpGet("national/{nationalId}")]
    [ProducesResponseType(typeof(ServiceResponse<DonorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByNationalId(string nationalId)
    {
        var response = await _donorService.GetByNationalIdAsync(nationalId);
        
        if (!response.IsSuccess)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>
    /// الحصول على المتبرعين المؤهلين للتبرع
    /// </summary>
    [HttpGet("eligible")]
    [ProducesResponseType(typeof(ServiceResponse<IEnumerable<DonorDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEligible([FromQuery] int? bloodTypeId)
    {
        var response = await _donorService.GetEligibleDonorsAsync(bloodTypeId);
        return Ok(response);
    }

    /// <summary>
    /// التحقق من أهلية المتبرع للتبرع
    /// </summary>
    [HttpGet("{id}/check-eligibility")]
    [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckEligibility(int id)
    {
        var response = await _donorService.CheckEligibilityAsync(id);
        
        if (!response.IsSuccess)
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>
    /// الحصول على متبرع مع تبرعاته
    /// </summary>
    [HttpGet("{id}/donations")]
    [ProducesResponseType(typeof(ServiceResponse<DonorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWithDonations(int id)
    {
        var response = await _donorService.GetWithDonationsAsync(id);
        
        if (!response.IsSuccess)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>
    /// الحصول على سجل المتبرع المرتبط بمعرف المستخدم
    /// </summary>
    [HttpGet("by-user/{userId}")]
    [Authorize]
    [ProducesResponseType(typeof(ServiceResponse<DonorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByUserId(string userId)
    {
        // السماح للمستخدم بالوصول لبياناته الخاصة أو للـ Admin بالوصول لأي مستخدم
        var requesterId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        if (!isAdmin && requesterId != userId)
            return Forbid();

        var response = await _donorService.GetByUserIdAsync(userId);

        if (!response.IsSuccess)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>
    /// إنشاء متبرع جديد
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ServiceResponse<DonorDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateDonorDto request)
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

        _logger.LogInformation("محاولة إنشاء متبرع جديد: {NationalId}", request.NationalID);

        var response = await _donorService.CreateAsync(request);
        
        if (!response.IsSuccess)
            return BadRequest(response);

        _logger.LogInformation("تم إنشاء متبرع جديد: {DonorId}", response.Data?.DonorID);
        return CreatedAtAction(nameof(GetById), new { id = response.Data?.DonorID }, response);
    }

    /// <summary>
    /// تحديث متبرع
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ServiceResponse<DonorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDonorDto request)
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

        var response = await _donorService.UpdateAsync(id, request);
        
        if (!response.IsSuccess)
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>
    /// حذف متبرع
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var response = await _donorService.DeleteAsync(id);
        
        if (!response.IsSuccess)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>
    /// ربط متبرع بمستخدم
    /// </summary>
    [HttpPost("{donorId}/link-user/{userId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServiceResponse<DonorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> LinkDonorToUser(int donorId, string userId)
    {
        _logger.LogInformation("محاولة ربط المتبرع {DonorId} بالمستخدم {UserId}", donorId, userId);

        var response = await _donorService.LinkDonorToUserAsync(donorId, userId);
        
        if (!response.IsSuccess)
        {
            // Determine appropriate status code based on error message
            if (response.Message.Contains("غير موجود"))
                return NotFound(response);
            
            if (response.Message.Contains("مرتبط بمتبرع آخر"))
                return Conflict(response);
            
            return BadRequest(response);
        }

        _logger.LogInformation("تم ربط المتبرع {DonorId} بالمستخدم {UserId} بنجاح", donorId, userId);
        return Ok(response);
    }

    /// <summary>
    /// فك ربط متبرع من مستخدم
    /// </summary>
    [HttpDelete("{donorId}/unlink-user")]
    [ProducesResponseType(typeof(ServiceResponse<DonorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlinkDonorFromUser(int donorId)
    {
        _logger.LogInformation("محاولة فك ربط المتبرع {DonorId} من المستخدم", donorId);

        var response = await _donorService.UnlinkDonorFromUserAsync(donorId);
        
        if (!response.IsSuccess)
        {
            // Determine appropriate status code based on error message
            if (response.Message.Contains("غير موجود"))
                return NotFound(response);
            
            return BadRequest(response);
        }

        _logger.LogInformation("تم فك ربط المتبرع {DonorId} من المستخدم بنجاح", donorId);
        return Ok(response);
    }

    /// <summary>
    /// الحصول على متبرع مع معلومات المستخدم المرتبط
    /// </summary>
    [HttpGet("{id}/with-user")]
    [ProducesResponseType(typeof(ServiceResponse<DonorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDonorWithUser(int id)
    {
        var response = await _donorService.GetWithUserAsync(id);
        
        if (!response.IsSuccess)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>
    /// الموافقة على متبرع أو رفضه
    /// </summary>
    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServiceResponse<DonorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApproveDonor(int id, [FromBody] ApproveDonorDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var reviewerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var response = await _donorService.ApproveDonorAsync(id, dto, reviewerId);
        if (!response.IsSuccess) return BadRequest(response);
        return Ok(response);
    }

    /// <summary>
    /// الحصول على المتبرعين حسب حالة الموافقة
    /// </summary>
    [HttpGet("by-status/{status}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServiceResponse<PagedResult<DonorDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByApprovalStatus(
        DonorApprovalStatus status, [FromQuery] PaginationParams pagination)
    {
        var response = await _donorService.GetByApprovalStatusAsync(status, pagination);
        return Ok(response);
    }

    /// <summary>
    /// الحصول على الوثائق الطبية للمتبرع
    /// </summary>
    [HttpGet("{id}/medical-documents")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServiceResponse<IEnumerable<DonorMedicalDocumentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMedicalDocuments(int id)
    {
        var response = await _donorService.GetMedicalDocumentsAsync(id);
        if (!response.IsSuccess) return NotFound(response);
        return Ok(response);
    }
}
