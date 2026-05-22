using BloodConnectAPI.Models;
using BloodConnectAPI.Models.DTOs;
using BloodConnectAPI.Service.Common;

namespace BloodConnectAPI.Service.Interfaces;

/// <summary>
/// خدمة المصادقة والتسجيل
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// تسجيل الدخول
    /// </summary>
    Task<ServiceResponse<LoginResponseDto>> LoginAsync(LoginDto dto);

    /// <summary>
    /// إنشاء حساب جديد — يُعيد Token مباشرةً لتمكين الـ Frontend من الاستمرار بدون login منفصل
    /// </summary>
    Task<ServiceResponse<LoginResponseDto>> RegisterAsync(RegisterDto dto);

    /// <summary>
    /// تغيير كلمة المرور
    /// </summary>
    Task<ServiceResponse<bool>> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
}
