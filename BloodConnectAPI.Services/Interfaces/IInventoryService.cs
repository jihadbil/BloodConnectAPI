using BloodConnectAPI.Models;
using BloodConnectAPI.Models.DTOs;
using BloodConnectAPI.Models.Enums;
using BloodConnectAPI.Service.Common;

namespace BloodConnectAPI.Service.Interfaces;

/// <summary>
/// خدمة إدارة مخزون الدم
/// </summary>
public interface IInventoryService
{
    Task<ServiceResponse<IEnumerable<BloodInventoryDto>>> GetAllInventoryAsync();
    Task<ServiceResponse<BloodInventoryDto>> GetByBloodTypeAsync(int bloodTypeId);
    Task<ServiceResponse<IEnumerable<BloodInventoryDto>>> GetLowStockAsync(int threshold = 5);
    Task<ServiceResponse<bool>> UpdateQuantityAsync(int bloodTypeId, int quantityChange);
    Task<ServiceResponse<Dictionary<string, int>>> GetInventorySummaryAsync();

    // إدارة وحدات المخزون مع انتهاء الصلاحية
    Task<ServiceResponse<bool>> AddToInventoryAsync(int donationId, int bloodTypeId, int quantity);
    Task<ServiceResponse<bool>> DisburseDonationAsync(int bloodTypeId, int quantity, int disbursementId);
    Task<ServiceResponse<int>> RemoveExpiredUnitsAsync();
    Task<ServiceResponse<IEnumerable<BloodInventoryItemDto>>> GetExpiringItemsAsync(int daysThreshold = 7);
    
    // إدارة حالة الوحدات
    Task<ServiceResponse<BloodInventoryItemDto>> UpdateItemStatusAsync(int itemId, BloodUnitStatus newStatus);
}
