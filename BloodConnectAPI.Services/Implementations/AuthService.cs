using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BloodConnectAPI.Models;
using BloodConnectAPI.Models.DTOs;
using BloodConnectAPI.Service.Common;
using BloodConnectAPI.Service.Interfaces;

namespace BloodConnectAPI.Service.Implementations;

/// <summary>
/// Implementation لخدمة المصادقة
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    public async Task<ServiceResponse<LoginResponseDto>> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.UserName);
        if (user == null)
        {
            return ServiceResponse<LoginResponseDto>.FailureResponse("اسم المستخدم أو كلمة المرور غير صحيحة");
        }

        if (!user.IsActive)
        {
            return ServiceResponse<LoginResponseDto>.FailureResponse("الحساب غير نشط");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!result.Succeeded)
        {
            return ServiceResponse<LoginResponseDto>.FailureResponse("اسم المستخدم أو كلمة المرور غير صحيحة");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var (tokenString, expiryDate) = GenerateJwtToken(user, roles);

        var userDto = new ApplicationUserDto
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

        var loginResponse = new LoginResponseDto
        {
            Token = tokenString,
            ExpiresAt = expiryDate,
            User = userDto
        };

        return ServiceResponse<LoginResponseDto>.SuccessResponse(loginResponse, "تم تسجيل الدخول بنجاح");
    }

    public async Task<ServiceResponse<LoginResponseDto>> RegisterAsync(RegisterDto dto)
    {
        var user = new ApplicationUser
        {
            UserName = dto.UserName,
            Email = dto.Email,
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return ServiceResponse<LoginResponseDto>.FailureResponse("فشل إنشاء الحساب", errors);
        }

        // تعيين دور المتبرع افتراضياً
        await _userManager.AddToRoleAsync(user, "Donor");

        // توليد JWT فوراً بعد التسجيل لتجنب خطوة تسجيل دخول منفصلة
        var roles = await _userManager.GetRolesAsync(user);
        var (tokenString, expiryDate) = GenerateJwtToken(user, roles);

        var userDto = new ApplicationUserDto
        {
            Id = user.Id,
            UserName = user.UserName!,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            Roles = roles.ToList()
        };

        var loginResponse = new LoginResponseDto
        {
            Token = tokenString,
            ExpiresAt = expiryDate,
            User = userDto
        };

        return ServiceResponse<LoginResponseDto>.SuccessResponse(loginResponse, "تم إنشاء الحساب بنجاح");
    }

    public async Task<ServiceResponse<bool>> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ServiceResponse<bool>.FailureResponse("المستخدم غير موجود");
        }

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return ServiceResponse<bool>.FailureResponse("فشل تغيير كلمة المرور", errors);
        }

        return ServiceResponse<bool>.SuccessResponse(true, "تم تغيير كلمة المرور بنجاح");
    }

    private (string tokenString, DateTime expiryDate) GenerateJwtToken(ApplicationUser user, IList<string> roles)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = Encoding.ASCII.GetBytes(jwtSettings["Key"] ?? "BloodConnect-Secret-Key-Min-32-Characters!");
        var expiryDays = Convert.ToDouble(jwtSettings["ExpiryInDays"] ?? "7");
        var expiryDate = DateTime.UtcNow.AddDays(expiryDays);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiryDate,
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return (tokenHandler.WriteToken(token), expiryDate);
    }
}
