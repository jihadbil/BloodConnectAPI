using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BloodConnectAPI.DataAccess.Repositories.Interfaces;
using BloodConnectAPI.Models;
using BloodConnectAPI.Models.DTOs;
using BloodConnectAPI.Service.Common;
using BloodConnectAPI.Service.Interfaces;

namespace BloodConnectAPI.Service.Implementations;

/// <summary>
/// خدمة إدارة المستخدمين
/// </summary>
public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ServiceResponse<ApplicationUserDto>> GetByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return ServiceResponse<ApplicationUserDto>.FailureResponse("المستخدم غير موجود");

        var roles = await _userManager.GetRolesAsync(user);
        
        var dto = new ApplicationUserDto
        {
            Id = user.Id,
            UserName = user.UserName!,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Roles = roles.ToList()
        };

        return ServiceResponse<ApplicationUserDto>.SuccessResponse(dto);
    }

    public async Task<ServiceResponse<PagedResult<ApplicationUserDto>>> GetAllAsync(PaginationParams pagination)
    {
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrEmpty(pagination.SearchTerm))
        {
            query = query.Where(u => u.UserName!.Contains(pagination.SearchTerm) || 
                                     u.FullName.Contains(pagination.SearchTerm));
        }

        var totalCount = await query.CountAsync();

        if (pagination.SortDescending)
            query = query.OrderByDescending(u => u.CreatedAt);
        else
            query = query.OrderBy(u => u.CreatedAt);

        var users = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        var dtos = new List<ApplicationUserDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            dtos.Add(new ApplicationUserDto
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Roles = roles.ToList()
            });
        }

        var result = new PagedResult<ApplicationUserDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };

        return ServiceResponse<PagedResult<ApplicationUserDto>>.SuccessResponse(result);
    }

    public async Task<ServiceResponse<ApplicationUserDto>> UpdateAsync(string id, UpdateApplicationUserDto dto)
    {
        var existing = await _userManager.FindByIdAsync(id);
        if (existing == null)
            return ServiceResponse<ApplicationUserDto>.FailureResponse("المستخدم غير موجود");

        if (dto.FullName != null)
            existing.FullName = dto.FullName;
            
        if (dto.PhoneNumber != null)
            existing.PhoneNumber = dto.PhoneNumber;
            
        if (dto.IsActive.HasValue)
            existing.IsActive = dto.IsActive.Value;
            
        existing.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(existing);
        
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return ServiceResponse<ApplicationUserDto>.FailureResponse("فشل تحديث المستخدم", errors);
        }

        var roles = await _userManager.GetRolesAsync(existing);
        var userDto = new ApplicationUserDto
        {
            Id = existing.Id,
            UserName = existing.UserName!,
            Email = existing.Email ?? string.Empty,
            FullName = existing.FullName,
            PhoneNumber = existing.PhoneNumber,
            IsActive = existing.IsActive,
            CreatedAt = existing.CreatedAt,
            UpdatedAt = existing.UpdatedAt,
            Roles = roles.ToList()
        };

        return ServiceResponse<ApplicationUserDto>.SuccessResponse(userDto, "تم تحديث المستخدم بنجاح");
    }

    public async Task<ServiceResponse<bool>> DeleteAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return ServiceResponse<bool>.FailureResponse("المستخدم غير موجود");

        // 1) إزالة جميع أدوار المستخدم أولاً لتفادي FK violation في AspNetUserRoles
        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Any())
        {
            var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, roles);
            if (!removeRolesResult.Succeeded)
                return ServiceResponse<bool>.FailureResponse("فشل إزالة أدوار المستخدم قبل الحذف");
        }

        // 2) إزالة جميع Claims المرتبطة
        var claims = await _userManager.GetClaimsAsync(user);
        if (claims.Any())
            await _userManager.RemoveClaimsAsync(user, claims);

        // 3) حذف المستخدم
        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
            return ServiceResponse<bool>.FailureResponse("فشل حذف المستخدم");

        return ServiceResponse<bool>.SuccessResponse(true, "تم حذف المستخدم بنجاح");
    }

    public async Task<ServiceResponse<bool>> ToggleActiveAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return ServiceResponse<bool>.FailureResponse("المستخدم غير موجود");

        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        
        if (!result.Succeeded)
            return ServiceResponse<bool>.FailureResponse("فشل تغيير حالة المستخدم");

        return ServiceResponse<bool>.SuccessResponse(
            true,
            user.IsActive ? "تم تفعيل المستخدم" : "تم إلغاء تفعيل المستخدم"
        );
    }

    public async Task<ServiceResponse<IEnumerable<string>>> GetUserRolesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return ServiceResponse<IEnumerable<string>>.FailureResponse("المستخدم غير موجود");

        var roles = await _userManager.GetRolesAsync(user);
        return ServiceResponse<IEnumerable<string>>.SuccessResponse(roles);
    }

    public async Task<ServiceResponse<bool>> AssignRoleAsync(string userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return ServiceResponse<bool>.FailureResponse("المستخدم غير موجود");

        if (!await _roleManager.RoleExistsAsync(roleName))
            return ServiceResponse<bool>.FailureResponse("الدور غير موجود");

        var result = await _userManager.AddToRoleAsync(user, roleName);
        
        if (!result.Succeeded)
            return ServiceResponse<bool>.FailureResponse("فشل تعيين الدور");

        return ServiceResponse<bool>.SuccessResponse(true, "تم تعيين الدور بنجاح");
    }

    public async Task<ServiceResponse<bool>> RemoveRoleAsync(string userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return ServiceResponse<bool>.FailureResponse("المستخدم غير موجود");

        var result = await _userManager.RemoveFromRoleAsync(user, roleName);
        
        if (!result.Succeeded)
            return ServiceResponse<bool>.FailureResponse("فشل إزالة الدور");

        return ServiceResponse<bool>.SuccessResponse(true, "تم إزالة الدور بنجاح");
    }

    public async Task<ServiceResponse<ApplicationUserDto>> GetWithDonorAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        
        if (user == null)
            return ServiceResponse<ApplicationUserDto>.FailureResponse("المستخدم غير موجود");

        var roles = await _userManager.GetRolesAsync(user);
        var donor = await _unitOfWork.Donors.FirstOrDefaultAsync(d => d.UserId == userId);

        var dto = new ApplicationUserDto
        {
            Id = user.Id,
            UserName = user.UserName!,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Roles = roles.ToList(),
            DonorID = donor?.DonorID,
            DonorName = donor?.FullName
        };

        return ServiceResponse<ApplicationUserDto>.SuccessResponse(dto);
    }
}
