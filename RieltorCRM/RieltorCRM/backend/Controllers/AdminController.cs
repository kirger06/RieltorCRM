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
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ── Users ──────────────────────────────────────────────────────

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var query = _context.Users
                .Include(u => u.Company)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(u =>
                    (u.FirstName ?? "").ToLower().Contains(s) ||
                    (u.LastName ?? "").ToLower().Contains(s) ||
                    (u.Email ?? "").ToLower().Contains(s));
            }

            var total = await query.CountAsync();
            var users = await query
                .OrderBy(u => u.LastName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new
                {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.PhoneNumber,
                    Role = u.Role.ToString(),
                    u.IsActive,
                    u.CreatedAt,
                    CompanyName = u.Company != null ? u.Company.Name : null,
                    u.CompanyId
                })
                .ToListAsync();

            return Ok(new { total, page, pageSize, users });
        }

        [HttpPut("users/{id}/toggle-active")]
        public async Task<IActionResult> ToggleUserActive(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            return Ok(new { user.Id, user.IsActive });
        }

        // ── Properties moderation ──────────────────────────────────────

        [HttpGet("properties/pending")]
        public async Task<IActionResult> GetPendingProperties([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var query = _context.Properties
                .Include(p => p.Agent)
                .Where(p => p.Status == PropertyStatus.PendingApproval && p.IsActive);

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Type,
                    p.Status,
                    p.Price,
                    p.Area,
                    p.Rooms,
                    p.Address,
                    p.City,
                    p.ImageUrls,
                    p.Description,
                    AgentName = p.Agent != null ? $"{p.Agent.FirstName} {p.Agent.LastName}" : null,
                    AgentEmail = p.Agent != null ? p.Agent.Email : null,
                    p.CreatedAt,
                    p.UpdatedAt
                })
                .ToListAsync();

            return Ok(new { total, page, pageSize, items });
        }

        [HttpPost("properties/{id}/approve")]
        public async Task<IActionResult> ApproveProperty(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            if (property == null) return NotFound();

            property.Status = PropertyStatus.Available;
            property.RejectionReason = null;
            property.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { property.Id, property.Status });
        }

        [HttpPost("properties/{id}/reject")]
        public async Task<IActionResult> RejectProperty(int id, [FromBody] RejectPropertyDto dto)
        {
            var property = await _context.Properties.FindAsync(id);
            if (property == null) return NotFound();

            property.Status = PropertyStatus.ApprovalRejected;
            property.RejectionReason = dto.Reason;
            property.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { property.Id, property.Status, property.RejectionReason });
        }

        // ── Companies ──────────────────────────────────────────────────

        [HttpGet("companies")]
        public async Task<IActionResult> GetCompanies()
        {
            var companies = await _context.Companies
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Phone,
                    c.Address,
                    c.CreatedAt,
                    EmployeeCount = _context.Users.Count(u => u.CompanyId == c.Id)
                })
                .ToListAsync();

            return Ok(companies);
        }

        [HttpGet("companies/{id}/stats")]
        public async Task<IActionResult> GetCompanyStats(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null) return NotFound();

            var agentIds = await _context.Users
                .Where(u => u.CompanyId == id && u.Role == UserRole.Agent)
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

            return Ok(new { company = new { company.Id, company.Name }, propertyStats, bookingStats });
        }

        [HttpGet("companies/{id}/clients")]
        public async Task<IActionResult> GetCompanyClients(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null) return NotFound();

            var agentIds = await _context.Users
                .Where(u => u.CompanyId == id && u.Role == UserRole.Agent)
                .Select(u => u.Id)
                .ToListAsync();

            var rawClients = await _context.Bookings
                .Include(b => b.Client)
                .Include(b => b.Property)
                .Where(b => b.Property!.AgentId.HasValue && agentIds.Contains(b.Property.AgentId.Value))
                .ToListAsync();

            var clients = rawClients.Select(b => new
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
                PropertyPrice = b.Property.Price
            });

            return Ok(clients);
        }

        [HttpGet("companies/{id}/bookings")]
        public async Task<IActionResult> GetCompanyBookings(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null) return NotFound();

            var agentIds = await _context.Users
                .Where(u => u.CompanyId == id && u.Role == UserRole.Agent)
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
                PropertyTitle = b.Property!.Title,
                PropertyPrice = b.Property.Price
            });

            return Ok(new { total, page, pageSize, bookings });
        }

        [HttpPost("companies")]
        public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyDto dto)
        {
            var company = new Company
            {
                Name = dto.Name,
                Description = dto.Description,
                Phone = dto.Phone,
                Address = dto.Address,
                CreatedAt = DateTime.UtcNow
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCompanies), new { }, company);
        }
    }

    public class RejectPropertyDto
    {
        public string Reason { get; set; } = string.Empty;
    }

    public class CreateCompanyDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }
}
