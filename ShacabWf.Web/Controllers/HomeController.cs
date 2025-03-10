using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShacabWf.Web.Data;
using ShacabWf.Web.Models;
using ShacabWf.Web.Services;
using System.Diagnostics;

namespace ShacabWf.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IAuthService authService)
        {
            _logger = logger;
            _context = context;
            _authService = authService;
        }

        public IActionResult Index()
        {
            // Redirect to the Simple action to make it the default home page
            return RedirectToAction("Simple");
            
            // The code below is kept for reference but will not be executed due to the redirect above
            /*
            try
            {
                // Get the current user's name from claims
                var userName = User.Identity?.Name ?? "User";
                
                // Find the user in the database
                var user = await _authService.GetUserByUsernameAsync(userName);

                if (user == null)
                {
                    // This shouldn't happen with our authentication system, but just in case
                    return RedirectToAction("Logout", "Account");
                }

                // Get recent change requests
                var recentChangeRequests = await _context.ChangeRequests
                    .Include(cr => cr.CreatedBy)
                    .OrderByDescending(cr => cr.CreatedAt)
                    .Take(5)
                    .ToListAsync();

                // Get change requests created by the user
                var userChangeRequests = await _context.ChangeRequests
                    .Where(cr => cr.CreatedById == user.Id)
                    .OrderByDescending(cr => cr.CreatedAt)
                    .Take(5)
                    .ToListAsync();

                // Get change requests pending the user's approval (if they are a supervisor or CAB member)
                var pendingApprovals = new List<ChangeRequest>();
                if (user.IsCABMember || await _context.Users.AnyAsync(u => u.SupervisorId == user.Id))
                {
                    pendingApprovals = await _context.ChangeRequestApprovals
                        .Where(a => a.ApproverId == user.Id && a.Status == ApprovalStatus.Pending)
                        .Include(a => a.ChangeRequest)
                            .ThenInclude(cr => cr.CreatedBy)
                        .Select(a => a.ChangeRequest)
                        .Distinct()
                        .OrderByDescending(cr => cr.UpdatedAt ?? cr.CreatedAt)
                        .Take(5)
                        .ToListAsync();
                }

                // Get change requests assigned to the user (if they are support personnel)
                var assignedChangeRequests = new List<ChangeRequest>();
                if (user.IsSupportPersonnel)
                {
                    assignedChangeRequests = await _context.ChangeRequestAssignments
                        .Where(a => a.AssigneeId == user.Id)
                        .Include(a => a.ChangeRequest)
                            .ThenInclude(cr => cr.CreatedBy)
                        .Select(a => a.ChangeRequest)
                        .Distinct()
                        .OrderByDescending(cr => cr.UpdatedAt ?? cr.CreatedAt)
                        .Take(5)
                        .ToListAsync();
                }

                // Create the dashboard model
                var model = new DashboardViewModel
                {
                    User = user,
                    RecentChangeRequests = recentChangeRequests,
                    UserChangeRequests = userChangeRequests,
                    PendingApprovals = pendingApprovals,
                    AssignedChangeRequests = assignedChangeRequests,
                    CurrentTime = DateTime.Now
                };
                
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Index action");
                return RedirectToAction(nameof(Simple));
            }
            */
        }

        [AllowAnonymous]
        public async Task<IActionResult> Simple()
        {
            // Set current user for the view if authenticated
            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    // Find user by username
                    var currentUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Username == User.Identity.Name);
                    
                    ViewBag.CurrentUser = currentUser;
                    
                    // Log if user not found
                    if (currentUser == null)
                    {
                        _logger.LogWarning("User not found for username: {Username}", User.Identity.Name);
                    }
                }
                catch (Exception ex)
                {
                    // Log any errors that occur when retrieving the user
                    _logger.LogError(ex, "Error retrieving user for username: {Username}", User.Identity.Name);
                }
            }
            
            return View("SimpleIndex");
        }

        [AllowAnonymous]
        public IActionResult Test()
        {
            return View();
        }

        [Authorize]
        public IActionResult SimpleDashboard()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult TestData()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class DashboardViewModel
    {
        public DashboardViewModel()
        {
            // Initialize collections to empty to avoid null reference exceptions
            RecentChangeRequests = Array.Empty<ChangeRequest>();
            UserChangeRequests = Array.Empty<ChangeRequest>();
            PendingApprovals = Array.Empty<ChangeRequest>();
            AssignedChangeRequests = Array.Empty<ChangeRequest>();
            CurrentTime = DateTime.Now;
        }

        public User? User { get; set; }
        public IEnumerable<ChangeRequest> RecentChangeRequests { get; set; }
        public IEnumerable<ChangeRequest> UserChangeRequests { get; set; }
        public IEnumerable<ChangeRequest> PendingApprovals { get; set; }
        public IEnumerable<ChangeRequest> AssignedChangeRequests { get; set; }
        public DateTime CurrentTime { get; set; }
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        
        public string? Message { get; set; }
    }
} 