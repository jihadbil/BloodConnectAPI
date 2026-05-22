using BloodConnectAPI.Models.DTOs;
using BloodConnectAPI.Service.Common;
using BloodConnectAPI.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BloodConnectAPI.Controllers;

/// <summary>
/// Controller لإدارة المرضى
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;
    private readonly ILogger<PatientsController> _logger;

    public PatientsController(IPatientService patientService, ILogger<PatientsController> logger)
    {
        _patientService = patientService;
        _logger = logger;
    }

    /// <summary>
    /// الحصول على جميع المرضى (مع Pagination)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ServiceResponse<PagedResult<PatientDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
    {
        var response = await _patientService.GetAllAsync(pagination);
        return Ok(response);
    }

    /// <summary>
    /// الحصول على مريض حسب ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ServiceResponse<PatientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await _patientService.GetByIdAsync(id);
        
        if (!response.IsSuccess)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>
    /// الحصول على مريض حسب الرقم الوطني
    /// </summary>
    [HttpGet("national/{nationalId}")]
    [ProducesResponseType(typeof(ServiceResponse<PatientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByNationalId(string nationalId)
    {
        var response = await _patientService.GetByNationalIdAsync(nationalId);
        
        if (!response.IsSuccess)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>
    /// الحصول على مريض مع طلباته
    /// </summary>
    [HttpGet("{id}/requests")]
    [ProducesResponseType(typeof(ServiceResponse<PatientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWithRequests(int id)
    {
        var response = await _patientService.GetWithRequestsAsync(id);
        
        if (!response.IsSuccess)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>
    /// إنشاء مريض جديد
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ServiceResponse<PatientDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePatientDto request)
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

        _logger.LogInformation("محاولة إنشاء مريض جديد: {NationalId}", request.NationalID);

        var response = await _patientService.CreateAsync(request);
        
        if (!response.IsSuccess)
            return BadRequest(response);

        _logger.LogInformation("تم إنشاء مريض جديد: {PatientId}", response.Data?.PatientID);
        return CreatedAtAction(nameof(GetById), new { id = response.Data?.PatientID }, response);
    }

    /// <summary>
    /// تحديث مريض
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ServiceResponse<PatientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePatientDto request)
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

        var response = await _patientService.UpdateAsync(id, request);
        
        if (!response.IsSuccess)
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>
    /// حذف مريض
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var response = await _patientService.DeleteAsync(id);
        
        if (!response.IsSuccess)
            return NotFound(response);

        return Ok(response);
    }
}
