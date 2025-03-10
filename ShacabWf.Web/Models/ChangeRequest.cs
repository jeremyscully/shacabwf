using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShacabWf.Web.Models
{
    /// <summary>
    /// Represents a change request in the system
    /// </summary>
    public class ChangeRequest
    {
        [Key]
        public int Id { get; set; }

        // Auto-generated change request number (CR-YYYY-NNNNN)
        [Required]
        [StringLength(20)]
        public string ChangeRequestNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Justification { get; set; }

        [StringLength(500)]
        public string? RiskAssessment { get; set; }

        [StringLength(500)]
        public string? BackoutPlan { get; set; }

        // Current status of the change request
        [Required]
        public ChangeRequestStatus Status { get; set; } = ChangeRequestStatus.Draft;

        // Priority of the change request
        [Required]
        public ChangeRequestPriority Priority { get; set; } = ChangeRequestPriority.Medium;

        // Type of change
        [Required]
        public ChangeRequestType Type { get; set; } = ChangeRequestType.Normal;

        // Impact of the change
        [Required]
        public ChangeRequestImpact Impact { get; set; } = ChangeRequestImpact.Medium;

        // Risk level of the change
        [Required]
        public RiskLevel Risk { get; set; } = RiskLevel.Medium;

        // Creation date
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Last updated date
        public DateTime? UpdatedAt { get; set; }

        // Scheduled start date for implementation
        public DateTime? ScheduledStartDate { get; set; }

        // Scheduled end date for implementation
        public DateTime? ScheduledEndDate { get; set; }

        // Actual implementation date
        public DateTime? ImplementedAt { get; set; }

        // Foreign key for creator
        [Required]
        public int CreatedById { get; set; }

        // Navigation property for creator
        [ForeignKey("CreatedById")]
        public virtual User? CreatedBy { get; set; }

        // Navigation property for approvals
        public virtual ICollection<ChangeRequestApproval>? Approvals { get; set; }

        // Navigation property for assignments
        public virtual ICollection<ChangeRequestAssignment>? Assignments { get; set; }

        // Navigation property for comments
        public virtual ICollection<ChangeRequestComment>? Comments { get; set; }

        // Navigation property for history
        public virtual ICollection<ChangeRequestHistory>? History { get; set; }
    }

    /// <summary>
    /// Represents the status of a change request
    /// </summary>
    public enum ChangeRequestStatus
    {
        Draft,
        SubmittedForSupervisorApproval,
        SupervisorApproved,
        SupervisorRejected,
        SubmittedForCABApproval,
        CABApproved,
        CABRejected,
        Scheduled,
        Rescheduled,
        InProgress,
        Completed,
        Failed,
        Cancelled
    }

    /// <summary>
    /// Represents the priority of a change request
    /// </summary>
    public enum ChangeRequestPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    /// <summary>
    /// Represents the type of a change request
    /// </summary>
    public enum ChangeRequestType
    {
        Normal,
        Standard,
        Emergency
    }

    /// <summary>
    /// Represents the impact of a change request
    /// </summary>
    public enum ChangeRequestImpact
    {
        Low,
        Medium,
        High
    }

    /// <summary>
    /// Represents the risk level of a change request
    /// </summary>
    public enum RiskLevel
    {
        Low,
        Medium,
        High,
        Critical
    }
} 