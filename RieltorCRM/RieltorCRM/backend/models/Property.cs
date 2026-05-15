using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RieltorCRM.Models
{
    public enum PropertyType
    {
        Apartment,
        House,
        Land,
        Commercial,
        Garage,
        Townhouse
    }

    public enum PropertyStatus
    {
        PendingApproval,
        ApprovalRejected,
        Available,
        Reserved,
        Sold,
        Rented,
        UnderContract,
        Inactive
    }

    public class Property
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Required]
        public PropertyType Type { get; set; }

        [Required]
        public PropertyStatus Status { get; set; } = PropertyStatus.PendingApproval;

        [Required]
        public decimal Price { get; set; }

        public decimal? Area { get; set; }

        public int? Rooms { get; set; }
        public int? Floor { get; set; }
        public int? TotalFloors { get; set; }

        [Required]
        [MaxLength(300)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(100)]
        public string? District { get; set; }

        [MaxLength(20)]
        public string? PostalCode { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public string? Features { get; set; }

        public string? ImageUrls { get; set; }

        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        public int? SellerId { get; set; }
        public int? AgentId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        [ForeignKey("SellerId")]
        public User? Seller { get; set; }

        [ForeignKey("AgentId")]
        public User? Agent { get; set; }
    }
}