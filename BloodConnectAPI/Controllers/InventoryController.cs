using BloodConnectAPI.Models.DTOs;
using BloodConnectAPI.Models.Enums;
using BloodConnectAPI.Service.Common;
using BloodConnectAPI.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodConnectAPI.Controllers;

/// <summary>
/// Controller لإدارة مخزون الدم
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(IInventoryService inventoryService, ILogger<InventoryController> logger)
    {
        _inventoryService = inventoryService;
        _logger = logger;
    }

    /// <summary>
    /// الحصول على جميع المخزون
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ServiceResponse<IEnumerable<BloodInventoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var response = await _inventoryService.GetAllInventoryAsync();
        return Ok(response);
    }

    /// <summary>
    /// الحصول على مخزون فصيلة دم معينة
    /// </summary>
    [HttpGet("blood-type/{bloodTypeId}")]
    [ProducesResponseType(typeof(ServiceResponse<BloodInventoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByBloodType(int bloodTypeId)
    {
        var response = await _inventoryService.GetByBloodTypeAsync(bloodTypeId);
        
        if (!response.IsSuccess)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>
    /// الحصول على فصائل الدم ذات المخزون المنخفض
    /// </summary>
    [HttpGet("low-stock")]
    [ProducesResponseType(typeof(ServiceResponse<IEnumerable<BloodInventoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLowStock([FromQuery] int threshold = 5)
    {
        var response = await _inventoryService.GetLowStockAsync(threshold);
        return Ok(response);
    }

    /// <summary>
    /// الحصول على ملخص المخزون
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ServiceResponse<Dictionary<string, int>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary()
    {
        var response = await _inventoryService.GetInventorySummaryAsync();
        return Ok(response);
    }

    /// <summary>
    /// تحديث كمية المخزون
    /// </summary>
    [HttpPut("blood-type/{bloodTypeId}")]
    [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateQuantity(int bloodTypeId, [FromBody] UpdateQuantityDto dto)
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

        _logger.LogInformation("محاولة تحديث كمية المخزون لفصيلة: {BloodTypeId} بمقدار: {Quantity}", 
            bloodTypeId, dto.QuantityChange);

        var response = await _inventoryService.UpdateQuantityAsync(bloodTypeId, dto.QuantityChange);
        
        if (!response.IsSuccess)
            return BadRequest(response);

        _logger.LogInformation("تم تحديث كمية المخزون لفصيلة: {BloodTypeId}", bloodTypeId);
        return Ok(response);
    }

    /// <summary>
    /// الحصول على وحدات الدم التي تقترب من انتهاء الصلاحية
    /// </summary>
    [HttpGet("expiring")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServiceResponse<IEnumerable<BloodInventoryItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpiring([FromQuery] int daysThreshold = 7)
    {
        var response = await _inventoryService.GetExpiringItemsAsync(daysThreshold);
        return Ok(response);
    }

    /// <summary>
    /// إزالة وحدات الدم منتهية الصلاحية
    /// </summary>
    [HttpPost("remove-expired")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServiceResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveExpired()
    {
        _logger.LogInformation("تشغيل عملية إزالة وحدات الدم منتهية الصلاحية");
        var response = await _inventoryService.RemoveExpiredUnitsAsync();
        return Ok(response);
    }

    /// <summary>
    /// تحديث حالة وحدة دم
    /// </summary>
    [HttpPatch("items/{id}/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServiceResponse<BloodInventoryItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateItemStatus(int id, [FromBody] BloodUnitStatus newStatus)
    {
        var response = await _inventoryService.UpdateItemStatusAsync(id, newStatus);
        if (!response.IsSuccess) return BadRequest(response);
        return Ok(response);
    }
}
