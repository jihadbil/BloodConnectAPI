using System;

namespace BloodConnectAPI.Models.DTOs;

/// <summary>
/// DTO لتلبية جزء أو كل طلب الدم
/// </summary>
public class FulfillBloodRequestDto
{
    /// <summary>
    /// الكمية المراد توفيرها في هذه العملية
    /// </summary>
    public int QuantityToFulfill { get; set; }
    
    /// <summary>
    /// معرف التبرع الذي سيتم صرفه لتلبية الطلب
    /// </summary>
    public int DonationId { get; set; }
}
