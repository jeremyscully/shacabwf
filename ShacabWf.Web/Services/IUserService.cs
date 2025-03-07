using ShacabWf.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShacabWf.Web.Services
{
    /// <summary>
    /// Service for managing users
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Gets all users
        /// </summary>
        /// <returns>List of all users</returns>
        Task<IEnumerable<User>> GetAllUsersAsync();
        
        /// <summary>
        /// Gets a user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User if found, null otherwise</returns>
        Task<User?> GetUserByIdAsync(int id);
        
        /// <summary>
        /// Gets a user by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>User if found, null otherwise</returns>
        Task<User?> GetUserByUsernameAsync(string username);
        
        /// <summary>
        /// Gets users by role
        /// </summary>
        /// <param name="role">Role name</param>
        /// <returns>List of users with the specified role</returns>
        Task<IEnumerable<User>> GetUsersByRoleAsync(string role);
        
        /// <summary>
        /// Gets the supervisor of a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Supervisor if found, null otherwise</returns>
        Task<User?> GetSupervisorAsync(int userId);
        
        /// <summary>
        /// Gets the subordinates of a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of subordinates</returns>
        Task<IEnumerable<User>> GetSubordinatesAsync(int userId);
    }
} 