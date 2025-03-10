using System.Collections.Generic;
using ShacabWf.Web.Models;

namespace ShacabWf.Web.ViewModels
{
    /// <summary>
    /// View model for displaying user details
    /// </summary>
    public class UserDetailsViewModel
    {
        /// <summary>
        /// The user
        /// </summary>
        public User User { get; set; } = null!;
        
        /// <summary>
        /// The user's supervisor (if any)
        /// </summary>
        public User? Supervisor { get; set; }
        
        /// <summary>
        /// The user's subordinates (if any)
        /// </summary>
        public IEnumerable<User> Subordinates { get; set; } = new List<User>();
    }
} 