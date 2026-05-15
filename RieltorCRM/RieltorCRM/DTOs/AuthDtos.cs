using System.ComponentModel.DataAnnotations;

namespace RieltorCRM.DTOs
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? MiddleName { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public Models.UserRole Role { get; set; }
    }

    public class CreateUserDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? MiddleName { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public Models.UserRole Role { get; set; }
    }

    public class CreatePropertyDto
    {
        [Required]
        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        public Models.PropertyType Type { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        public decimal? Area { get; set; }
        public int? Rooms { get; set; }
        public int? Floor { get; set; }
        public int? TotalFloors { get; set; }

        [Required]
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? City { get; set; }

        [MaxLength(200)]
        public string? District { get; set; }

        [MaxLength(20)]
        public string? PostalCode { get; set; }

        public string? Features { get; set; }
        public string? ImageUrls { get; set; }
    }

    public class CreateDealDto
    {
        [Required]
        public int PropertyId { get; set; }

        [Required]
        public int ClientId { get; set; }

        [Required]
        public int SellerId { get; set; }

        public Models.DealType Type { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Range(0, 100, ErrorMessage = "Commission percent must be between 0 and 100")]
        public decimal? CommissionPercent { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
    }

    public class CreateShowingDto
    {
        [Required]
        public int PropertyId { get; set; }

        [Required]
        public int ClientId { get; set; }

        [Required]
        public DateTime ScheduledDate { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class UpdateDealStatusDto
    {
        [Required]
        public Models.DealStatus Status { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }
    }

    public class AssignAgentDto
    {
        [Required]
        public int AgentId { get; set; }
    }

    public class CreateTransactionDto
    {
        public Models.TransactionType Type { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(200)]
        public string? PaymentMethod { get; set; }

        public int? DealId { get; set; }
    }
}
