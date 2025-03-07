using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShacabWf.Web.Data;
using ShacabWf.Web.Models;
using ShacabWf.Web.Services;
using System.Security.Claims;

namespace ShacabWf.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
            ApplicationDbContext context, 
            IAuthService authService,
            ILogger<ProfileController> logger)
        {
            _context = context;
            _authService = authService;
            _logger = logger;
        }

        // Helper method to get the current user ID
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("User ID not found in claims");
            }
            return userId;
        }

        // GET: Profile
        public async Task<IActionResult> Index()
        {
            try
            {
                int userId = GetCurrentUserId();
                var user = await _authService.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return NotFound();
                }

                // Create a view model for the profile
                var model = new ProfileViewModel
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Department = user.Department,
                    Theme = user.Theme
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile");
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: Profile/UpdateTheme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTheme(string theme)
        {
            try
            {
                int userId = GetCurrentUserId();
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    return NotFound();
                }

                // Validate theme
                if (theme != "Default" && theme != "Seattlehousing")
                {
                    theme = "Default";
                }

                // Update theme
                user.Theme = theme;
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {Username} updated theme to {Theme}", user.Username, theme);

                // Redirect back to profile
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user theme");
                return RedirectToAction(nameof(Index));
            }
        }
    }

    // View model for the profile page
    public class ProfileViewModel
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Theme { get; set; } = "Default";
    }
} 