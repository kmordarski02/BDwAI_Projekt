using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Wypozyczalnia.Models;

namespace Wypozyczalnia.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if ((await context.Database.GetPendingMigrationsAsync()).Any())
            {
                await context.Database.MigrateAsync();
            }

            string[] roles = new[] { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminEmail = "admin@test.pl";
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "Administrator"
                };
                var result = await userManager.CreateAsync(admin, "Haslo123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
                else
                {
                    throw new Exception("Nie udało się utworzyć konta admin. Sprawdź konfigurację Identity.");
                }
            }

            if (!await context.Categories.AnyAsync())
            {
                var cat1 = new Category { Name = "Rowerowy" };
                var cat2 = new Category { Name = "Wodne" };
                var cat3 = new Category { Name = "Zimowy" };

                context.Categories.AddRange(cat1, cat2, cat3);
                await context.SaveChangesAsync();

                context.EquipmentItems.AddRange(
                    new EquipmentItem { Name = "Rower górski", Season = "Wiosna/Lato/Jesień", Quantity = 5, CategoryId = cat1.Id },
                    new EquipmentItem { Name = "Kajak jednoosobowy", Season = "Lato", Quantity = 3, CategoryId = cat2.Id },
                    new EquipmentItem { Name = "Narty zjazdowe", Season = "Zima", Quantity = 4, CategoryId = cat3.Id }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
