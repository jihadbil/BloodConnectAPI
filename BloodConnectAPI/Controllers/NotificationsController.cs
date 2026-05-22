using BloodConnectAPI.Models.DTOs;
using BloodConnectAPI.Service.Common;
using BloodConnectAPI.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BloodConnectAPI.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
[Produces("application/json")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationService notificationService,
        ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _logger              = logger;
    }

    // ─── Helper ───────────────────────────────────────────────────────────────

    private string GetCurrentUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("لم يتم التعرف على المستخدم");

    // ─── GET ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// جلب إشعارات المستخدم الحالي (آخر 30 يوم)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ServiceResponse<IEnumerable<NotificationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyNotifications([FromQuery] bool unreadOnly = false)
    {
        var userId   = GetCurrentUserId();
        var response = await _notificationService.GetMyNotificationsAsync(userId, unreadOnly);
        return Ok(response);
    }

    /// <summary>
    /// عدد الإشعارات غير المقروءة
    /// </summary>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(ServiceResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId   = GetCurrentUserId();
        var response = await _notificationService.GetUnreadCountAsync(userId);
        return Ok(response);
    }

    // ─── PATCH ────────────────────────────────────────────────────────────────

    /// <summary>
    /// تحديد إشعار واحد كمقروء
    /// </summary>
    [HttpPatch("{id:int}/read")]
    [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId   = GetCurrentUserId();
        var response = await _notificationService.MarkAsReadAsync(id, userId);

        if (!response.IsSuccess) return NotFound(response);
        return Ok(response);
    }

    /// <summary>
    /// تحديد جميع الإشعارات كمقروءة
    /// </summary>
    [HttpPatch("mark-all-read")]
    [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId   = GetCurrentUserId();
        var response = await _notificationService.MarkAllAsReadAsync(userId);
        return Ok(response);
    }

    // ─── DELETE ───────────────────────────────────────────────────────────────

    /// <summary>
    /// حذف إشعار
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var userId   = GetCurrentUserId();
        var response = await _notificationService.DeleteAsync(id, userId);

        if (!response.IsSuccess) return NotFound(response);
        return Ok(response);
    }
}
