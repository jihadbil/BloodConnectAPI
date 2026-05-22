using BloodConnectAPI.Models.DTOs;
using BloodConnectAPI.Service.Common;
using BloodConnectAPI.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodConnectAPI.Controllers;

/// <summary>
/// Controller لإدارة المستخدمين
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// الحصول على جميع المستخدمين (مع Pagination)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServiceResponse<PagedResult<ApplicationUserDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
    {
        var response = await _userService.GetAllAsync(pagination);
        return Ok(response);
    }

    /// <summary>
    /// الحصول على مستخدم حسب ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServiceResponse<ApplicationUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        var response = await _userService.GetByIdAsync(id);
        
        if (!response.IsSuccess)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>
    /// الحصول على أدوار مستخدم
    /// </summary>
    [HttpGet("{id}/roles")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServiceResponse<IEnumerable<string>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserRoles(string id)
    {
        var response = await _userService.GetUserRolesAsync(id);
        return Ok(response);
    }

    /// <summary>
    /// تحديث مستخدم
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServiceResponse<ApplicationUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateApplicationUserDto user)
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

        var response = await _userService.UpdateAsync(id, user);
        
        if (!response.IsSuccess)
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>
    /// حذف مستخدم
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {
        var response = await _userService.DeleteAsync(id);
        
        if (!response.IsSuccess)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>
    /// تفعيل/إلغاء تفعيل مستخدم
    /// </summary>
    [HttpPatch("{id}/toggle-active")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleActive(string id)
    {
        var response = await _userService.ToggleActiveAsync(id);
        
        if (!response.IsSuccess)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>
    /// تعيين دور لمستخدم
    /// </summary>
    [HttpPost("{userId}/assign-role")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignRole(string userId, [FromBody] AssignRoleDto dto)
    {
        var response = await _userService.AssignRoleAsync(userId, dto.RoleName);
        
        if (!response.IsSuccess)
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>
    /// إزالة دور من مستخدم
    /// </summary>
    [HttpDelete("{userId}/roles/{roleName}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveRole(string userId, string roleName)
    {
        var response = await _userService.RemoveRoleAsync(userId, roleName);
        return Ok(response);
    }

    /// <summary>
    /// الحصول على مستخدم مع معلومات المتبرع المرتبط
    /// </summary>
    [HttpGet("{id}/with-donor")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServiceResponse<ApplicationUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserWithDonor(string id)
    {
        var response = await _userService.GetWithDonorAsync(id);
        
        if (!response.IsSuccess)
            return NotFound(response);

        return Ok(response);
    }
}
