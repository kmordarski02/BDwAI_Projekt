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

            // 4. Categories & Equipment
            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Rowerowy" },
                    new Category { Name = "Wodne" },
                    new Category { Name = "Hulajnogi i rolki" },
                    new Category { Name = "Aktywny wypoczynek" },
                    new Category { Name = "Sporty zręcznościowe" },
                    new Category { Name = "Zimowy - Narciarski" },
                    new Category { Name = "Zimowy - Snowboard" }
                };
                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();

                // Get IDs (reload from DB to be safely consistent)
                var bikeCat = await context.Categories.FirstAsync(c => c.Name == "Rowerowy");
                var winterCat = await context.Categories.FirstAsync(c => c.Name == "Zimowy - Narciarski");
                var boardCat = await context.Categories.FirstAsync(c => c.Name == "Zimowy - Snowboard");
                var waterCat = await context.Categories.FirstAsync(c => c.Name == "Wodne");

                var items = new List<EquipmentItem>
                {
                    // Rowerowy
                    new EquipmentItem { Name = "Rower Górski MTB", CategoryId = bikeCat.Id, Season = "Summer", Quantity = 5, PricePerDay = 50, TargetGender = TargetGender.Unisex, Size = "L" },
                    new EquipmentItem { Name = "Rower Miejski", CategoryId = bikeCat.Id, Season = "Summer", Quantity = 3, PricePerDay = 40, TargetGender = TargetGender.Female, Size = "M" },
                    
                    // Wodne
                    new EquipmentItem { Name = "Kajak dwuosobowy", CategoryId = waterCat.Id, Season = "Summer", Quantity = 2, PricePerDay = 80, TargetGender = TargetGender.Unisex, Size = "Double" },

                    // Zima
                    new EquipmentItem { Name = "Narty Zjazdowe Rossignol", CategoryId = winterCat.Id, Season = "Winter", Quantity = 10, PricePerDay = 60, TargetGender = TargetGender.Unisex, Size = "170cm" },
                    new EquipmentItem { Name = "Deska Snowboardowa Burton", CategoryId = boardCat.Id, Season = "Winter", Quantity = 4, PricePerDay = 70, TargetGender = TargetGender.Male, Size = "155cm" },
                    new EquipmentItem { Name = "Buty Narciarskie", CategoryId = winterCat.Id, Season = "Winter", Quantity = 10, PricePerDay = 30, TargetGender = TargetGender.Unisex, Size = "42" }
                };

                context.EquipmentItems.AddRange(items);
                await context.SaveChangesAsync();
            }
        }
    }
}
