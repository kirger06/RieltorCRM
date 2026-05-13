using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RieltorCRM.Data;
using RieltorCRM.Models;

namespace RieltorCRM.backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Client")]
    public class ClientController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ClientController> _logger;

        public ClientController(ApplicationDbContext context, ILogger<ClientController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("properties")]
        public async Task<IActionResult> SearchProperties(
            [FromQuery] string? search = null,
            [FromQuery] PropertyType? type = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string? city = null,
            [FromQuery] int? rooms = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12)
        {
            try
            {
                var query = _context.Properties
                    .Include(p => p.Agent)
                    .Where(p => p.Status == PropertyStatus.Available && p.IsActive);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.ToLower();
                    query = query.Where(p =>
                        p.Title.ToLower().Contains(search) ||
                        p.Description!.ToLower().Contains(search));
                }

                if (type.HasValue)
                    query = query.Where(p => p.Type == type.Value);

                if (minPrice.HasValue)
                    query = query.Where(p => p.Price >= minPrice.Value);

                if (maxPrice.HasValue)
                    query = query.Where(p => p.Price <= maxPrice.Value);

                if (!string.IsNullOrWhiteSpace(city))
                    query = query.Where(p => p.City!.ToLower() == city.ToLower());

                if (rooms.HasValue)
                    query = query.Where(p => p.Rooms == rooms.Value);

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
                        p.Price,
                        p.Area,
                        p.Rooms,
                        p.Floor,
                        p.TotalFloors,
                        p.Address,
                        p.City,
                        p.District,
                        p.Description,
                        p.ImageUrls,
                        AgentName = p.Agent != null ? $"{p.Agent.FirstName} {p.Agent.LastName}" : null,
                        AgentPhone = p.Agent != null ? p.Agent.PhoneNumber : null
                    })
                    .ToListAsync();

                return Ok(new { properties, total, page, pageSize });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при поиске объектов");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        [HttpGet("properties/{id}")]
        public async Task<IActionResult> GetPropertyDetail(int id)
        {
            try
            {
                var property = await _context.Properties
                    .Include(p => p.Agent)
                    .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

                if (property == null)
                    return NotFound(new { message = "Объект не найден" });

                return Ok(new
                {
                    property.Id,
                    property.Title,
                    property.Type,
                    property.Price,
                    property.Area,
                    property.Rooms,
                    property.Floor,
                    property.TotalFloors,
                    property.Address,
                    property.City,
                    property.District,
                    property.Description,
                    property.Features,
                    property.ImageUrls,
                    AgentName = property.Agent != null ? $"{property.Agent.FirstName} {property.Agent.LastName}" : null,
                    AgentPhone = property.Agent?.PhoneNumber,
                    AgentEmail = property.Agent?.Email
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении объекта");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        [HttpGet("my-deals")]
        public async Task<IActionResult> GetMyDeals()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

                var deals = await _context.Deals
                    .Include(d => d.Property)
                    .Include(d => d.Agent)
                    .Where(d => d.ClientId == userId)
                    .OrderByDescending(d => d.CreatedAt)
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
                        PropertyAddress = d.Property.Address,
                        AgentName = $"{d.Agent!.FirstName} {d.Agent.LastName}",
                        AgentPhone = d.Agent.PhoneNumber,
                        AgentEmail = d.Agent.Email
                    })
                    .ToListAsync();

                return Ok(deals);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении сделок клиента");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        [HttpGet("my-showings")]
        public async Task<IActionResult> GetMyShowings()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

                var showings = await _context.Showings
                    .Include(s => s.Property)
                    .Include(s => s.Agent)
                    .Where(s => s.ClientId == userId)
                    .OrderByDescending(s => s.ScheduledDate)
                    .Select(s => new
                    {
                        s.Id,
                        s.ScheduledDate,
                        s.Status,
                        s.Notes,
                        PropertyTitle = s.Property!.Title,
                        PropertyAddress = s.Property.Address,
                        AgentName = $"{s.Agent!.FirstName} {s.Agent.LastName}",
                        AgentPhone = s.Agent.PhoneNumber
                    })
                    .ToListAsync();

                return Ok(showings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении показов");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }
    }
}