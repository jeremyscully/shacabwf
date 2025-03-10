using System.ComponentModel.DataAnnotations;

namespace ShacabWf.Web.Models
{
    public class ProfileViewModel
    {
        [Display(Name = "Theme")]
        public string Theme { get; set; } = "Default";
        
        // Return URL for redirecting back after saving
        public string ReturnUrl { get; set; }
        
        // Password update fields
        [Display(Name = "Current Password")]
        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }
        
        [Display(Name = "New Password")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        public string? NewPassword { get; set; }
        
        [Display(Name = "Confirm New Password")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }
    }
}
