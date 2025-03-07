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
    }
} 