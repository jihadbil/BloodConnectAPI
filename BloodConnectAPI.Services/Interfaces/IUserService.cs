using BloodConnectAPI.Models.DTOs;
using BloodConnectAPI.Service.Common;

namespace BloodConnectAPI.Service.Interfaces;

/// <summary>
/// خدمة إدارة المستخدمين
/// </summary>
public interface IUserService
{
    // CRUD
    Task<ServiceResponse<ApplicationUserDto>> GetByIdAsync(string id);
    Task<ServiceResponse<PagedResult<ApplicationUserDto>>> GetAllAsync(PaginationParams pagination);
    Task<ServiceResponse<ApplicationUserDto>> UpdateAsync(string id, UpdateApplicationUserDto dto);
    Task<ServiceResponse<bool>> DeleteAsync(string id);
    Task<ServiceResponse<bool>> ToggleActiveAsync(string id);
    
    // Roles (Identity)
    Task<ServiceResponse<IEnumerable<string>>> GetUserRolesAsync(string userId);
    Task<ServiceResponse<bool>> AssignRoleAsync(string userId, string roleName);
    Task<ServiceResponse<bool>> RemoveRoleAsync(string userId, string roleName);
    
    /// <summary>
    /// الحصول على مستخدم مع معلومات المتبرع المرتبط
    /// </summary>
    /// <param name="userId">معرف المستخدم</param>
    /// <returns>استجابة الخدمة تحتوي على بيانات المستخدم مع معلومات المتبرع</returns>
    Task<ServiceResponse<ApplicationUserDto>> GetWithDonorAsync(string userId);
}
