using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShacabWf.Web.Models;
using ShacabWf.Web.Services;
using System.Security.Claims;

namespace ShacabWf.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAuthService authService, ILogger<AccountController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            ViewData["ReturnUrl"] = model.ReturnUrl;

            if (ModelState.IsValid)
            {
                try
                {
                    // Attempt to authenticate the user
                    var user = await _authService.AuthenticateAsync(model.Username, model.Password);

                    if (user != null)
                    {
                        _logger.LogInformation("User {Username} logged in at {Time}", user.Username, DateTime.UtcNow);

                        // Create claims for the authenticated user
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                            new Claim(ClaimTypes.Name, user.Username),
                            new Claim(ClaimTypes.Email, user.Email),
                            new Claim(ClaimTypes.GivenName, user.FirstName),
                            new Claim(ClaimTypes.Surname, user.LastName)
                        };

                        // Add role claims from the Roles property
                        if (!string.IsNullOrEmpty(user.Roles))
                        {
                            foreach (var role in user.Roles.Split(',').Select(r => r.Trim()))
                            {
                                claims.Add(new Claim(ClaimTypes.Role, role));
                            }
                        }
                        else
                        {
                            // For backward compatibility, add role claims based on properties
                            // This ensures existing users still have roles
                            claims.Add(new Claim(ClaimTypes.Role, "User"));

                            if (user.IsCABMember)
                            {
                                claims.Add(new Claim(ClaimTypes.Role, "CABMember"));
                            }

                            if (user.IsSupportPersonnel)
                            {
                                claims.Add(new Claim(ClaimTypes.Role, "Support"));
                            }

                            // Add Admin role for admin user
                            if (user.Username.ToLower() == "admin")
                            {
                                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                            }

                            // Update the user's Roles property for future logins
                            var roles = claims
                                .Where(c => c.Type == ClaimTypes.Role)
                                .Select(c => c.Value)
                                .ToList();

                            user.Roles = string.Join(",", roles);
                            await _authService.UpdateUserAsync(user);
                        }

                        // Create the identity and principal
                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var principal = new ClaimsPrincipal(identity);

                        // Set authentication properties
                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = model.RememberMe,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(model.RememberMe ? 30 : 1)
                        };

                        // Sign in the user
                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            principal,
                            authProperties);

                        // Redirect to the Simple page if no return URL is specified
                        if (string.IsNullOrEmpty(model.ReturnUrl))
                        {
                            return RedirectToAction("Simple", "Home");
                        }
                        
                        // Otherwise redirect to the return URL
                        return LocalRedirect(model.ReturnUrl);
                    }

                    ModelState.AddModelError(string.Empty, "Invalid username or password");
                    return View(model);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during login attempt for user {Username}", model.Username);
                    ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Create a new user from the view model
                    var user = new User
                    {
                        Username = model.Username,
                        Email = model.Email,
                        Password = model.Password,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Department = model.Department
                    };

                    // Register the user
                    await _authService.RegisterAsync(user);

                    _logger.LogInformation("User {Username} registered at {Time}", user.Username, DateTime.UtcNow);

                    // Redirect to login page
                    TempData["SuccessMessage"] = "Registration successful! You can now log in.";
                    return RedirectToAction(nameof(Login));
                }
                catch (InvalidOperationException ex)
                {
                    // Handle specific registration errors (e.g., username already taken)
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                catch (Exception ex)
                {
                    // Handle general errors
                    _logger.LogError(ex, "Error during registration for user {Username}", model.Username);
                    ModelState.AddModelError(string.Empty, "An error occurred during registration. Please try again.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Sign out the user
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("User logged out at {Time}", DateTime.UtcNow);

            // Redirect to Simple home page
            return RedirectToAction("Simple", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
} 