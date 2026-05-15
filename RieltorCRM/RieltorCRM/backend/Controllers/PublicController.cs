using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RieltorCRM.Data;
using RieltorCRM.Models;

namespace RieltorCRM.Controllers
{
    [ApiController]
    [Route("api/public")]
    public class PublicController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PublicController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("properties")]
        public async Task<IActionResult> GetProperties(
            [FromQuery] string? city,
            [FromQuery] PropertyType? type,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] int? minRooms,
            [FromQuery] int? maxRooms,
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12)
        {
            var query = _context.Properties
                .Include(p => p.Agent)
                .Where(p => p.Status == PropertyStatus.Available && p.IsActive);

            if (!string.IsNullOrWhiteSpace(city))
                query = query.Where(p => p.City == city);

            if (type.HasValue)
                query = query.Where(p => p.Type == type.Value);

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            if (minRooms.HasValue)
                query = query.Where(p => p.Rooms >= minRooms.Value);

            if (maxRooms.HasValue)
                query = query.Where(p => p.Rooms <= maxRooms.Value);

            // Pull to memory so C# OrdinalIgnoreCase handles Cyrillic correctly
            // (SQLite lower() doesn't work with Cyrillic)
            var allMatched = await query.ToListAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var words = search
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                // All words must match somewhere — each word is checked across all text fields
                allMatched = allMatched.Where(p =>
                    words.All(w =>
                        (p.Title ?? "").Contains(w, StringComparison.OrdinalIgnoreCase) ||
                        (p.Address ?? "").Contains(w, StringComparison.OrdinalIgnoreCase) ||
                        (p.City ?? "").Contains(w, StringComparison.OrdinalIgnoreCase) ||
                        (p.District ?? "").Contains(w, StringComparison.OrdinalIgnoreCase) ||
                        (p.Description ?? "").Contains(w, StringComparison.OrdinalIgnoreCase) ||
                        (p.Features ?? "").Contains(w, StringComparison.OrdinalIgnoreCase) ||
                        p.Price.ToString().Contains(w, StringComparison.OrdinalIgnoreCase)
                    )
                ).ToList();
            }

            var total = allMatched.Count;
            var raw = allMatched
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var items = raw.Select(p => new
            {
                p.Id,
                p.Title,
                Type = p.Type.ToString(),
                Status = p.Status.ToString(),
                p.Price,
                p.Area,
                p.Rooms,
                p.Floor,
                p.TotalFloors,
                p.Address,
                p.City,
                p.District,
                p.ImageUrls,
                AgentName = p.Agent != null ? $"{p.Agent.FirstName} {p.Agent.LastName}" : null,
                p.CreatedAt
            });

            return Ok(new { total, page, pageSize, items });
        }

        [HttpGet("properties/{id}")]
        public async Task<IActionResult> GetProperty(int id)
        {
            var raw = await _context.Properties
                .Include(p => p.Agent)
                .Where(p => p.Id == id && p.IsActive)
                .FirstOrDefaultAsync();

            if (raw == null)
                return NotFound();

            var property = new
            {
                raw.Id,
                raw.Title,
                raw.Description,
                Type = raw.Type.ToString(),
                Status = raw.Status.ToString(),
                raw.Price,
                raw.Area,
                raw.Rooms,
                raw.Floor,
                raw.TotalFloors,
                raw.Address,
                raw.City,
                raw.District,
                raw.PostalCode,
                raw.Latitude,
                raw.Longitude,
                raw.Features,
                raw.ImageUrls,
                raw.RejectionReason,
                AgentName = raw.Agent != null ? $"{raw.Agent.FirstName} {raw.Agent.LastName}" : null,
                AgentPhone = raw.Agent != null ? raw.Agent.PhoneNumber : null,
                raw.CreatedAt,
                raw.UpdatedAt
            };

            return Ok(property);
        }

        [HttpGet("cities")]
        public async Task<IActionResult> GetCities()
        {
            var cities = await _context.Properties
                .Where(p => p.Status == PropertyStatus.Available && p.City != null)
                .Select(p => p.City!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return Ok(cities);
        }
    }
}
