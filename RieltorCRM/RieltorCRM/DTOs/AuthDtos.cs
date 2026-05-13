namespace RieltorCRM.DTOs
{
    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string? PhoneNumber { get; set; }
        public Models.UserRole Role { get; set; }
    }

    public class CreateUserDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string? PhoneNumber { get; set; }
        public Models.UserRole Role { get; set; }
    }

    public class CreatePropertyDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Models.PropertyType Type { get; set; }
        public decimal Price { get; set; }
        public decimal? Area { get; set; }
        public int? Rooms { get; set; }
        public int? Floor { get; set; }
        public int? TotalFloors { get; set; }
        public string Address { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? District { get; set; }
        public string? PostalCode { get; set; }
        public string? Features { get; set; }
        public string? ImageUrls { get; set; }
    }

    public class CreateDealDto
    {
        public int PropertyId { get; set; }
        public int ClientId { get; set; }
        public int SellerId { get; set; }
        public Models.DealType Type { get; set; }
        public decimal Amount { get; set; }
        public decimal? CommissionPercent { get; set; }
        public string? Description { get; set; }
    }

    public class CreateShowingDto
    {
        public int PropertyId { get; set; }
        public int ClientId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateDealStatusDto
    {
        public Models.DealStatus Status { get; set; }
        public string? Comment { get; set; }
    }

    public class AssignAgentDto
    {
        public int AgentId { get; set; }
    }

    public class CreateTransactionDto
    {
        public Models.TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public string? PaymentMethod { get; set; }
        public int? DealId { get; set; }
    }
}