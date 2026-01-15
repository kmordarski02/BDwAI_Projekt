using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Wypozyczalnia.Models;

namespace Wypozyczalnia.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            // Create a scope to get request services
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure database is created/migrated
            if ((await context.Database.GetPendingMigrationsAsync()).Any())
            {
                await context.Database.MigrateAsync();
            }
            else
            {
                await context.Database.EnsureCreatedAsync();
            }

            // 1. Roles
            string[] roleNames = { "Admin", "User" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Admin User
            var adminEmail = "admin@wypozyczalnia.pl";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser { UserName = adminEmail, Email = adminEmail, FullName = "Administrator", EmailConfirmed = true };
                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // 3. Normal User
            var userEmail = "user@wypozyczalnia.pl";
            var normalUser = await userManager.FindByEmailAsync(userEmail);
            if (normalUser == null)
            {
                normalUser = new ApplicationUser { UserName = userEmail, Email = userEmail, FullName = "Jan Kowalski", EmailConfirmed = true };
                var result = await userManager.CreateAsync(normalUser, "User123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(normalUser, "User");
                }
            }

            // 4. Categories
            var categories = new[]
            {
                "Rowery",
                "Wodne",
                "Hulajnogi i rolki",
                "Aktywny wypoczynek",
                "Sporty zręcznościowe",
                "Zimowy - Narciarski",
                "Zimowy - Snowboard"
            };

            foreach (var catName in categories)
            {
                if (!await context.Categories.AnyAsync(c => c.Name == catName))
                {
                    context.Categories.Add(new Category { Name = catName });
                }
            }
            await context.SaveChangesAsync();

            var bikeCat = await context.Categories.FirstAsync(c => c.Name == "Rowery");
            var winterCat = await context.Categories.FirstAsync(c => c.Name == "Zimowy - Narciarski");
            var boardCat = await context.Categories.FirstAsync(c => c.Name == "Zimowy - Snowboard");
            var waterCat = await context.Categories.FirstAsync(c => c.Name == "Wodne");
            var activeCat = await context.Categories.FirstAsync(c => c.Name == "Aktywny wypoczynek");

            // 5. Equipment
            var items = new List<EquipmentItem>
            {
                // === ROWEROWE ===
                new EquipmentItem { Name = "Rower Górski MTB", CategoryId = bikeCat.Id, Season = "Letni", Quantity = 5, PricePerHour = 30, TargetGender = TargetGender.Unisex, Size = "L" },
                new EquipmentItem { Name = "Rower Miejski", CategoryId = bikeCat.Id, Season = "Letni", Quantity = 5, PricePerHour = 20, TargetGender = TargetGender.Unisex, Size = "M" },
                new EquipmentItem { Name = "Rower Elektryczny E-Bike", CategoryId = bikeCat.Id, Season = "Letni", Quantity = 3, PricePerHour = 65, TargetGender = TargetGender.Unisex, Size = "L" },
                
                // === WODNE ===
                new EquipmentItem { Name = "Kajak Dwuosobowy", CategoryId = waterCat.Id, Season = "Letni", Quantity = 2, PricePerHour = 30, TargetGender = TargetGender.Unisex, Size = "Double" },
                new EquipmentItem { Name = "Kajak Jednoosobowy", CategoryId = waterCat.Id, Season = "Letni", Quantity = 3, PricePerHour = 30, TargetGender = TargetGender.Unisex, Size = "Single" },
                new EquipmentItem { Name = "Deska SUP (Stand Up Paddle)", CategoryId = waterCat.Id, Season = "Letni", Quantity = 4, PricePerHour = 35, TargetGender = TargetGender.Unisex, Size = "Standard" },
                new EquipmentItem { Name = "Deska Windsurfingowa", CategoryId = waterCat.Id, Season = "Letni", Quantity = 2, PricePerHour = 55, TargetGender = TargetGender.Unisex, Size = "140L" },

                // === INNE RETNIE (Hulajnogi, Rolki) ===
                new EquipmentItem { Name = "Hulajnoga Miejska", CategoryId = activeCat.Id, Season = "Letni", Quantity = 6, PricePerHour = 15, TargetGender = TargetGender.Unisex, Size = "Uni" },
                new EquipmentItem { Name = "Hulajnoga Elektryczna", CategoryId = activeCat.Id, Season = "Letni", Quantity = 4, PricePerHour = 40, TargetGender = TargetGender.Unisex, Size = "Uni" },
                new EquipmentItem { Name = "Rolki Fitness", CategoryId = activeCat.Id, Season = "Letni", Quantity = 8, PricePerHour = 15, TargetGender = TargetGender.Unisex, Size = "40-44" },
                new EquipmentItem { Name = "Wrotki Retro", CategoryId = activeCat.Id, Season = "Letni", Quantity = 4, PricePerHour = 15, TargetGender = TargetGender.Kobieta, Size = "38" },

                // === ZIMOWE - NARTY ===
                new EquipmentItem { Name = "Narty Zjazdowe Rossignol", CategoryId = winterCat.Id, Season = "Zimowy", Quantity = 10, PricePerHour = 40, TargetGender = TargetGender.Unisex, Size = "170cm" },
                new EquipmentItem { Name = "Buty Narciarskie", CategoryId = winterCat.Id, Season = "Zimowy", Quantity = 15, PricePerHour = 20, TargetGender = TargetGender.Unisex, Size = "42" },
                new EquipmentItem { Name = "Kijki Narciarskie", CategoryId = winterCat.Id, Season = "Zimowy", Quantity = 20, PricePerHour = 10, TargetGender = TargetGender.Unisex, Size = "125cm" },
                new EquipmentItem { Name = "Kask Narciarski", CategoryId = winterCat.Id, Season = "Zimowy", Quantity = 15, PricePerHour = 15, TargetGender = TargetGender.Unisex, Size = "L" },
                new EquipmentItem { Name = "Gogle Narciarskie", CategoryId = winterCat.Id, Season = "Zimowy", Quantity = 10, PricePerHour = 10, TargetGender = TargetGender.Unisex, Size = "Uni" },

                // === ZIMOWE - SNOWBOARD ===
                new EquipmentItem { Name = "Deska Snowboardowa Burton", CategoryId = boardCat.Id, Season = "Zimowy", Quantity = 4, PricePerHour = 40, TargetGender = TargetGender.Mężczyzna, Size = "155cm" },
                new EquipmentItem { Name = "Buty Snowboardowe", CategoryId = boardCat.Id, Season = "Zimowy", Quantity = 6, PricePerHour = 20, TargetGender = TargetGender.Unisex, Size = "43" }
            };

            foreach (var item in items)
            {
                if (!await context.EquipmentItems.AnyAsync(e => e.Name == item.Name && e.Size == item.Size))
                {
                    context.EquipmentItems.Add(item);
                }
            }
            await context.SaveChangesAsync();
        }
    }
}
