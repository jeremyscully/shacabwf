using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ShacabWf.Web.Models;
using ShacabWf.Web.Services;
using ShacabWf.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShacabWf.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserManagementController> _logger;

        public UserManagementController(IUserService userService, ILogger<UserManagementController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // GET: UserManagement
        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users for management");
                return View("Error", new ErrorViewModel { Message = "An error occurred while retrieving users." });
            }
        }

        // GET: UserManagement/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                var allRoles = await _userService.GetAllRolesAsync();
                var userRoles = string.IsNullOrEmpty(user.Roles) 
                    ? new List<string>() 
                    : user.Roles.Split(',').Select(r => r.Trim()).ToList();
                
                var potentialSupervisors = await _userService.GetPotentialSupervisorsAsync(id);

                var viewModel = new UserRolesViewModel
                {
                    UserId = user.Id,
                    Username = user.Username,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName,
                    Email = user.Email,
                    Department = user.Department,
                    IsCABMember = user.IsCABMember,
                    IsSupportPersonnel = user.IsSupportPersonnel,
                    SupervisorId = user.SupervisorId,
                    AvailableSupervisors = potentialSupervisors,
                    AllRoles = allRoles,
                    SelectedRoles = userRoles
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId} for editing", id);
                return View("Error", new ErrorViewModel { Message = "An error occurred while retrieving the user." });
            }
        }

        // POST: UserManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserRolesViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload all roles and supervisors for the view
                model.AllRoles = await _userService.GetAllRolesAsync();
                model.AvailableSupervisors = await _userService.GetPotentialSupervisorsAsync(model.UserId);
                return View(model);
            }

            try
            {
                // Update user basic information
                await _userService.UpdateUserInfoAsync(
                    model.UserId, 
                    model.FirstName, 
                    model.LastName, 
                    model.Email, 
                    model.Department);
                
                // Update supervisor
                await _userService.UpdateSupervisorAsync(model.UserId, model.SupervisorId);
                
                // Update CAB member status first
                await _userService.UpdateCABMemberStatusAsync(model.UserId, model.IsCABMember);
                
                // Update support personnel status next
                await _userService.UpdateSupportPersonnelStatusAsync(model.UserId, model.IsSupportPersonnel);
                
                // Update user roles last, which will preserve the special status roles
                string roles = model.SelectedRoles != null && model.SelectedRoles.Any() 
                    ? string.Join(",", model.SelectedRoles) 
                    : string.Empty;
                
                await _userService.UpdateUserRolesAsync(model.UserId, roles);

                _logger.LogInformation("User {UserId} updated successfully by {AdminUsername}", 
                    model.UserId, User.Identity?.Name);
                
                TempData["SuccessMessage"] = "User updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", model.UserId);
                ModelState.AddModelError("", "An error occurred while updating the user.");
                
                // Reload all roles and supervisors for the view
                model.AllRoles = await _userService.GetAllRolesAsync();
                model.AvailableSupervisors = await _userService.GetPotentialSupervisorsAsync(model.UserId);
                return View(model);
            }
        }
        
        // GET: UserManagement/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }
                
                // Get supervisor details if available
                User? supervisor = null;
                if (user.SupervisorId.HasValue)
                {
                    supervisor = await _userService.GetUserByIdAsync(user.SupervisorId.Value);
                }
                
                // Get subordinates
                var subordinates = await _userService.GetSubordinatesAsync(id);
                
                var viewModel = new UserDetailsViewModel
                {
                    User = user,
                    Supervisor = supervisor,
                    Subordinates = subordinates
                };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId} details", id);
                return View("Error", new ErrorViewModel { Message = "An error occurred while retrieving user details." });
            }
        }

        // GET: UserManagement/SyncRoles
        public async Task<IActionResult> SyncRoles()
        {
            try
            {
                int updatedCount = await _userService.SyncUserRolesWithSpecialStatusesAsync();
                
                _logger.LogInformation("Synchronized roles with special statuses for {Count} users", updatedCount);
                
                TempData["SuccessMessage"] = $"Successfully synchronized roles for {updatedCount} users.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error synchronizing user roles with special statuses");
                return View("Error", new ErrorViewModel { Message = "An error occurred while synchronizing user roles." });
            }
        }
    }
} 