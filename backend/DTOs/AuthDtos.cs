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

        public bool RegisterAsManager { get; set; } = false;

        [MaxLength(200)]
        public string? CompanyName { get; set; }
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

        [Range(0.01, double.MaxValue)]
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
}
