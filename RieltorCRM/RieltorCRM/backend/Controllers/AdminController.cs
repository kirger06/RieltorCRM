using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RieltorCRM.Data;
using RieltorCRM.DTOs;
using RieltorCRM.Models;

namespace RieltorCRM.backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ApplicationDbContext context,
            UserManager<User> userManager,
            ILogger<AdminController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(
            [FromQuery] UserRole? role = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var query = _context.Users.AsQueryable();

                if (role.HasValue)
                    query = query.Where(u => u.Role == role.Value);

                if (isActive.HasValue)
                    query = query.Where(u => u.IsActive == isActive.Value);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.ToLower();
                    query = query.Where(u =>
                        u.FirstName.ToLower().Contains(search) ||
                        u.LastName.ToLower().Contains(search) ||
                        u.Email!.ToLower().Contains(search));
                }

                var total = await query.CountAsync();
                var users = await query
                    .OrderByDescending(u => u.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new
                    {
                        u.Id,
                        u.Email,
                        u.FirstName,
                        u.LastName,
                        u.Role,
                        u.PhoneNumber,
                        u.IsActive,
                        u.CreatedAt
                    })
                    .ToListAsync();

                return Ok(new { users, total, page, pageSize });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении пользователей");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto model)
        {
            try
            {
                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    MiddleName = model.MiddleName,
                    PhoneNumber = model.PhoneNumber,
                    Role = model.Role,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                    return BadRequest(new { errors = result.Errors });

                await _userManager.AddToRoleAsync(user, model.Role.ToString());

                return Ok(new
                {
                    message = "Пользователь создан",
                    user = new { user.Id, user.Email, user.FirstName, user.LastName, user.Role }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании пользователя");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        [HttpPut("users/{userId}/toggle-status")]
        public async Task<IActionResult> ToggleUserStatus(int userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                    return NotFound(new { message = "Пользователь не найден" });

                user.IsActive = !user.IsActive;
                await _userManager.UpdateAsync(user);

                return Ok(new { message = $"Пользователь {(user.IsActive ? "активирован" : "деактивирован")}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при изменении статуса пользователя");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                var totalUsers = await _context.Users.CountAsync();
                var totalProperties = await _context.Properties.CountAsync();
                var totalDeals = await _context.Deals.CountAsync();
                var activeDeals = await _context.Deals.CountAsync(d => d.Status == DealStatus.InProgress);
                var completedDeals = await _context.Deals.CountAsync(d => d.Status == DealStatus.Completed);

                var totalRevenue = await _context.Transactions
                    .Where(t => t.Status == TransactionStatus.Completed && t.Type == TransactionType.Income)
                    .SumAsync(t => t.Amount);

                var recentDeals = await _context.Deals
                    .Include(d => d.Property)
                    .Include(d => d.Agent)
                    .OrderByDescending(d => d.CreatedAt)
                    .Take(5)
                    .Select(d => new
                    {
                        d.Id,
                        d.DealNumber,
                        d.Amount,
                        d.Status,
                        d.CreatedAt,
                        PropertyTitle = d.Property!.Title,
                        AgentName = $"{d.Agent!.FirstName} {d.Agent.LastName}"
                    })
                    .ToListAsync();

                var usersByRole = await _context.Users
                    .GroupBy(u => u.Role)
                    .Select(g => new { Role = g.Key.ToString(), Count = g.Count() })
                    .ToListAsync();

                var propertiesByStatus = await _context.Properties
                    .GroupBy(p => p.Status)
                    .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                    .ToListAsync();

                return Ok(new
                {
                    totalUsers,
                    totalProperties,
                    totalDeals,
                    activeDeals,
                    completedDeals,
                    totalRevenue,
                    recentDeals,
                    usersByRole,
                    propertiesByStatus
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении дашборда");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }
    }
}