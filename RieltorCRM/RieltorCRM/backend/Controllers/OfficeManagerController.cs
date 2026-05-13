using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RieltorCRM.Data;
using RieltorCRM.DTOs;
using RieltorCRM.Models;

namespace RieltorCRM.backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "OfficeManager,Admin")]
    public class OfficeManagerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OfficeManagerController> _logger;

        public OfficeManagerController(ApplicationDbContext context, ILogger<OfficeManagerController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("properties")]
        public async Task<IActionResult> GetAllProperties(
            [FromQuery] PropertyStatus? status = null,
            [FromQuery] PropertyType? type = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string? city = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var query = _context.Properties
                    .Include(p => p.Seller)
                    .Include(p => p.Agent)
                    .Where(p => p.IsActive);

                if (status.HasValue)
                    query = query.Where(p => p.Status == status.Value);

                if (type.HasValue)
                    query = query.Where(p => p.Type == type.Value);

                if (minPrice.HasValue)
                    query = query.Where(p => p.Price >= minPrice.Value);

                if (maxPrice.HasValue)
                    query = query.Where(p => p.Price <= maxPrice.Value);

                if (!string.IsNullOrWhiteSpace(city))
                    query = query.Where(p => p.City == city);

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
                        p.City,
                        p.Address,
                        p.CreatedAt,
                        SellerName = $"{p.Seller!.FirstName} {p.Seller.LastName}",
                        AgentName = p.Agent != null ? $"{p.Agent.FirstName} {p.Agent.LastName}" : "Не назначен"
                    })
                    .ToListAsync();

                return Ok(new { properties, total, page, pageSize });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении объектов");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        [HttpPut("properties/{propertyId}/assign-agent")]
        public async Task<IActionResult> AssignAgent(int propertyId, [FromBody] AssignAgentDto model)
        {
            try
            {
                var property = await _context.Properties.FindAsync(propertyId);
                if (property == null)
                    return NotFound(new { message = "Объект не найден" });

                var agent = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == model.AgentId && u.Role == UserRole.Agent);

                if (agent == null)
                    return BadRequest(new { message = "Агент не найден" });

                property.AgentId = model.AgentId;
                property.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Агент назначен" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при назначении агента");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        [HttpPut("properties/{propertyId}/status")]
        public async Task<IActionResult> UpdatePropertyStatus(int propertyId, [FromBody] PropertyStatus status)
        {
            try
            {
                var property = await _context.Properties.FindAsync(propertyId);
                if (property == null)
                    return NotFound(new { message = "Объект не найден" });

                property.Status = status;
                property.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Статус объекта обновлен" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении статуса");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        [HttpGet("deals")]
        public async Task<IActionResult> GetAllDeals(
            [FromQuery] DealStatus? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var query = _context.Deals
                    .Include(d => d.Property)
                    .Include(d => d.Agent)
                    .Include(d => d.Client)
                    .Include(d => d.Seller)
                    .AsQueryable();

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
                        d.CompletedAt,
                        PropertyTitle = d.Property!.Title,
                        AgentName = $"{d.Agent!.FirstName} {d.Agent.LastName}",
                        ClientName = $"{d.Client!.FirstName} {d.Client.LastName}",
                        SellerName = $"{d.Seller!.FirstName} {d.Seller.LastName}"
                    })
                    .ToListAsync();

                return Ok(new { deals, total, page, pageSize });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении сделок");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        [HttpGet("agents")]
        public async Task<IActionResult> GetAgents()
        {
            try
            {
                var agents = await _context.Users
                    .Where(u => u.Role == UserRole.Agent && u.IsActive)
                    .Select(u => new { u.Id, u.Email, u.FirstName, u.LastName, u.PhoneNumber })
                    .ToListAsync();

                return Ok(agents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении агентов");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }
    }
}