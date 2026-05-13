using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RieltorCRM.Data;
using RieltorCRM.DTOs;
using RieltorCRM.Models;
using System.Security.Claims;

namespace RieltorCRM.backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Agent,Admin")]
    public class AgentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AgentController> _logger;

        public AgentController(ApplicationDbContext context, ILogger<AgentController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("properties")]
        public async Task<IActionResult> GetAssignedProperties(
            [FromQuery] PropertyStatus? status = null,
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = GetCurrentUserId();

                var query = _context.Properties
                    .Include(p => p.Seller)
                    .Where(p => p.AgentId == userId && p.IsActive);

                if (status.HasValue)
                    query = query.Where(p => p.Status == status.Value);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.ToLower();
                    query = query.Where(p =>
                        p.Title.ToLower().Contains(search) ||
                        p.Address.ToLower().Contains(search) ||
                        p.City!.ToLower().Contains(search));
                }

                var total = await query.CountAsync();
                var properties = await query
                    .OrderByDescending(p => p.CreatedAt)
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
                        p.CreatedAt,
                        SellerName = $"{p.Seller!.FirstName} {p.Seller.LastName}"
                    })
                    .ToListAsync();

                return Ok(new { properties, total, page, pageSize });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении объектов агента");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        [HttpGet("deals")]
        public async Task<IActionResult> GetAgentDeals(
            [FromQuery] DealStatus? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = GetCurrentUserId();

                var query = _context.Deals
                    .Include(d => d.Property)
                    .Include(d => d.Client)
                    .Include(d => d.Seller)
                    .Where(d => d.AgentId == userId);

                if (status.HasValue)
                    query = query.Where(d => d.Status == status.Value);

                var total = await query.CountAsync();
                var deals = await query
                    .OrderByDescending(d => d.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(d => new
                    {
                        d.Id,
                        d.DealNumber,
                        d.Type,
                        d.Status,
                        d.Amount,
                        d.Commission,
                        d.CreatedAt,
                        PropertyTitle = d.Property!.Title,
                        ClientName = $"{d.Client!.FirstName} {d.Client.LastName}",
                        SellerName = $"{d.Seller!.FirstName} {d.Seller.LastName}"
                    })
                    .ToListAsync();

                return Ok(new { deals, total, page, pageSize });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении сделок агента");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        [HttpPost("deals")]
        public async Task<IActionResult> CreateDeal([FromBody] CreateDealDto model)
        {
            try
            {
                var userId = GetCurrentUserId();

                var deal = new Deal
                {
                    DealNumber = $"DEAL-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                    Type = model.Type,
                    Status = DealStatus.Draft,
                    Amount = model.Amount,
                    CommissionPercent = model.CommissionPercent,
                    Commission = model.Amount * (model.CommissionPercent ?? 0) / 100,
                    Description = model.Description,
                    PropertyId = model.PropertyId,
                    AgentId = userId,
                    ClientId = model.ClientId,
                    SellerId = model.SellerId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Deals.Add(deal);

                var property = await _context.Properties.FindAsync(model.PropertyId);
                if (property != null)
                {
                    property.Status = PropertyStatus.UnderContract;
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Сделка создана", dealId = deal.Id, deal.DealNumber });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании сделки");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        [HttpPost("showings")]
        public async Task<IActionResult> ScheduleShowing([FromBody] CreateShowingDto model)
        {
            try
            {
                var userId = GetCurrentUserId();

                var property = await _context.Properties
                    .FirstOrDefaultAsync(p => p.Id == model.PropertyId && p.AgentId == userId);

                if (property == null)
                    return BadRequest(new { message = "Объект не найден или не закреплен за вами" });

                var showing = new Showing
                {
                    PropertyId = model.PropertyId,
                    AgentId = userId,
                    ClientId = model.ClientId,
                    ScheduledDate = model.ScheduledDate,
                    Notes = model.Notes,
                    Status = ShowingStatus.Scheduled
                };

                _context.Showings.Add(showing);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Показ запланирован", showingId = showing.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при планировании показа");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        [HttpPut("deals/{dealId}/status")]
        public async Task<IActionResult> UpdateDealStatus(int dealId, [FromBody] UpdateDealStatusDto model)
        {
            try
            {
                var userId = GetCurrentUserId();

                var deal = await _context.Deals
                    .FirstOrDefaultAsync(d => d.Id == dealId && d.AgentId == userId);

                if (deal == null)
                    return NotFound(new { message = "Сделка не найдена" });

                var oldStatus = deal.Status;
                deal.Status = model.Status;
                deal.UpdatedAt = DateTime.UtcNow;

                if (model.Status == DealStatus.Completed)
                {
                    deal.CompletedAt = DateTime.UtcNow;

                    var property = await _context.Properties.FindAsync(deal.PropertyId);
                    if (property != null)
                    {
                        property.Status = PropertyStatus.Sold;
                    }
                }

                var history = new DealHistory
                {
                    DealId = deal.Id,
                    OldStatus = oldStatus,
                    NewStatus = model.Status,
                    ChangedById = userId,
                    Comment = model.Comment,
                    ChangedAt = DateTime.UtcNow
                };

                _context.DealHistories.Add(history);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Статус сделки обновлен" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении статуса сделки");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        [HttpGet("report")]
        public async Task<IActionResult> GetAgentReport(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var userId = GetCurrentUserId();
                startDate ??= DateTime.UtcNow.AddMonths(-1);
                endDate ??= DateTime.UtcNow;

                var deals = await _context.Deals
                    .Where(d => d.AgentId == userId &&
                                d.CreatedAt >= startDate &&
                                d.CreatedAt <= endDate)
                    .GroupBy(d => d.Status)
                    .Select(g => new
                    {
                        Status = g.Key,
                        Count = g.Count(),
                        TotalAmount = g.Sum(d => d.Amount),
                        TotalCommission = g.Sum(d => d.Commission ?? 0)
                    })
                    .ToListAsync();

                var totalDeals = deals.Sum(d => d.Count);
                var totalAmount = deals.Sum(d => d.TotalAmount);
                var totalCommission = deals.Sum(d => d.TotalCommission);

                var showings = await _context.Showings
                    .Where(s => s.AgentId == userId &&
                                s.ScheduledDate >= startDate &&
                                s.ScheduledDate <= endDate)
                    .GroupBy(s => s.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync();

                return Ok(new
                {
                    period = new { start = startDate, end = endDate },
                    summary = new { totalDeals, totalAmount, totalCommission },
                    details = deals,
                    showings
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при формировании отчета");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        [HttpGet("clients")]
        public async Task<IActionResult> GetClients([FromQuery] string? search = null)
        {
            try
            {
                var query = _context.Users.Where(u => u.Role == UserRole.Client && u.IsActive);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.ToLower();
                    query = query.Where(u =>
                        u.FirstName.ToLower().Contains(search) ||
                        u.LastName.ToLower().Contains(search) ||
                        u.Email!.ToLower().Contains(search));
                }

                var clients = await query
                    .Select(u => new { u.Id, u.Email, u.FirstName, u.LastName, u.PhoneNumber })
                    .ToListAsync();

                return Ok(clients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении клиентов");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }
    }
}