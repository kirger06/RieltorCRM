using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Transactions;

namespace RieltorCRM.Models
{
    public enum UserRole
    {
        Admin,
        Agent,
        Client,
        Seller,
        OfficeManager,
        Accountant
    }

    public class User : IdentityUser<int>
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? MiddleName { get; set; }

        public UserRole Role { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber2 { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public virtual ICollection<Deal>? DealsAsAgent { get; set; }
        public virtual ICollection<Deal>? DealsAsClient { get; set; }
        public virtual ICollection<Deal>? DealsAsSeller { get; set; }
        public virtual ICollection<Property>? Properties { get; set; }
        public virtual ICollection<Transaction>? Transactions { get; set; }
    }
}