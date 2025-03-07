using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShacabWf.Web.Models
{
    /// <summary>
    /// Represents a history entry for a change request
    /// </summary>
    public class ChangeRequestHistory
    {
        [Key]
        public int Id { get; set; }

        // Foreign key for change request
        [Required]
        public int ChangeRequestId { get; set; }

        // Navigation property for change request
        [ForeignKey("ChangeRequestId")]
        public virtual ChangeRequest? ChangeRequest { get; set; }

        // Foreign key for user who made the change
        [Required]
        public int UserId { get; set; }

        // Navigation property for user who made the change
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        // Type of action performed
        [Required]
        [StringLength(50)]
        public string ActionType { get; set; } = string.Empty;

        // Description of the change
        [Required]
        public string Description { get; set; } = string.Empty;

        // Previous state (JSON serialized)
        public string? PreviousState { get; set; }

        // New state (JSON serialized)
        public string? NewState { get; set; }

        // Date when the history entry was created
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 