using RieltorCRM.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;
using System.Transactions;

namespace RieltorCRM.Models
{
    public enum DealType
    {
        Sale,
        Rent,
        Purchase,
        Exchange
    }

    public enum DealStatus
    {
        Draft,
        InProgress,
        DocumentsPreparing,
        PendingPayment,
        Completed,
        Cancelled,
        Disputed
    }

    public class Deal
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string DealNumber { get; set; } = string.Empty;

        [Required]
        public DealType Type { get; set; }

        [Required]
        public DealStatus Status { get; set; } = DealStatus.Draft;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Commission { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CommissionPercent { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? ContractNumber { get; set; }

        public DateTime? ContractDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Внешние ключи - ТОЛЬКО int
        [Required]
        public int PropertyId { get; set; }

        [Required]
        public int AgentId { get; set; }

        [Required]
        public int ClientId { get; set; }

        [Required]
        public int SellerId { get; set; }

        public int? OfficeManagerId { get; set; }

        // Навигационные свойства - с атрибутами ForeignKey
        [ForeignKey("PropertyId")]
        public Property? Property { get; set; }

        [ForeignKey("AgentId")]
        public User? Agent { get; set; }

        [ForeignKey("ClientId")]
        public User? Client { get; set; }

        [ForeignKey("SellerId")]
        public User? Seller { get; set; }

        [ForeignKey("OfficeManagerId")]
        public User? OfficeManager { get; set; }
    }
}