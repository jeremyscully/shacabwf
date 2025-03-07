using ShacabWf.Web.Models;

namespace ShacabWf.Web.Services
{
    /// <summary>
    /// Interface for authentication service
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Authenticates a user with the given username and password
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <returns>User if authentication is successful, null otherwise</returns>
        Task<User?> AuthenticateAsync(string username, string password);

        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <param name="user">User to register</param>
        /// <returns>Registered user</returns>
        Task<User> RegisterAsync(User user);

        /// <summary>
        /// Gets a user by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>User if found, null otherwise</returns>
        Task<User?> GetUserByUsernameAsync(string username);

        /// <summary>
        /// Gets a user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User if found, null otherwise</returns>
        Task<User?> GetUserByIdAsync(int id);

        /// <summary>
        /// Updates an existing user
        /// </summary>
        /// <param name="user">User to update</param>
        /// <returns>Updated user</returns>
        Task<User> UpdateUserAsync(User user);
    }
} 