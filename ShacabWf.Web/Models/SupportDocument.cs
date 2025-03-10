using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShacabWf.Web.Models
{
    /// <summary>
    /// Represents a support document in the system (Word doc or PDF)
    /// </summary>
    public class SupportDocument
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public string ContentType { get; set; } = string.Empty;

        [Required]
        public long FileSize { get; set; }

        // Creation date
        [Required]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Foreign key for uploader
        [Required]
        public int UploadedById { get; set; }

        // Navigation property for uploader
        [ForeignKey("UploadedById")]
        public virtual User? UploadedBy { get; set; }
    }
} 