using BloodConnectAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BloodConnectAPI.DataAccess.Data;

/// <summary>
/// مسؤول عن إضافة البيانات الأولية لقاعدة البيانات
/// </summary>
public static class DataSeeder
{
    /// <summary>
    /// إضافة البيانات الأولية (Roles, BloodTypes, Inventory, Admin User)
    /// </summary>
    public static async Task SeedAsync(ApplicationDbContext context, Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager, Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole> roleManager)
    {
        // 1. Seed Roles using RoleManager
        string[] roleNames = { "Admin", "Doctor", "Nurse", "LabTechnician", "Donor", "Patient" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole(roleName));
            }
        }

        // 2. Seed BloodTypes
        if (!await context.BloodTypes.AnyAsync())
        {
            var bloodTypes = new List<BloodType>
            {
                new BloodType { TypeName = "A+",  CreatedAt = DateTime.UtcNow },
                new BloodType { TypeName = "A-",  CreatedAt = DateTime.UtcNow },
                new BloodType { TypeName = "B+",  CreatedAt = DateTime.UtcNow },
                new BloodType { TypeName = "B-",  CreatedAt = DateTime.UtcNow },
                new BloodType { TypeName = "AB+", CreatedAt = DateTime.UtcNow },
                new BloodType { TypeName = "AB-", CreatedAt = DateTime.UtcNow },
                new BloodType { TypeName = "O+",  CreatedAt = DateTime.UtcNow },
                new BloodType { TypeName = "O-",  CreatedAt = DateTime.UtcNow }
            };

            await context.BloodTypes.AddRangeAsync(bloodTypes);
            await context.SaveChangesAsync();
        }

        // 3. Seed BloodInventory
        if (!await context.BloodInventories.AnyAsync())
        {
            // اجلب الـ IDs الحقيقية بعد الإدراج — لا تفترض أنها 1-8
            var bloodTypeIds = await context.BloodTypes
                .Select(bt => bt.BloodTypeID)
                .ToListAsync();

            var inventories = bloodTypeIds.Select(id => new BloodInventory
            {
                BloodTypeID      = id,
                QuantityAvailable = 0,
                QuantityReserved  = 0,
                CreatedAt        = DateTime.UtcNow,
                LastUpdated      = DateTime.UtcNow
            }).ToList();

            await context.BloodInventories.AddRangeAsync(inventories);
            await context.SaveChangesAsync();
        }


        // 4. Seed Default Admin User using UserManager
        var adminEmail = "admin@bloodconnect.com";
        var adminUser = await userManager.FindByNameAsync("admin");
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = "admin",
                Email = adminEmail,
                FullName = "مدير النظام",
                PhoneNumber = "0000000000",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            var result = await userManager.CreateAsync(adminUser, "Admin@123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
