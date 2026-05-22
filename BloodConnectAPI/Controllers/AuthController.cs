using BloodConnectAPI.Models.DTOs;
using BloodConnectAPI.Service.Common;
using BloodConnectAPI.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BloodConnectAPI.Controllers;

/// <summary>
/// Controller للمصادقة والتسجيل
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// تسجيل الدخول
    /// </summary>
    /// <param name="request">بيانات تسجيل الدخول</param>
    /// <returns>بيانات المستخدم مع UserRoleId</returns>
    /// <response code="200">تم تسجيل الدخول بنجاح</response>
    /// <response code="400">بيانات غير صحيحة</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ServiceResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ServiceResponse<object>
            {
                IsSuccess = false,
                Message = "بيانات غير صحيحة",
                Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()
            });
        }

        _logger.LogInformation("محاولة تسجيل دخول للمستخدم: {Username}", request.UserName);

        var response = await _authService.LoginAsync(request);

        if (!response.IsSuccess)
        {
            _logger.LogWarning("فشل تسجيل الدخول للمستخدم: {Username}", request.UserName);
            return BadRequest(response);
        }

        _logger.LogInformation("تم تسجيل دخول المستخدم بنجاح: {Username}", request.UserName);
        return Ok(response);
    }

    /// <summary>
    /// إنشاء حساب جديد
    /// </summary>
    /// <param name="request">بيانات التسجيل</param>
    /// <returns>بيانات المستخدم الجديد</returns>
    /// <response code="201">تم إنشاء الحساب بنجاح</response>
    /// <response code="400">بيانات غير صحيحة أو اسم المستخدم موجود</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ServiceResponse<LoginResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ServiceResponse<object>
            {
                IsSuccess = false,
                Message = "بيانات غير صحيحة",
                Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()
            });
        }

        _logger.LogInformation("محاولة إنشاء حساب جديد: {Username}", request.UserName);

        var response = await _authService.RegisterAsync(request);

        if (!response.IsSuccess)
        {
            _logger.LogWarning("فشل إنشاء حساب: {Username}", request.UserName);
            return BadRequest(response);
        }

        _logger.LogInformation("تم إنشاء حساب جديد بنجاح: {Username}", request.UserName);
        return CreatedAtAction(nameof(Login), response);
    }

    /// <summary>
    /// تغيير كلمة المرور
    /// </summary>
    /// <param name="request">بيانات تغيير كلمة المرور</param>
    /// <returns>نتيجة العملية</returns>
    /// <response code="200">تم تغيير كلمة المرور بنجاح</response>
    /// <response code="400">بيانات غير صحيحة</response>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ServiceResponse<object>
            {
                IsSuccess = false,
                Message = "بيانات غير صحيحة",
                Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()
            });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        _logger.LogInformation("محاولة تغيير كلمة المرور للمستخدم: {UserId}", userId);

        var response = await _authService.ChangePasswordAsync(
            userId,
            request.CurrentPassword,
            request.NewPassword
        );

        if (!response.IsSuccess)
        {
            _logger.LogWarning("فشل تغيير كلمة المرور للمستخدم: {UserId}", userId);
            return BadRequest(response);
        }

        _logger.LogInformation("تم تغيير كلمة المرور بنجاح للمستخدم: {UserId}", userId);
        return Ok(response);
    }
}
