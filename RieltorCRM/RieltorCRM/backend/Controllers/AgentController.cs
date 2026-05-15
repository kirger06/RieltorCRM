using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RieltorCRM.Data;
using RieltorCRM.DTOs;
using RieltorCRM.Models;
using System.Security.Claims;
using System.Text.Json;

namespace RieltorCRM.Controllers
{
    [ApiController]
    [Route("api/agent")]
    [Authorize(Roles = "Agent")]
    public class AgentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AgentController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet("properties")]
        public async Task<IActionResult> GetMyProperties(
            [FromQuery] PropertyStatus? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var agentId = GetCurrentUserId();
            if (agentId == 0) return Unauthorized();

            var query = _context.Properties
                .Where(p => p.AgentId == agentId);

            if (status.HasValue)
                query = query.Where(p => p.Status == status.Value);

            var total = await query.CountAsync();
            var raw = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = raw.Select(p => new
            {
                p.Id,
                p.Title,
                Type = p.Type.ToString(),
                Status = p.Status.ToString(),
                p.Price,
                p.Area,
                p.Rooms,
                p.Address,
                p.City,
                p.ImageUrls,
                p.RejectionReason,
                p.CreatedAt,
                p.UpdatedAt
            });

            return Ok(new { total, page, pageSize, items });
        }

        [HttpGet("properties/{id}")]
        public async Task<IActionResult> GetProperty(int id)
        {
            var agentId = GetCurrentUserId();
            if (agentId == 0) return Unauthorized();

            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == id && p.AgentId == agentId);

            if (property == null)
                return NotFound();

            return Ok(property);
        }

        [HttpPost("properties")]
        public async Task<IActionResult> CreateProperty([FromBody] CreatePropertyDto dto)
        {
            var agentId = GetCurrentUserId();
            if (agentId == 0) return Unauthorized();

            var property = new Property
            {
                Title = dto.Title,
                Description = dto.Description,
                Type = dto.Type,
                Status = PropertyStatus.PendingApproval,
                Price = dto.Price,
                Area = dto.Area,
                Rooms = dto.Rooms,
                Floor = dto.Floor,
                TotalFloors = dto.TotalFloors,
                Address = dto.Address,
                City = dto.City,
                District = dto.District,
                PostalCode = dto.PostalCode,
                Features = dto.Features,
                AgentId = agentId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Properties.Add(property);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProperty), new { id = property.Id }, new { property.Id, property.Title, property.Status });
        }

        [HttpPut("properties/{id}")]
        public async Task<IActionResult> UpdateProperty(int id, [FromBody] CreatePropertyDto dto)
        {
            var agentId = GetCurrentUserId();
            if (agentId == 0) return Unauthorized();

            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == id && p.AgentId == agentId);

            if (property == null)
                return NotFound();

            property.Title = dto.Title;
            property.Description = dto.Description;
            property.Type = dto.Type;
            property.Price = dto.Price;
            property.Area = dto.Area;
            property.Rooms = dto.Rooms;
            property.Floor = dto.Floor;
            property.TotalFloors = dto.TotalFloors;
            property.Address = dto.Address;
            property.City = dto.City;
            property.District = dto.District;
            property.PostalCode = dto.PostalCode;
            property.Features = dto.Features;
            property.Status = PropertyStatus.PendingApproval;
            property.RejectionReason = null;
            property.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { property.Id, property.Status, message = "Объект отправлен на повторную модерацию" });
        }

        [HttpPut("properties/{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            var agentId = GetCurrentUserId();
            if (agentId == 0) return Unauthorized();

            var forbidden = new[] { PropertyStatus.PendingApproval, PropertyStatus.ApprovalRejected };
            if (forbidden.Contains(dto.Status))
                return BadRequest("Нельзя вручную установить статус модерации");

            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == id && p.AgentId == agentId);

            if (property == null)
                return NotFound();

            if (property.Status == PropertyStatus.PendingApproval)
                return BadRequest("Объект на модерации, изменение статуса недоступно");

            property.Status = dto.Status;
            property.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { property.Id, property.Status });
        }

        [HttpPost("properties/{id}/images")]
        public async Task<IActionResult> UploadImages(int id, [FromForm] IFormFileCollection files)
        {
            var agentId = GetCurrentUserId();
            if (agentId == 0) return Unauthorized();

            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == id && p.AgentId == agentId);

            if (property == null)
                return NotFound();

            if (files == null || files.Count == 0)
                return BadRequest("Файлы не выбраны");

            var webRoot = _env.WebRootPath;
            if (string.IsNullOrEmpty(webRoot))
                webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            var uploadsDir = Path.Combine(webRoot, "uploads", "properties", id.ToString());
            Directory.CreateDirectory(uploadsDir);

            var existingUrls = string.IsNullOrEmpty(property.ImageUrls)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(property.ImageUrls) ?? new List<string>();

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            foreach (var file in files)
            {
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(ext))
                    continue;

                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsDir, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                existingUrls.Add($"/uploads/properties/{id}/{fileName}");
            }

            property.ImageUrls = JsonSerializer.Serialize(existingUrls);
            property.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { urls = existingUrls });
        }

        [HttpDelete("properties/{propertyId}/images")]
        public async Task<IActionResult> DeleteImage(int propertyId, [FromBody] DeleteImageDto dto)
        {
            var agentId = GetCurrentUserId();
            if (agentId == 0) return Unauthorized();

            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == propertyId && p.AgentId == agentId);

            if (property == null)
                return NotFound();

            var urls = string.IsNullOrEmpty(property.ImageUrls)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(property.ImageUrls) ?? new List<string>();

            urls.Remove(dto.Url);
            property.ImageUrls = JsonSerializer.Serialize(urls);
            await _context.SaveChangesAsync();

            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var filePath = Path.Combine(webRoot, dto.Url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            return Ok(new { urls });
        }

        [HttpDelete("properties/{id}")]
        public async Task<IActionResult> DeleteProperty(int id)
        {
            var agentId = GetCurrentUserId();
            if (agentId == 0) return Unauthorized();

            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == id && p.AgentId == agentId);

            if (property == null)
                return NotFound();

            var hasActiveBooking = await _context.Bookings
                .AnyAsync(b => b.PropertyId == id && b.Status == BookingStatus.Active);

            if (hasActiveBooking)
                return BadRequest("Нельзя удалить объект с активным бронированием");

            property.IsActive = false;
            property.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Объект деактивирован" });
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var agentId = GetCurrentUserId();
            if (agentId == 0) return Unauthorized();

            var stats = await _context.Properties
                .Where(p => p.AgentId == agentId)
                .GroupBy(p => p.Status)
                .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                .ToListAsync();

            var rawBookings = await _context.Bookings
                .Include(b => b.Property)
                .Include(b => b.Client)
                .Where(b => b.Property!.AgentId == agentId && b.Status == BookingStatus.Active)
                .ToListAsync();

            var activeBookings = rawBookings.Select(b => new
            {
                b.Id,
                Type = b.Type.ToString(),
                Status = b.Status.ToString(),
                b.CreatedAt,
                PropertyTitle = b.Property!.Title,
                PropertyId = b.Property.Id,
                ClientName = $"{b.Client!.FirstName} {b.Client.LastName}",
                ClientEmail = b.Client.Email,
                ClientPhone = b.Client.PhoneNumber
            });

            return Ok(new { stats, activeBookings });
        }

        private int GetCurrentUserId()
        {
            int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId);
            return userId;
        }
    }

    public class UpdateStatusDto
    {
        public PropertyStatus Status { get; set; }
    }

    public class DeleteImageDto
    {
        public string Url { get; set; } = string.Empty;
    }
}
