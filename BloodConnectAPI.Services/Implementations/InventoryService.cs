using AutoMapper;
using BloodConnectAPI.DataAccess.Repositories.Interfaces;
using BloodConnectAPI.Models;
using BloodConnectAPI.Models.DTOs;
using BloodConnectAPI.Models.Enums;
using BloodConnectAPI.Service.Common;
using BloodConnectAPI.Service.Interfaces;

namespace BloodConnectAPI.Service.Implementations;

/// <summary>
/// خدمة إدارة مخزون الدم
/// </summary>
public class InventoryService : IInventoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public InventoryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ServiceResponse<IEnumerable<BloodInventoryDto>>> GetAllInventoryAsync()
    {
        var inventory = await _unitOfWork.BloodInventories.GetAllAsync();
        var dtos = _mapper.Map<IEnumerable<BloodInventoryDto>>(inventory);
        return ServiceResponse<IEnumerable<BloodInventoryDto>>.SuccessResponse(dtos);
    }

    public async Task<ServiceResponse<BloodInventoryDto>> GetByBloodTypeAsync(int bloodTypeId)
    {
        var inventory = await _unitOfWork.BloodInventories.GetByBloodTypeAsync(bloodTypeId);
        
        if (inventory == null)
            return ServiceResponse<BloodInventoryDto>.FailureResponse("المخزون غير موجود لهذه الفصيلة");

        var dto = _mapper.Map<BloodInventoryDto>(inventory);
        return ServiceResponse<BloodInventoryDto>.SuccessResponse(dto);
    }

    public async Task<ServiceResponse<IEnumerable<BloodInventoryDto>>> GetLowStockAsync(int threshold = 5)
    {
        var lowStock = await _unitOfWork.BloodInventories.GetLowStockAsync(threshold);
        var dtos = _mapper.Map<IEnumerable<BloodInventoryDto>>(lowStock);
        return ServiceResponse<IEnumerable<BloodInventoryDto>>.SuccessResponse(
            dtos,
            $"يوجد {lowStock.Count()} فصائل دم بكميات منخفضة"
        );
    }

    public async Task<ServiceResponse<bool>> UpdateQuantityAsync(int bloodTypeId, int quantityChange)
    {
        var inventory = await _unitOfWork.BloodInventories.GetByBloodTypeAsync(bloodTypeId);
        
        if (inventory == null)
            return ServiceResponse<bool>.FailureResponse("المخزون غير موجود لهذه الفصيلة");

        var newQuantity = inventory.QuantityAvailable + quantityChange;
        
        if (newQuantity < 0)
            return ServiceResponse<bool>.FailureResponse("الكمية المتاحة غير كافية");

        await _unitOfWork.BloodInventories.UpdateQuantityAsync(bloodTypeId, quantityChange);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResponse<bool>.SuccessResponse(
            true,
            $"تم تحديث الكمية بنجاح. الكمية الجديدة: {newQuantity}"
        );
    }

    public async Task<ServiceResponse<Dictionary<string, int>>> GetInventorySummaryAsync()
    {
        var allInventory = await _unitOfWork.BloodInventories.GetAllAsync();
        
        var summary = new Dictionary<string, int>();
        
        foreach (var item in allInventory)
        {
            var bloodType = await _unitOfWork.BloodTypes.GetByIdAsync(item.BloodTypeID);
            if (bloodType != null)
            {
                summary[bloodType.TypeName] = item.QuantityAvailable;
            }
        }

        return ServiceResponse<Dictionary<string, int>>.SuccessResponse(summary);
    }

    /// <summary>
    /// إضافة دم للمخزون مع تاريخ انتهاء الصلاحية
    /// </summary>
    public async Task<ServiceResponse<bool>> AddToInventoryAsync(
        int donationId,
        int bloodTypeId,
        int quantity)
    {
        var donation = await _unitOfWork.Donations.GetByIdAsync(donationId);
        if (donation == null)
            return ServiceResponse<bool>.FailureResponse("التبرع غير موجود");

        if (donation.TestResult != TestResult.Accepted)
            return ServiceResponse<bool>.FailureResponse("لا يمكن إضافة تبرع غير مقبول للمخزون");

        // حساب تاريخ انتهاء الصلاحية
        var expiryDate = DonationBusinessRules.CalculateBloodExpiryDate(donation.DonationDate);

        // Find BloodInventory
        var inventory = await _unitOfWork.BloodInventories.GetByBloodTypeAsync(bloodTypeId);
        if (inventory == null)
            return ServiceResponse<bool>.FailureResponse("المخزون غير موجود لهذه الفصيلة");

        // إنشاء InventoryItem
        var inventoryItem = new BloodInventoryItem
        {
            DonationID = donationId,
            InventoryID = inventory.InventoryID,
            Quantity = quantity,
            ExpiryDate = expiryDate,
            Status = BloodUnitStatus.Available,
            IsUsed = false,
            AddedAt = DateTime.UtcNow
        };

        await _unitOfWork.BloodInventoryItems.AddAsync(inventoryItem);

        // تحديث الكمية المتاحة
        await _unitOfWork.BloodInventories.UpdateQuantityAsync(bloodTypeId, quantity);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResponse<bool>.SuccessResponse(
            true,
            $"تم إضافة {quantity} وحدة للمخزون. تاريخ انتهاء الصلاحية: {expiryDate:yyyy-MM-dd}"
        );
    }

    /// <summary>
    /// صرف دم من المخزون (استخدام FIFO - الأقدم أولاً)
    /// </summary>
    public async Task<ServiceResponse<bool>> DisburseDonationAsync(
        int bloodTypeId,
        int quantity,
        int disbursementId)
    {
        var inventory = await _unitOfWork.BloodInventories.GetByBloodTypeAsync(bloodTypeId);
        if (inventory == null)
            return ServiceResponse<bool>.FailureResponse("فصيلة الدم غير موجودة في المخزون");

        if (inventory.QuantityAvailable < quantity)
            return ServiceResponse<bool>.FailureResponse(
                $"الكمية المتاحة غير كافية. المتوفر: {inventory.QuantityAvailable} وحدة");

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // جلب الوحدات غير المستخدمة مرتبة حسب تاريخ الانتهاء (FIFO)
            var availableItems = await _unitOfWork.BloodInventoryItems
                .GetAvailableItemsByBloodTypeAsync(bloodTypeId);

            int remainingQuantity = quantity;

            foreach (var item in availableItems)
            {
                if (remainingQuantity <= 0) break;

                int quantityToUse = Math.Min(item.Quantity, remainingQuantity);

                item.Quantity -= quantityToUse;
                if (item.Quantity == 0)
                {
                    item.IsUsed = true;
                    item.Status = BloodUnitStatus.Used;
                    item.UsedAt = DateTime.UtcNow;
                }

                await _unitOfWork.BloodInventoryItems.UpdateAsync(item);
                remainingQuantity -= quantityToUse;
            }

            // تحديث المخزون
            await _unitOfWork.BloodInventories.UpdateQuantityAsync(bloodTypeId, -quantity);

            await _unitOfWork.CommitTransactionAsync();

            return ServiceResponse<bool>.SuccessResponse(
                true,
                $"تم صرف {quantity} وحدة من فصيلة الدم بنجاح"
            );
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    /// <summary>
    /// حذف الوحدات منتهية الصلاحية
    /// </summary>
    public async Task<ServiceResponse<int>> RemoveExpiredUnitsAsync()
    {
        var expiredItems = await _unitOfWork.BloodInventoryItems.GetExpiredItemsAsync();

        if (!expiredItems.Any())
            return ServiceResponse<int>.SuccessResponse(0, "لا توجد وحدات منتهية الصلاحية");

        int totalRemoved = 0;

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            foreach (var item in expiredItems)
            {
                // تقليل الكمية من المخزون
                await _unitOfWork.BloodInventories.UpdateQuantityAsync(
                    item.Inventory.BloodTypeID,
                    -item.Quantity
                );

                // حذف الوحدة
                await _unitOfWork.BloodInventoryItems.DeleteAsync(item);
                totalRemoved += item.Quantity;
            }

            await _unitOfWork.CommitTransactionAsync();

            return ServiceResponse<int>.SuccessResponse(
                totalRemoved,
                $"تم حذف {totalRemoved} وحدة منتهية الصلاحية من {expiredItems.Count()} سجل"
            );
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    /// <summary>
    /// الحصول على الوحدات القريبة من انتهاء الصلاحية (خلال 7 أيام)
    /// </summary>
    public async Task<ServiceResponse<IEnumerable<BloodInventoryItemDto>>> GetExpiringItemsAsync(
        int daysThreshold = 7)
    {
        var expiringItems = await _unitOfWork.BloodInventoryItems
            .GetExpiringItemsAsync(daysThreshold);

        var dtos = _mapper.Map<IEnumerable<BloodInventoryItemDto>>(expiringItems);
        return ServiceResponse<IEnumerable<BloodInventoryItemDto>>.SuccessResponse(
            dtos,
            $"يوجد {expiringItems.Count()} وحدة ستنتهي خلال {daysThreshold} أيام"
        );
    }
    
    /// <summary>
    /// تحديث حالة وحدة المخزون
    /// </summary>
    public async Task<ServiceResponse<BloodInventoryItemDto>> UpdateItemStatusAsync(int itemId, BloodUnitStatus newStatus)
    {
        var item = await _unitOfWork.BloodInventoryItems.GetByIdAsync(itemId);
        if (item == null)
            return ServiceResponse<BloodInventoryItemDto>.FailureResponse("وحدة المخزون غير موجودة");

        item.Status = newStatus;
        
        if (newStatus == BloodUnitStatus.Used || newStatus == BloodUnitStatus.Discarded)
        {
            item.IsUsed = true;
            if (newStatus == BloodUnitStatus.Used && item.UsedAt == null)
                item.UsedAt = DateTime.UtcNow;
        }

        await _unitOfWork.BloodInventoryItems.UpdateAsync(item);
        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<BloodInventoryItemDto>(item);
        return ServiceResponse<BloodInventoryItemDto>.SuccessResponse(dto, "تم تحديث حالة الوحدة بنجاح");
    }
}
