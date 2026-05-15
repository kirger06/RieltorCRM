using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RieltorCRM.Data;
using RieltorCRM.Models;
using System.Security.Claims;

namespace RieltorCRM.Controllers
{
    [ApiController]
    [Route("api/client")]
    [Authorize(Roles = "Client")]
    public class ClientController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ClientController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("bookings")]
        public async Task<IActionResult> GetMyBookings()
        {
            var clientId = GetCurrentUserId();
            if (clientId == 0) return Unauthorized();

            var raw = await _context.Bookings
                .Include(b => b.Property)
                    .ThenInclude(p => p!.Agent)
                .Where(b => b.ClientId == clientId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            var bookings = raw.Select(b => new
            {
                b.Id,
                Type = b.Type.ToString(),
                Status = b.Status.ToString(),
                b.CreatedAt,
                Property = b.Property == null ? null : (object)new
                {
                    b.Property.Id,
                    b.Property.Title,
                    b.Property.Address,
                    b.Property.City,
                    b.Property.Price,
                    b.Property.Area,
                    b.Property.Rooms,
                    b.Property.ImageUrls,
                    Status = b.Property.Status.ToString(),
                    AgentName = b.Property.Agent != null
                        ? $"{b.Property.Agent.FirstName} {b.Property.Agent.LastName}"
                        : null,
                    AgentPhone = b.Property.Agent != null ? b.Property.Agent.PhoneNumber : null
                }
            });

            return Ok(bookings);
        }

        [HttpPost("bookings")]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
        {
            var clientId = GetCurrentUserId();
            if (clientId == 0) return Unauthorized();

            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == dto.PropertyId && p.IsActive);

            if (property == null)
                return NotFound("Объект не найден");

            if (property.Status != PropertyStatus.Available)
                return BadRequest($"Объект недоступен для бронирования (статус: {property.Status})");

            var existingBooking = await _context.Bookings
                .AnyAsync(b => b.PropertyId == dto.PropertyId && b.Status == BookingStatus.Active);

            if (existingBooking)
                return BadRequest("Объект уже забронирован");

            var booking = new Booking
            {
                PropertyId = dto.PropertyId,
                ClientId = clientId,
                Type = dto.Type,
                Status = BookingStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            property.Status = dto.Type == BookingType.Purchase
                ? PropertyStatus.Sold
                : PropertyStatus.Reserved;
            property.UpdatedAt = DateTime.UtcNow;

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMyBookings), new { },
                new { booking.Id, booking.Type, booking.Status, PropertyStatus = property.Status });
        }

        [HttpDelete("bookings/{id}")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var clientId = GetCurrentUserId();
            if (clientId == 0) return Unauthorized();

            var booking = await _context.Bookings
                .Include(b => b.Property)
                .FirstOrDefaultAsync(b => b.Id == id && b.ClientId == clientId);

            if (booking == null)
                return NotFound();

            if (booking.Status != BookingStatus.Active)
                return BadRequest("Бронирование уже не активно");

            booking.Status = BookingStatus.Cancelled;

            if (booking.Property != null)
            {
                booking.Property.Status = PropertyStatus.Available;
                booking.Property.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Бронирование отменено" });
        }

        private int GetCurrentUserId()
        {
            int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId);
            return userId;
        }
    }

    public class CreateBookingDto
    {
        public int PropertyId { get; set; }
        public BookingType Type { get; set; }
    }
}
