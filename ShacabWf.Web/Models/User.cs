using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShacabWf.Web.Models
{
    /// <summary>
    /// Represents a user in the system
    /// </summary>
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        // Password field - stored in clear text for now
        [Required]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;

        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(200)]
        public string Department { get; set; } = string.Empty;

        // User's preferred theme
        [StringLength(50)]
        public string Theme { get; set; } = "Default";

        // Navigation property for users who report to this user (supervisor relationship)
        public virtual ICollection<User>? DirectReports { get; set; }

        // Foreign key for supervisor
        public int? SupervisorId { get; set; }

        // Navigation property for supervisor
        [ForeignKey("SupervisorId")]
        public virtual User? Supervisor { get; set; }

        // Flag to indicate if user is a CAB member
        public bool IsCABMember { get; set; } = false;

        // Flag to indicate if user is support personnel
        public bool IsSupportPersonnel { get; set; } = false;

        // Navigation property for change requests created by this user
        public virtual ICollection<ChangeRequest>? CreatedChangeRequests { get; set; }

        // Navigation property for change requests assigned to this user
        public virtual ICollection<ChangeRequestAssignment>? Assignments { get; set; }

        // Full name property for display purposes
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        // Roles for the user (comma-separated string)
        [StringLength(500)]
        public string Roles { get; set; } = string.Empty;

        // Helper method to check if user has a specific role
        public bool HasRole(string role)
        {
            if (string.IsNullOrEmpty(Roles))
                return false;

            var rolesList = Roles.Split(',').Select(r => r.Trim()).ToList();
            return rolesList.Contains(role);
        }
    }
} 