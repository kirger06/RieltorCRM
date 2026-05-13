using RieltorCRM.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RieltorCRM.Models
{
    public enum DocumentType
    {
        Contract,
        Act,
        Invoice,
        Certificate,
        Passport,
        PowerOfAttorney,
        Other
    }

    public class Document
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        public DocumentType Type { get; set; }

        [MaxLength(200)]
        public string? OriginalFileName { get; set; }

        public long FileSize { get; set; }

        [MaxLength(100)]
        public string? ContentType { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public int? DealId { get; set; }
        public int? PropertyId { get; set; }
        public int? UploadedById { get; set; }

        [ForeignKey("DealId")]
        public virtual Deal? Deal { get; set; }

        [ForeignKey("PropertyId")]
        public virtual Property? Property { get; set; }

        [ForeignKey("UploadedById")]
        public virtual User? UploadedBy { get; set; }
    }
}