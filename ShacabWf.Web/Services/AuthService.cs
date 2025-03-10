using Microsoft.EntityFrameworkCore;
using ShacabWf.Web.Data;
using ShacabWf.Web.Models;

namespace ShacabWf.Web.Services
{
    /// <summary>
    /// Implementation of the authentication service
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Authenticates a user with the given username and password
        /// </summary>
        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            // Find the user by username
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            // Check if user exists and password matches
            if (user != null && user.Password == password)
            {
                // Ensure the roles string is synchronized with special status flags
                bool updated = false;
                var roles = string.IsNullOrEmpty(user.Roles) 
                    ? new List<string>() 
                    : user.Roles.Split(',').Select(r => r.Trim()).ToList();
                
                // Ensure User role is present
                if (!roles.Contains("User"))
                {
                    roles.Add("User");
                    updated = true;
                }
                
                // Sync CABMember role
                if (user.IsCABMember && !roles.Contains("CABMember"))
                {
                    roles.Add("CABMember");
                    updated = true;
                }
                else if (!user.IsCABMember && roles.Contains("CABMember"))
                {
                    roles.Remove("CABMember");
                    updated = true;
                }
                
                // Sync Support role
                if (user.IsSupportPersonnel && !roles.Contains("Support"))
                {
                    roles.Add("Support");
                    updated = true;
                }
                else if (!user.IsSupportPersonnel && roles.Contains("Support"))
                {
                    roles.Remove("Support");
                    updated = true;
                }
                
                // Add Admin role for admin user
                if (user.Username.ToLower() == "admin" && !roles.Contains("Admin"))
                {
                    roles.Add("Admin");
                    updated = true;
                }
                
                if (updated)
                {
                    user.Roles = string.Join(",", roles);
                    await _context.SaveChangesAsync();
                }
                
                return user;
            }

            return null;
        }

        /// <summary>
        /// Registers a new user
        /// </summary>
        public async Task<User> RegisterAsync(User user)
        {
            // Check if username already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == user.Username);

            if (existingUser != null)
            {
                throw new InvalidOperationException($"Username '{user.Username}' is already taken");
            }

            // Check if email already exists
            existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == user.Email);

            if (existingUser != null)
            {
                throw new InvalidOperationException($"Email '{user.Email}' is already registered");
            }

            // Set roles based on user properties
            var roles = new List<string>();
            
            // Add "User" role by default
            roles.Add("User");
            
            // Add additional roles based on properties
            if (user.IsCABMember)
            {
                roles.Add("CABMember");
            }
            
            if (user.IsSupportPersonnel)
            {
                roles.Add("Support");
            }
            
            // Add Admin role for admin user
            if (user.Username.ToLower() == "admin")
            {
                roles.Add("Admin");
            }
            
            // Set the roles string
            user.Roles = string.Join(",", roles);

            // Add the user to the database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        /// <summary>
        /// Gets a user by username
        /// </summary>
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        /// <summary>
        /// Gets a user by ID
        /// </summary>
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        /// <summary>
        /// Updates an existing user
        /// </summary>
        public async Task<User> UpdateUserAsync(User user)
        {
            // Check if user exists
            var existingUser = await _context.Users.FindAsync(user.Id);
            if (existingUser == null)
            {
                throw new InvalidOperationException($"User with ID {user.Id} not found");
            }

            // Update user properties
            _context.Entry(existingUser).CurrentValues.SetValues(user);
            await _context.SaveChangesAsync();

            return existingUser;
        }
    }
}
