using RieltorCRM.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RieltorCRM.Models
{
    public class DealHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DealId { get; set; }

        public DealStatus OldStatus { get; set; }

        [Required]
        public DealStatus NewStatus { get; set; }

        public int ChangedById { get; set; }

        [MaxLength(500)]
        public string? Comment { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("DealId")]
        public virtual Deal? Deal { get; set; }

        [ForeignKey("ChangedById")]
        public virtual User? ChangedBy { get; set; }
    }
}