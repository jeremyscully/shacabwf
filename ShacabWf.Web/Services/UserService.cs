using Microsoft.EntityFrameworkCore;
using ShacabWf.Web.Data;
using ShacabWf.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShacabWf.Web.Services
{
    /// <summary>
    /// Implementation of the IUserService interface
    /// </summary>
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        /// <inheritdoc />
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        /// <inheritdoc />
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string role)
        {
            var users = await _context.Users.ToListAsync();
            return users.Where(u => u.HasRole(role)).ToList();
        }

        /// <inheritdoc />
        public async Task<User?> GetSupervisorAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Supervisor)
                .FirstOrDefaultAsync(u => u.Id == userId);

            return user?.Supervisor;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<User>> GetSubordinatesAsync(int userId)
        {
            return await _context.Users
                .Where(u => u.SupervisorId == userId)
                .ToListAsync();
        }
        
        /// <inheritdoc />
        public async Task<bool> UpdateUserRolesAsync(int userId, string roles)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;
                
            user.Roles = roles;
            
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <inheritdoc />
        public async Task<bool> UpdateCABMemberStatusAsync(int userId, bool isCABMember)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;
                
            user.IsCABMember = isCABMember;
            
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <inheritdoc />
        public async Task<bool> UpdateSupportPersonnelStatusAsync(int userId, bool isSupportPersonnel)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;
                
            user.IsSupportPersonnel = isSupportPersonnel;
            
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <inheritdoc />
        public async Task<bool> SaveUserAsync(User user)
        {
            try
            {
                if (user.Id == 0)
                {
                    // New user
                    _context.Users.Add(user);
                }
                else
                {
                    // Existing user
                    _context.Entry(user).State = EntityState.Modified;
                }
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <inheritdoc />
        public async Task<IEnumerable<string>> GetAllRolesAsync()
        {
            // Return a list of predefined roles in the system
            // This could be stored in a database table in a more complex system
            return await Task.FromResult(new List<string>
            {
                "Admin",
                "Supervisor",
                "User",
                "Approver",
                "Implementer"
            });
        }

        /// <inheritdoc />
        public async Task<bool> UpdateSupervisorAsync(int userId, int? supervisorId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;
                
            // Prevent circular supervisor relationships
            if (supervisorId.HasValue)
            {
                // Check if the supervisor exists
                var supervisor = await _context.Users.FindAsync(supervisorId.Value);
                if (supervisor == null)
                    return false;
                    
                // Check if this would create a circular relationship
                if (supervisorId.Value == userId)
                    return false;
                    
                // Check if the supervisor has this user as their supervisor (direct or indirect)
                var currentSupervisorId = supervisor.SupervisorId;
                while (currentSupervisorId.HasValue)
                {
                    if (currentSupervisorId.Value == userId)
                        return false;
                        
                    var currentSupervisor = await _context.Users.FindAsync(currentSupervisorId.Value);
                    if (currentSupervisor == null)
                        break;
                        
                    currentSupervisorId = currentSupervisor.SupervisorId;
                }
            }
            
            user.SupervisorId = supervisorId;
            
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <inheritdoc />
        public async Task<IEnumerable<User>> GetPotentialSupervisorsAsync(int excludeUserId)
        {
            return await _context.Users
                .Where(u => u.Id != excludeUserId)
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync();
        }
        
        /// <inheritdoc />
        public async Task<bool> UpdateUserInfoAsync(int userId, string firstName, string lastName, string email, string department)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;
                
            user.FirstName = firstName;
            user.LastName = lastName;
            user.Email = email;
            user.Department = department;
            
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
} 