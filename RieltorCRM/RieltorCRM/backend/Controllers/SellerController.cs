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
    [Authorize(Roles = "Seller")]
    public class SellerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SellerController> _logger;

        public SellerController(ApplicationDbContext context, ILogger<SellerController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("my-properties")]
        public async Task<IActionResult> GetMyProperties()
        {
            try
            {
                int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var userId);

                var properties = await _context.Properties
                    .Include(p => p.Agent)
                    .Where(p => p.SellerId == userId && p.IsActive)
                    .OrderByDescending(p => p.CreatedAt)
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
                        p.UpdatedAt,
                        AgentName = p.Agent != null ? $"{p.Agent.FirstName} {p.Agent.LastName}" : "Не назначен",
                        AgentPhone = p.Agent != null ? p.Agent.PhoneNumber : null,
                        DealsCount = _context.Deals.Count(d => d.PropertyId == p.Id)
                    })
                    .ToListAsync();

                return Ok(properties);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении объектов продавца");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        [HttpPost("properties")]
        public async Task<IActionResult> AddProperty([FromBody] CreatePropertyDto model)
        {
            try
            {
                int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var userId);

                var property = new Property
                {
                    Title = model.Title,
                    Description = model.Description,
                    Type = model.Type,
                    Price = model.Price,
                    Area = model.Area,
                    Rooms = model.Rooms,
                    Floor = model.Floor,
                    TotalFloors = model.TotalFloors,
                    Address = model.Address,
                    City = model.City,
                    District = model.District,
                    PostalCode = model.PostalCode,
                    Features = model.Features,
                    ImageUrls = model.ImageUrls,
                    SellerId = userId,
                    Status = PropertyStatus.Available,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Properties.Add(property);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Объект добавлен", propertyId = property.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении объекта");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        [HttpPut("properties/{propertyId}")]
        public async Task<IActionResult> UpdateProperty(int propertyId, [FromBody] CreatePropertyDto model)
        {
            try
            {
                int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var userId);

                var property = await _context.Properties
                    .FirstOrDefaultAsync(p => p.Id == propertyId && p.SellerId == userId);

                if (property == null)
                    return NotFound(new { message = "Объект не найден" });

                property.Title = model.Title;
                property.Description = model.Description;
                property.Type = model.Type;
                property.Price = model.Price;
                property.Area = model.Area;
                property.Rooms = model.Rooms;
                property.Floor = model.Floor;
                property.TotalFloors = model.TotalFloors;
                property.Address = model.Address;
                property.City = model.City;
                property.District = model.District;
                property.PostalCode = model.PostalCode;
                property.Features = model.Features;
                property.ImageUrls = model.ImageUrls;
                property.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Объект обновлен" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении объекта");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        [HttpDelete("properties/{propertyId}")]
        public async Task<IActionResult> DeleteProperty(int propertyId)
        {
            try
            {
                int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var userId);

                var property = await _context.Properties
                    .FirstOrDefaultAsync(p => p.Id == propertyId && p.SellerId == userId);

                if (property == null)
                    return NotFound(new { message = "Объект не найден" });

                property.IsActive = false;
                property.Status = PropertyStatus.Inactive;
                property.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Объект удален" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении объекта");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        [HttpGet("my-deals")]
        public async Task<IActionResult> GetMyDeals()
        {
            try
            {
                int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var userId);

                var deals = await _context.Deals
                    .Include(d => d.Property)
                    .Include(d => d.Agent)
                    .Include(d => d.Client)
                    .Where(d => d.SellerId == userId)
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
                        PropertyTitle = d.Property != null ? d.Property.Title : "",
                        AgentName = d.Agent != null ? $"{d.Agent.FirstName} {d.Agent.LastName}".Trim() : "",
                        ClientName = d.Client != null ? $"{d.Client.FirstName} {d.Client.LastName}".Trim() : ""
                    })
                    .ToListAsync();

                return Ok(deals);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении сделок продавца");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }
    }
}