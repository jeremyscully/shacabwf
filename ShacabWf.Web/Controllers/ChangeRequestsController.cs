using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShacabWf.Web.Models;
using ShacabWf.Web.Services;
using System.Security.Claims;

namespace ShacabWf.Web.Controllers
{
    [Authorize]
    public class ChangeRequestsController : Controller
    {
        private readonly IChangeRequestService _changeRequestService;
        private readonly IAuthService _authService;

        public ChangeRequestsController(IChangeRequestService changeRequestService, IAuthService authService)
        {
            _changeRequestService = changeRequestService;
            _authService = authService;
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

        // GET: /ChangeRequests
        public async Task<IActionResult> Index()
        {
            var changeRequests = await _changeRequestService.GetAllChangeRequestsAsync();
            return View(changeRequests);
        }

        // GET: /ChangeRequests/MyRequests
        [Route("my-requests")]
        [Route("change-requests/my-requests")]
        public async Task<IActionResult> MyRequests()
        {
            try
            {
                int userId = GetCurrentUserId();
                var myRequests = await _changeRequestService.GetChangeRequestsCreatedByUserAsync(userId);
                return View(myRequests);
            }
            catch (Exception ex)
            {
                // Log the error
                return RedirectToAction("Error", "Home", new { message = "Could not retrieve your requests. " + ex.Message });
            }
        }

        // GET: /ChangeRequests/PendingApproval
        [Authorize(Roles = "Admin,Manager,CABMember")]
        public async Task<IActionResult> PendingApproval()
        {
            try
            {
                int userId = GetCurrentUserId();
                // Get change requests that need approval from the current user
                var pendingApprovals = await _changeRequestService.GetPendingApprovalsForUserAsync(userId);
                return View(pendingApprovals);
            }
            catch (Exception ex)
            {
                // Log the error
                return RedirectToAction("Error", "Home", new { message = "Could not retrieve pending approvals. " + ex.Message });
            }
        }

        // GET: /ChangeRequests/Assigned
        public async Task<IActionResult> Assigned()
        {
            try
            {
                int userId = GetCurrentUserId();
                // Get change requests assigned to the current user
                var assignedRequests = await _changeRequestService.GetChangeRequestsAssignedToUserAsync(userId);
                return View(assignedRequests);
            }
            catch (Exception ex)
            {
                // Log the error
                return RedirectToAction("Error", "Home", new { message = "Could not retrieve your assignments. " + ex.Message });
            }
        }

        // GET: /ChangeRequests/Cab
        [Authorize(Roles = "Admin,CABMember")]
        public async Task<IActionResult> Cab()
        {
            try
            {
                // Get change requests that need CAB approval
                var cabRequests = await _changeRequestService.GetChangeRequestsForCABAsync();
                return View(cabRequests);
            }
            catch (Exception ex)
            {
                // Log the error
                return RedirectToAction("Error", "Home", new { message = "Could not retrieve CAB requests. " + ex.Message });
            }
        }

        // GET: /ChangeRequests/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var changeRequest = await _changeRequestService.GetChangeRequestByIdAsync(id);
            if (changeRequest == null)
            {
                return NotFound();
            }
            return View(changeRequest);
        }

        // GET: /ChangeRequests/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /ChangeRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChangeRequest changeRequest)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int userId = GetCurrentUserId();
                    changeRequest.CreatedById = userId;
                    await _changeRequestService.CreateChangeRequestAsync(changeRequest, userId);
                    return RedirectToAction(nameof(MyRequests));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error creating change request: " + ex.Message);
                }
            }
            return View(changeRequest);
        }
    }
} 