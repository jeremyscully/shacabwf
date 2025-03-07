using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShacabWf.Web.Models
{
    /// <summary>
    /// Represents an assignment of a change request to a support personnel
    /// </summary>
    public class ChangeRequestAssignment
    {
        [Key]
        public int Id { get; set; }

        // Foreign key for change request
        [Required]
        public int ChangeRequestId { get; set; }

        // Navigation property for change request
        [ForeignKey("ChangeRequestId")]
        public virtual ChangeRequest? ChangeRequest { get; set; }

        // Foreign key for assignee
        [Required]
        public int AssigneeId { get; set; }

        // Navigation property for assignee
        [ForeignKey("AssigneeId")]
        public virtual User? Assignee { get; set; }

        // Role of the assignee in the change request
        [Required]
        [StringLength(100)]
        public string Role { get; set; } = string.Empty;

        // Notes about the assignment
        [StringLength(500)]
        public string? Notes { get; set; }

        // Date when the assignment was created
        [Required]
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        // Status of the assignment
        [Required]
        public AssignmentStatus Status { get; set; } = AssignmentStatus.Assigned;
    }

    /// <summary>
    /// Represents the status of an assignment
    /// </summary>
    public enum AssignmentStatus
    {
        Assigned,
        InProgress,
        Completed,
        Cancelled
    }
} 