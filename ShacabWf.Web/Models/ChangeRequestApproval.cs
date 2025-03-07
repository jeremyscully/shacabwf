using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShacabWf.Web.Models
{
    /// <summary>
    /// Represents an approval for a change request
    /// </summary>
    public class ChangeRequestApproval
    {
        [Key]
        public int Id { get; set; }

        // Foreign key for change request
        [Required]
        public int ChangeRequestId { get; set; }

        // Navigation property for change request
        [ForeignKey("ChangeRequestId")]
        public virtual ChangeRequest? ChangeRequest { get; set; }

        // Foreign key for approver
        [Required]
        public int ApproverId { get; set; }

        // Navigation property for approver
        [ForeignKey("ApproverId")]
        public virtual User? Approver { get; set; }

        // Type of approval
        [Required]
        public ApprovalType Type { get; set; }

        // Status of the approval
        [Required]
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

        // Comments from the approver
        [StringLength(500)]
        public string? Comments { get; set; }

        // Date when the approval was requested
        [Required]
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        // Date when the approval was acted upon
        public DateTime? ActionedAt { get; set; }
    }

    /// <summary>
    /// Represents the type of approval
    /// </summary>
    public enum ApprovalType
    {
        Supervisor,
        CAB
    }

    /// <summary>
    /// Represents the status of an approval
    /// </summary>
    public enum ApprovalStatus
    {
        Pending,
        Approved,
        Rejected
    }
} 