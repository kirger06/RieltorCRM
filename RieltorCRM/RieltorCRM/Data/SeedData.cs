using Microsoft.AspNetCore.Identity;
using RieltorCRM.Models;


namespace RieltorCRM.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

            string[] roles = { "Admin", "Agent", "Client", "Seller", "OfficeManager", "Accountant" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<int> { Name = role });
                }
            }

            var testUsers = new[]
            {
                new { Email = "admin@rioter.ru", FirstName = "Администратор", LastName = "Системы", Role = UserRole.Admin, Password = "Admin123!" },
                new { Email = "agent@rioter.ru", FirstName = "Иван", LastName = "Риелторов", Role = UserRole.Agent, Password = "Agent123!" },
                new { Email = "client@rioter.ru", FirstName = "Петр", LastName = "Клиентов", Role = UserRole.Client, Password = "Client123!" },
                new { Email = "seller@rioter.ru", FirstName = "Мария", LastName = "Продавцова", Role = UserRole.Seller, Password = "Seller123!" },
                new { Email = "manager@rioter.ru", FirstName = "Ольга", LastName = "Офисная", Role = UserRole.OfficeManager, Password = "Manager123!" },
                new { Email = "buh@rioter.ru", FirstName = "Елена", LastName = "Бухгалтерова", Role = UserRole.Accountant, Password = "Buh123!" }
            };

            foreach (var testUser in testUsers)
            {
                var existingUser = await userManager.FindByEmailAsync(testUser.Email);
                if (existingUser == null)
                {
                    var user = new User
                    {
                        UserName = testUser.Email,
                        Email = testUser.Email,
                        FirstName = testUser.FirstName,
                        LastName = testUser.LastName,
                        Role = testUser.Role,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(user, testUser.Password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, testUser.Role.ToString());
                    }
                }
            }
        }
    }
}