using RieltorCRM.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RieltorCRM.Models
{
    public enum ShowingStatus
    {
        Scheduled,
        Confirmed,
        InProgress,
        Completed,
        Cancelled,
        NoShow
    }

    public class Showing
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime ScheduledDate { get; set; }

        public DateTime? ActualDate { get; set; }

        [Required]
        public ShowingStatus Status { get; set; } = ShowingStatus.Scheduled;

        [MaxLength(500)]
        public string? Notes { get; set; }

        [MaxLength(200)]
        public string? ClientFeedback { get; set; }

        public int? Rating { get; set; }

        [Required]
        public int PropertyId { get; set; }

        [Required]
        public int AgentId { get; set; }

        [Required]
        public int ClientId { get; set; }

        [ForeignKey("PropertyId")]
        public virtual Property? Property { get; set; }

        [ForeignKey("AgentId")]
        public virtual User? Agent { get; set; }

        [ForeignKey("ClientId")]
        public virtual User? Client { get; set; }
    }
}