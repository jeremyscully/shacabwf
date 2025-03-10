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
        public async Task<IActionResult> Index(string returnUrl = null)
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
                    Theme = user.Theme ?? "Default",
                    ReturnUrl = returnUrl ?? Url.Action("Simple", "Home") // Default to home page if no return URL
                };

                // Store return URL in ViewBag as well
                ViewBag.ReturnUrl = model.ReturnUrl;

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
        public async Task<IActionResult> UpdateTheme(string theme, string returnUrl = null)
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
                if (string.IsNullOrEmpty(theme))
                {
                    theme = "Default";
                }

                _logger.LogInformation("Attempting to update theme from {OldTheme} to {NewTheme}", user.Theme, theme);

                // Update theme
                user.Theme = theme;
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {Username} updated theme to {Theme}", user.Username, theme);

                // Redirect back to the return URL if provided, otherwise to profile
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user theme");
                return RedirectToAction(nameof(Index));
            }
        }
        
        // POST: Profile/UpdatePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePassword(ProfileViewModel model)
        {
            // If password fields are not provided, just return to the profile page
            if (string.IsNullOrEmpty(model.CurrentPassword) || 
                string.IsNullOrEmpty(model.NewPassword) || 
                string.IsNullOrEmpty(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "All password fields are required.");
                return View("Index", model);
            }
            
            // Validate that new password and confirm password match
            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "The new password and confirmation password do not match.");
                return View("Index", model);
            }
            
            try
            {
                int userId = GetCurrentUserId();
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    return NotFound();
                }

                // Verify current password
                if (user.Password != model.CurrentPassword)
                {
                    ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                    return View("Index", model);
                }

                _logger.LogInformation("Attempting to update password for user {Username}", user.Username);

                // Update password
                user.Password = model.NewPassword;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Password updated successfully for user {Username}", user.Username);

                // Add success message
                TempData["SuccessMessage"] = "Your password has been updated successfully.";

                // Redirect back to the return URL if provided, otherwise to profile
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user password");
                ModelState.AddModelError("", "An error occurred while updating your password. Please try again.");
                return View("Index", model);
            }
        }
    }
} 