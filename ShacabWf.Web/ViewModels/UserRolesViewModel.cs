using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ShacabWf.Web.Models;

namespace ShacabWf.Web.ViewModels
{
    /// <summary>
    /// View model for managing user roles and details
    /// </summary>
    public class UserRolesViewModel
    {
        /// <summary>
        /// User ID
        /// </summary>
        public int UserId { get; set; }
        
        /// <summary>
        /// Username
        /// </summary>
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// User's first name
        /// </summary>
        [Display(Name = "First Name")]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        /// <summary>
        /// User's last name
        /// </summary>
        [Display(Name = "Last Name")]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        /// <summary>
        /// User's full name
        /// </summary>
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;
        
        /// <summary>
        /// User's email
        /// </summary>
        [Display(Name = "Email")]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// User's department
        /// </summary>
        [Display(Name = "Department")]
        [StringLength(200)]
        public string Department { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether the user is a CAB member
        /// </summary>
        [Display(Name = "CAB Member")]
        public bool IsCABMember { get; set; }
        
        /// <summary>
        /// Whether the user is support personnel
        /// </summary>
        [Display(Name = "Support Personnel")]
        public bool IsSupportPersonnel { get; set; }
        
        /// <summary>
        /// ID of the user's supervisor
        /// </summary>
        [Display(Name = "Supervisor")]
        public int? SupervisorId { get; set; }
        
        /// <summary>
        /// List of all available supervisors
        /// </summary>
        public IEnumerable<User> AvailableSupervisors { get; set; } = new List<User>();
        
        /// <summary>
        /// All available roles in the system
        /// </summary>
        public IEnumerable<string> AllRoles { get; set; } = new List<string>();
        
        /// <summary>
        /// Roles selected for the user
        /// </summary>
        [Display(Name = "Roles")]
        public List<string> SelectedRoles { get; set; } = new List<string>();
    }
} 