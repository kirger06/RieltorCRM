using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RieltorCRM.Models;

namespace RieltorCRM.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            string[] roles = { "Admin", "Agent", "Client", "Seller", "OfficeManager", "Accountant" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<int> { Name = role });
                }
            }

            // Create default company
            Company? company = null;
            if (!await context.Companies.AnyAsync())
            {
                company = new Company
                {
                    Name = "Риелтор Плюс",
                    Description = "Ведущее агентство недвижимости",
                    Phone = "+7 (495) 123-45-67",
                    Address = "Москва, ул. Тверская, 1",
                    CreatedAt = DateTime.UtcNow
                };
                context.Companies.Add(company);
                await context.SaveChangesAsync();
            }
            else
            {
                company = await context.Companies.FirstAsync();
            }

            var testUsers = new[]
            {
                new { Email = "admin@rioter.ru", FirstName = "Администратор", LastName = "Системы", Role = UserRole.Admin, Password = "Admin123!", CompanyId = (int?)null },
                new { Email = "agent@rioter.ru", FirstName = "Иван", LastName = "Риелторов", Role = UserRole.Agent, Password = "Agent123!", CompanyId = (int?)company.Id },
                new { Email = "client@rioter.ru", FirstName = "Петр", LastName = "Клиентов", Role = UserRole.Client, Password = "Client123!", CompanyId = (int?)null },
                new { Email = "seller@rioter.ru", FirstName = "Мария", LastName = "Продавцова", Role = UserRole.Seller, Password = "Seller123!", CompanyId = (int?)null },
                new { Email = "manager@rioter.ru", FirstName = "Ольга", LastName = "Офисная", Role = UserRole.OfficeManager, Password = "Manager123!", CompanyId = (int?)company.Id },
                new { Email = "buh@rioter.ru", FirstName = "Елена", LastName = "Бухгалтерова", Role = UserRole.Accountant, Password = "Buh123!", CompanyId = (int?)company.Id }
            };

            User? agentUser = null;
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
                        EmailConfirmed = true,
                        CompanyId = testUser.CompanyId
                    };

                    var result = await userManager.CreateAsync(user, testUser.Password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, testUser.Role.ToString());
                        if (testUser.Role == UserRole.Agent)
                            agentUser = user;
                    }
                }
                else
                {
                    if (existingUser.CompanyId != testUser.CompanyId)
                    {
                        existingUser.CompanyId = testUser.CompanyId;
                        await userManager.UpdateAsync(existingUser);
                    }
                    if (testUser.Role == UserRole.Agent)
                        agentUser = existingUser;
                }
            }

            // Seed demo properties
            if (!await context.Properties.AnyAsync())
            {
                if (agentUser == null)
                    agentUser = await userManager.FindByEmailAsync("agent@rioter.ru");

                var properties = new[]
                {
                    new Property
                    {
                        Title = "3-комнатная квартира на Арбате",
                        Description = "Просторная квартира в самом центре Москвы. Евроремонт, встроенная кухня, паркинг.",
                        Type = PropertyType.Apartment,
                        Status = PropertyStatus.Available,
                        Price = 18500000,
                        Area = 87,
                        Rooms = 3,
                        Floor = 5,
                        TotalFloors = 12,
                        Address = "ул. Арбат, 15, кв. 47",
                        City = "Москва",
                        District = "Арбат",
                        AgentId = agentUser?.Id,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Property
                    {
                        Title = "1-комнатная квартира в Химках",
                        Description = "Уютная однокомнатная квартира рядом с метро. Чистый подъезд, хорошие соседи.",
                        Type = PropertyType.Apartment,
                        Status = PropertyStatus.Available,
                        Price = 5200000,
                        Area = 38,
                        Rooms = 1,
                        Floor = 3,
                        TotalFloors = 9,
                        Address = "ул. Ленинградская, 42, кв. 12",
                        City = "Химки",
                        District = "Центральный",
                        AgentId = agentUser?.Id,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Property
                    {
                        Title = "Загородный дом в Подмосковье",
                        Description = "Двухэтажный дом 200 кв.м. с участком 10 соток. Гараж, баня, газ, вода.",
                        Type = PropertyType.House,
                        Status = PropertyStatus.Available,
                        Price = 12000000,
                        Area = 200,
                        Rooms = 5,
                        Floor = 2,
                        TotalFloors = 2,
                        Address = "с. Николо-Урюпино, ул. Лесная, 7",
                        City = "Красногорск",
                        District = "Подмосковье",
                        AgentId = agentUser?.Id,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Property
                    {
                        Title = "Коммерческое помещение на Садовом кольце",
                        Description = "Офис 120 кв.м. на первом этаже. Отдельный вход, витринные окна.",
                        Type = PropertyType.Commercial,
                        Status = PropertyStatus.Available,
                        Price = 25000000,
                        Area = 120,
                        Rooms = null,
                        Floor = 1,
                        TotalFloors = 7,
                        Address = "Садовое кольцо, 28",
                        City = "Москва",
                        District = "Садовое кольцо",
                        AgentId = agentUser?.Id,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Property
                    {
                        Title = "2-комнатная квартира в ЖК Новое Очаково",
                        Description = "Новостройка с отделкой. Закрытый двор, детская площадка, подземный паркинг.",
                        Type = PropertyType.Apartment,
                        Status = PropertyStatus.Available,
                        Price = 9800000,
                        Area = 58,
                        Rooms = 2,
                        Floor = 8,
                        TotalFloors = 25,
                        Address = "ЖК Новое Очаково, корп. 3, кв. 201",
                        City = "Москва",
                        District = "Очаково",
                        AgentId = agentUser?.Id,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.Properties.AddRange(properties);
                await context.SaveChangesAsync();
            }
        }
    }
}
