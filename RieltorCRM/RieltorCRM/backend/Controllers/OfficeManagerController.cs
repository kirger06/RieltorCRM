using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RieltorCRM.Data;
using RieltorCRM.Models;
using System.Security.Claims;

namespace RieltorCRM.Controllers
{
    [ApiController]
    [Route("api/manager")]
    [Authorize(Roles = "OfficeManager")]
    public class OfficeManagerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public OfficeManagerController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("company")]
        public async Task<IActionResult> GetMyCompany()
        {
            var managerId = GetCurrentUserId();
            if (managerId == 0) return Unauthorized();

            var manager = await _context.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == managerId);

            if (manager?.Company == null)
                return NotFound("Менеджер не привязан к компании");

            return Ok(new
            {
                manager.Company.Id,
                manager.Company.Name,
                manager.Company.Description,
                manager.Company.Phone,
                manager.Company.Address,
                manager.Company.CreatedAt
            });
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var companyId = await GetCurrentCompanyId();
            if (companyId == null) return Unauthorized("Менеджер не привязан к компании");

            var agentIds = await _context.Users
                .Where(u => u.CompanyId == companyId && u.Role == UserRole.Agent)
                .Select(u => u.Id)
                .ToListAsync();

            var propertyStats = await _context.Properties
                .Where(p => p.AgentId.HasValue && agentIds.Contains(p.AgentId.Value))
                .GroupBy(p => p.Status)
                .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                .ToListAsync();

            var bookingStats = await _context.Bookings
                .Include(b => b.Property)
                .Where(b => b.Property!.AgentId.HasValue && agentIds.Contains(b.Property.AgentId.Value))
                .GroupBy(b => b.Type)
                .Select(g => new { Type = g.Key.ToString(), Count = g.Count() })
                .ToListAsync();

            var revenuePrices = await _context.Bookings
                .Include(b => b.Property)
                .Where(b =>
                    b.Type == BookingType.Purchase &&
                    b.Status == BookingStatus.Active &&
                    b.Property!.AgentId.HasValue &&
                    agentIds.Contains(b.Property.AgentId.Value))
                .Select(b => b.Property!.Price)
                .ToListAsync();
            var totalRevenue = revenuePrices.Sum();

            return Ok(new { propertyStats, bookingStats, totalRevenue });
        }

        [HttpGet("clients")]
        public async Task<IActionResult> GetClients([FromQuery] string? search)
        {
            var companyId = await GetCurrentCompanyId();
            if (companyId == null) return Unauthorized("Менеджер не привязан к компании");

            var agentIds = await _context.Users
                .Where(u => u.CompanyId == companyId && u.Role == UserRole.Agent)
                .Select(u => u.Id)
                .ToListAsync();

            var query = _context.Bookings
                .Include(b => b.Client)
                .Include(b => b.Property)
                .Where(b => b.Property!.AgentId.HasValue && agentIds.Contains(b.Property.AgentId.Value));

            var rawBookings = await query
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            var bookings = rawBookings.Select(b => new
            {
                ClientId = b.Client!.Id,
                ClientName = $"{b.Client.FirstName} {b.Client.LastName}",
                b.Client.Email,
                b.Client.PhoneNumber,
                BookingId = b.Id,
                Type = b.Type.ToString(),
                Status = b.Status.ToString(),
                b.CreatedAt,
                PropertyTitle = b.Property!.Title,
                PropertyId = b.Property.Id,
                PropertyPrice = b.Property.Price,
                PropertyAddress = b.Property.Address
            }).ToList();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                bookings = bookings.Where(b =>
                    (b.ClientName ?? "").ToLower().Contains(s) ||
                    (b.Email ?? "").ToLower().Contains(s) ||
                    (b.PropertyTitle ?? "").ToLower().Contains(s))
                    .ToList();
            }

            return Ok(bookings);
        }

        [HttpGet("bookings")]
        public async Task<IActionResult> GetBookings([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var companyId = await GetCurrentCompanyId();
            if (companyId == null) return Unauthorized("Менеджер не привязан к компании");

            var agentIds = await _context.Users
                .Where(u => u.CompanyId == companyId && u.Role == UserRole.Agent)
                .Select(u => u.Id)
                .ToListAsync();

            var query = _context.Bookings
                .Include(b => b.Client)
                .Include(b => b.Property)
                .Where(b => b.Property!.AgentId.HasValue && agentIds.Contains(b.Property.AgentId.Value));

            var total = await query.CountAsync();
            var rawBk = await query
                .OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var bookings = rawBk.Select(b => new
            {
                b.Id,
                Type = b.Type.ToString(),
                Status = b.Status.ToString(),
                b.CreatedAt,
                ClientName = $"{b.Client!.FirstName} {b.Client.LastName}",
                ClientEmail = b.Client.Email,
                ClientPhone = b.Client.PhoneNumber,
                PropertyTitle = b.Property!.Title,
                PropertyPrice = b.Property.Price,
                PropertyAddress = b.Property.Address
            });

            return Ok(new { total, page, pageSize, bookings });
        }

        // ── User / role management ────────────────────────────────────

        [HttpGet("users")]
        public async Task<IActionResult> GetCompanyUsers()
        {
            var companyId = await GetCurrentCompanyId();
            if (companyId == null) return Unauthorized("Менеджер не привязан к компании");

            var users = await _context.Users
                .Where(u => u.CompanyId == companyId && u.Role != UserRole.OfficeManager)
                .Select(u => new
                {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.PhoneNumber,
                    Role = u.Role.ToString(),
                    u.IsActive,
                    u.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPost("users/{userId}/assign-agent")]
        public async Task<IActionResult> AssignAgent(int userId)
        {
            var companyId = await GetCurrentCompanyId();
            if (companyId == null) return Unauthorized("Менеджер не привязан к компании");

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return NotFound("Пользователь не найден");

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, "Agent");

            user.Role = UserRole.Agent;
            user.CompanyId = companyId;
            await _userManager.UpdateAsync(user);

            return Ok(new { user.Id, Role = user.Role.ToString(), CompanyId = companyId });
        }

        [HttpPost("users/{userId}/assign-accountant")]
        public async Task<IActionResult> AssignAccountant(int userId)
        {
            var companyId = await GetCurrentCompanyId();
            if (companyId == null) return Unauthorized("Менеджер не привязан к компании");

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return NotFound("Пользователь не найден");

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, "Accountant");

            user.Role = UserRole.Accountant;
            user.CompanyId = companyId;
            await _userManager.UpdateAsync(user);

            return Ok(new { user.Id, Role = user.Role.ToString(), CompanyId = companyId });
        }

        [HttpDelete("users/{userId}/remove")]
        public async Task<IActionResult> RemoveFromCompany(int userId)
        {
            var companyId = await GetCurrentCompanyId();
            if (companyId == null) return Unauthorized("Менеджер не привязан к компании");

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return NotFound("Пользователь не найден");

            if (user.CompanyId != companyId)
                return Forbid();

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, "Client");

            user.Role = UserRole.Client;
            user.CompanyId = null;
            await _userManager.UpdateAsync(user);

            return Ok(new { user.Id, Role = user.Role.ToString(), message = "Пользователь переведён в роль Client" });
        }

        private int GetCurrentUserId()
        {
            int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId);
            return userId;
        }

        private async Task<int?> GetCurrentCompanyId()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return null;

            var user = await _context.Users.FindAsync(userId);
            return user?.CompanyId;
        }
    }
}
