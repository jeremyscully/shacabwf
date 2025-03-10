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
        
        /// <summary>
        /// Updates a user's roles
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="roles">Comma-separated list of roles</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> UpdateUserRolesAsync(int userId, string roles);
        
        /// <summary>
        /// Updates a user's CAB member status
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="isCABMember">Whether the user is a CAB member</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> UpdateCABMemberStatusAsync(int userId, bool isCABMember);
        
        /// <summary>
        /// Updates a user's support personnel status
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="isSupportPersonnel">Whether the user is support personnel</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> UpdateSupportPersonnelStatusAsync(int userId, bool isSupportPersonnel);
        
        /// <summary>
        /// Saves changes to a user
        /// </summary>
        /// <param name="user">User to save</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> SaveUserAsync(User user);
        
        /// <summary>
        /// Gets all available roles in the system
        /// </summary>
        /// <returns>List of role names</returns>
        Task<IEnumerable<string>> GetAllRolesAsync();
        
        /// <summary>
        /// Updates a user's supervisor
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="supervisorId">Supervisor ID (null to remove supervisor)</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> UpdateSupervisorAsync(int userId, int? supervisorId);
        
        /// <summary>
        /// Gets all users who can be supervisors (excluding the specified user)
        /// </summary>
        /// <param name="excludeUserId">User ID to exclude from the results</param>
        /// <returns>List of potential supervisors</returns>
        Task<IEnumerable<User>> GetPotentialSupervisorsAsync(int excludeUserId);
        
        /// <summary>
        /// Updates a user's basic information
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="firstName">First name</param>
        /// <param name="lastName">Last name</param>
        /// <param name="email">Email</param>
        /// <param name="department">Department</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> UpdateUserInfoAsync(int userId, string firstName, string lastName, string email, string department);
    }
} 