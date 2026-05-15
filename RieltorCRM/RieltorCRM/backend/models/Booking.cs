using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RieltorCRM.Models
{
    public enum BookingType
    {
        Purchase,
        Reserve
    }

    public enum BookingStatus
    {
        Active,
        Cancelled,
        Completed
    }

    public class Booking
    {
        public int Id { get; set; }

        [Required]
        public int PropertyId { get; set; }

        [Required]
        public int ClientId { get; set; }

        public BookingType Type { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Active;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("PropertyId")]
        public Property? Property { get; set; }

        [ForeignKey("ClientId")]
        public User? Client { get; set; }
    }
}
