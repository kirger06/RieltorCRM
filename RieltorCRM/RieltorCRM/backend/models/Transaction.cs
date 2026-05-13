using RieltorCRM.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RieltorCRM.Models
{
    public enum TransactionType
    {
        Income,
        Expense,
        Commission,
        Tax,
        Refund,
        Salary,
        Other
    }

    public enum TransactionStatus
    {
        Planned,
        Pending,
        Completed,
        Cancelled
    }

    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string TransactionNumber { get; set; } = string.Empty;

        [Required]
        public TransactionType Type { get; set; }

        [Required]
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(100)]
        public string? PaymentMethod { get; set; }

        public DateTime? PaymentDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? DealId { get; set; }
        public int? CreatedById { get; set; }
        public int? ConfirmedById { get; set; }

        [ForeignKey("DealId")]
        public virtual Deal? Deal { get; set; }

        [ForeignKey("CreatedById")]
        public virtual User? CreatedBy { get; set; }

        [ForeignKey("ConfirmedById")]
        public virtual User? ConfirmedBy { get; set; }

        public virtual ICollection<Invoice>? Invoices { get; set; }
    }
}