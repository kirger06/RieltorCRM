using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RieltorCRM.Data;
using RieltorCRM.Models;
using System.Security.Claims;
using System.Text;

namespace RieltorCRM.Controllers
{
    [ApiController]
    [Route("api/accountant")]
    [Authorize(Roles = "Accountant")]
    public class AccountantController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AccountantController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var companyId = await GetCurrentCompanyId();
            if (companyId == null) return Unauthorized("Бухгалтер не привязан к компании");

            var agents = await _context.Users
                .Where(u => u.CompanyId == companyId && u.Role == UserRole.Agent)
                .ToListAsync();

            var agentIds = agents.Select(u => u.Id).ToList();

            var allProperties = await _context.Properties
                .Where(p => p.AgentId.HasValue && agentIds.Contains(p.AgentId.Value))
                .ToListAsync();

            var propertyStats = allProperties
                .GroupBy(p => p.Status.ToString())
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            var allBookings = await _context.Bookings
                .Include(b => b.Property)
                .Include(b => b.Client)
                .Where(b => b.Property!.AgentId.HasValue && agentIds.Contains(b.Property.AgentId.Value))
                .ToListAsync();

            var bookingsByType = allBookings
                .GroupBy(b => b.Type.ToString())
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToList();

            var bookingsByStatus = allBookings
                .GroupBy(b => b.Status.ToString())
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            var purchaseBookings = allBookings
                .Where(b => b.Type == BookingType.Purchase && b.Status == BookingStatus.Active)
                .ToList();

            var totalRevenue = purchaseBookings.Sum(b => b.Property?.Price ?? 0);

            var reservedRevenue = allBookings
                .Where(b => b.Type == BookingType.Reserve && b.Status == BookingStatus.Active)
                .Sum(b => b.Property?.Price ?? 0);

            var monthlyRevenue = purchaseBookings
                .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
                .Select(g => new
                {
                    Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Revenue = g.Sum(b => b.Property?.Price ?? 0),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Period)
                .Take(12)
                .ToList();

            var agentStats = agents.Select(agent =>
            {
                var agentProps = allProperties.Where(p => p.AgentId == agent.Id).ToList();
                var agentPurchases = purchaseBookings.Where(b => b.Property?.AgentId == agent.Id).ToList();
                return new
                {
                    AgentName = $"{agent.FirstName} {agent.LastName}",
                    TotalProperties = agentProps.Count,
                    AvailableCount = agentProps.Count(p => p.Status == PropertyStatus.Available),
                    SoldCount = agentProps.Count(p => p.Status == PropertyStatus.Sold),
                    PendingCount = agentProps.Count(p => p.Status == PropertyStatus.PendingApproval),
                    Revenue = agentPurchases.Sum(b => b.Property?.Price ?? 0)
                };
            }).OrderByDescending(a => a.Revenue).ToList();

            var propertiesByCity = allProperties
                .Where(p => !string.IsNullOrEmpty(p.City))
                .GroupBy(p => p.City!)
                .Select(g => new { City = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            return Ok(new
            {
                propertyStats,
                bookingsByType,
                bookingsByStatus,
                totalRevenue,
                reservedRevenue,
                monthlyRevenue,
                agentStats,
                propertiesByCity
            });
        }

        [HttpGet("export/bookings")]
        public async Task<IActionResult> ExportBookings()
        {
            var companyId = await GetCurrentCompanyId();
            if (companyId == null) return Unauthorized("Бухгалтер не привязан к компании");

            var agentIds = await _context.Users
                .Where(u => u.CompanyId == companyId && u.Role == UserRole.Agent)
                .Select(u => u.Id)
                .ToListAsync();

            var bookings = await _context.Bookings
                .Include(b => b.Client)
                .Include(b => b.Property)
                    .ThenInclude(p => p!.Agent)
                .Where(b => b.Property!.AgentId.HasValue && agentIds.Contains(b.Property.AgentId.Value))
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("ID,Тип,Статус,Дата,Клиент,Email клиента,Телефон клиента,Объект,Адрес,Цена,Агент");

            foreach (var b in bookings)
            {
                var line = string.Join(",",
                    b.Id,
                    b.Type.ToString(),
                    b.Status.ToString(),
                    b.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                    EscapeCsv($"{b.Client?.FirstName} {b.Client?.LastName}"),
                    EscapeCsv(b.Client?.Email ?? ""),
                    EscapeCsv(b.Client?.PhoneNumber ?? ""),
                    EscapeCsv(b.Property?.Title ?? ""),
                    EscapeCsv(b.Property?.Address ?? ""),
                    b.Property?.Price.ToString("F2") ?? "",
                    EscapeCsv(b.Property?.Agent != null ? $"{b.Property.Agent.FirstName} {b.Property.Agent.LastName}" : "")
                );
                csv.AppendLine(line);
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
            return File(bytes, "text/csv", $"bookings_{DateTime.UtcNow:yyyyMMdd}.csv");
        }

        private static string EscapeCsv(string value)
        {
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
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
