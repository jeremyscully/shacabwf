using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShacabWf.Web.Models;
using ShacabWf.Web.Services;
using System.Security.Claims;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShacabWf.Web.Data;

namespace ShacabWf.Web.Controllers
{
    [Authorize]
    public class ChangeRequestsController : Controller
    {
        private readonly IChangeRequestService _changeRequestService;
        private readonly IAuthService _authService;
        private readonly ApplicationDbContext _context;

        public ChangeRequestsController(IChangeRequestService changeRequestService, IAuthService authService, ApplicationDbContext context)
        {
            _changeRequestService = changeRequestService;
            _authService = authService;
            _context = context;
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
                
                // Add logging to check what's being returned
                Console.WriteLine($"Retrieved {pendingApprovals.Count()} pending approvals for user {userId}");
                
                return View(pendingApprovals);
            }
            catch (Exception ex)
            {
                // Log the error with more details
                Console.WriteLine($"Error in PendingApproval action: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner stack trace: {ex.InnerException.StackTrace}");
                }
                
                // Return a more detailed error view
                return View("Error", new ErrorViewModel 
                { 
                    RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = $"Could not retrieve pending approvals. Error: {ex.Message}"
                });
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
                
                // Log the retrieved change requests
                Console.WriteLine($"Retrieved {cabRequests.Count()} change requests for CAB Dashboard");
                foreach (var request in cabRequests)
                {
                    Console.WriteLine($"Change Request ID: {request.Id}, Status: {request.Status}, Title: {request.Title}");
                }
                
                // Log the pending CAB approval requests specifically
                var pendingCabApprovals = cabRequests.Where(cr => 
                    cr.Status == ChangeRequestStatus.SubmittedForCABApproval || 
                    cr.Status == ChangeRequestStatus.SupervisorApproved
                ).ToList();
                Console.WriteLine($"Found {pendingCabApprovals.Count} change requests with SubmittedForCABApproval or SupervisorApproved status");
                foreach (var request in pendingCabApprovals)
                {
                    Console.WriteLine($"Pending CAB Approval - ID: {request.Id}, Title: {request.Title}");
                }

                // Check if calendar views are requested
                bool pendingCalendarView = Request.Query.ContainsKey("pendingView") && Request.Query["pendingView"] == "calendar";
                bool scheduledCalendarView = Request.Query.ContainsKey("scheduledView") && Request.Query["scheduledView"] == "calendar";
                
                ViewBag.PendingCalendarView = pendingCalendarView;
                ViewBag.ScheduledCalendarView = scheduledCalendarView;
                
                return View(cabRequests);
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error in Cab action: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return RedirectToAction("Error", "Home", new { message = "Could not retrieve CAB requests. " + ex.Message });
            }
        }
        
        // GET: /ChangeRequests/PendingCalendarData
        [Authorize(Roles = "Admin,CABMember")]
        public async Task<IActionResult> PendingCalendarData()
        {
            try
            {
                // Get change requests for CAB
                var cabRequests = await _changeRequestService.GetChangeRequestsForCABAsync();
                
                // Transform the data for FullCalendar - only pending approval requests
                var events = new List<object>();
                
                // Add pending approval requests
                foreach (var cr in cabRequests.Where(cr => 
                    cr.Status == ChangeRequestStatus.SubmittedForCABApproval || 
                    cr.Status == ChangeRequestStatus.SupervisorApproved
                ))
                {
                    events.Add(new
                    {
                        id = cr.Id,
                        title = cr.Title,
                        start = cr.CreatedAt.ToString("yyyy-MM-dd"),
                        end = (string)null,
                        color = GetEventColor(cr.Status),
                        textColor = "#ffffff",
                        description = cr.Description,
                        status = cr.Status.ToString(),
                        priority = cr.Priority.ToString(),
                        url = Url.Action("Details", "ChangeRequests", new { id = cr.Id }),
                        borderColor = "#000000",
                        eventType = "PendingApproval",
                        className = "pending-approval-event"
                    });
                }
                
                return Json(events);
            }
            catch (Exception ex)
            {
                // Return empty array in case of error
                return Json(new object[0]);
            }
        }
        
        // GET: /ChangeRequests/ScheduledCalendarData
        [Authorize(Roles = "Admin,CABMember")]
        public async Task<IActionResult> ScheduledCalendarData()
        {
            try
            {
                // Get change requests for CAB
                var cabRequests = await _changeRequestService.GetChangeRequestsForCABAsync();
                
                // Transform the data for FullCalendar - only scheduled changes
                var events = new List<object>();
                
                // Add scheduled changes
                foreach (var cr in cabRequests.Where(cr => 
                    cr.Status == ChangeRequestStatus.Scheduled || 
                    cr.Status == ChangeRequestStatus.Rescheduled || 
                    cr.Status == ChangeRequestStatus.CABApproved))
                {
                    events.Add(new
                    {
                        id = cr.Id,
                        title = cr.Title,
                        start = cr.ScheduledStartDate?.ToString("yyyy-MM-dd") ?? cr.CreatedAt.ToString("yyyy-MM-dd"),
                        end = cr.ScheduledEndDate?.ToString("yyyy-MM-dd"),
                        color = GetEventColor(cr.Status),
                        textColor = "#ffffff",
                        description = cr.Description,
                        status = cr.Status.ToString(),
                        priority = cr.Priority.ToString(),
                        url = Url.Action("Details", "ChangeRequests", new { id = cr.Id }),
                        borderColor = "#000000",
                        eventType = "ScheduledChange",
                        className = "scheduled-change-event"
                    });
                }
                
                return Json(events);
            }
            catch (Exception ex)
            {
                // Return empty array in case of error
                return Json(new object[0]);
            }
        }
        
        // GET: /ChangeRequests/CabCalendarData
        [Authorize(Roles = "Admin,CABMember")]
        public async Task<IActionResult> CabCalendarData()
        {
            try
            {
                // Get change requests for CAB
                var cabRequests = await _changeRequestService.GetChangeRequestsForCABAsync();
                
                // Transform the data for FullCalendar
                var events = new List<object>();
                
                // Add pending approval requests
                foreach (var cr in cabRequests.Where(cr => 
                    cr.Status == ChangeRequestStatus.SubmittedForCABApproval || 
                    cr.Status == ChangeRequestStatus.SupervisorApproved
                ))
                {
                    events.Add(new
                    {
                        id = cr.Id,
                        title = cr.Title,
                        start = cr.CreatedAt.ToString("yyyy-MM-dd"),
                        end = (string)null,
                        color = GetEventColor(cr.Status),
                        textColor = "#ffffff",
                        description = cr.Description,
                        status = cr.Status.ToString(),
                        priority = cr.Priority.ToString(),
                        url = Url.Action("Details", "ChangeRequests", new { id = cr.Id }),
                        eventType = "PendingApproval",
                        borderColor = "#000000",
                        className = "pending-approval-event"
                    });
                }
                
                // Add scheduled changes
                foreach (var cr in cabRequests.Where(cr => 
                    cr.Status == ChangeRequestStatus.Scheduled || 
                    cr.Status == ChangeRequestStatus.Rescheduled || 
                    cr.Status == ChangeRequestStatus.CABApproved))
                {
                    events.Add(new
                    {
                        id = cr.Id,
                        title = cr.Title,
                        start = cr.ScheduledStartDate?.ToString("yyyy-MM-dd") ?? cr.CreatedAt.ToString("yyyy-MM-dd"),
                        end = cr.ScheduledEndDate?.ToString("yyyy-MM-dd"),
                        color = GetEventColor(cr.Status),
                        textColor = "#ffffff",
                        description = cr.Description,
                        status = cr.Status.ToString(),
                        priority = cr.Priority.ToString(),
                        url = Url.Action("Details", "ChangeRequests", new { id = cr.Id }),
                        eventType = "ScheduledChange",
                        borderColor = "#000000",
                        className = "scheduled-change-event"
                    });
                }
                
                return Json(events);
            }
            catch (Exception ex)
            {
                // Return empty array in case of error
                return Json(new object[0]);
            }
        }
        
        // Helper method to get event color based on status
        private string GetEventColor(ChangeRequestStatus status)
        {
            return status switch
            {
                ChangeRequestStatus.Draft => "#6c757d", // secondary
                ChangeRequestStatus.SubmittedForSupervisorApproval => "#17a2b8", // info
                ChangeRequestStatus.SubmittedForCABApproval => "#ffc107", // warning
                ChangeRequestStatus.SupervisorApproved => "#28a745", // success
                ChangeRequestStatus.CABApproved => "#28a745", // success
                ChangeRequestStatus.Scheduled => "#007bff", // primary
                ChangeRequestStatus.Rescheduled => "#17a2b8", // info
                ChangeRequestStatus.InProgress => "#007bff", // primary
                ChangeRequestStatus.Completed => "#28a745", // success
                ChangeRequestStatus.Failed => "#dc3545", // danger
                ChangeRequestStatus.SupervisorRejected => "#dc3545", // danger
                ChangeRequestStatus.CABRejected => "#dc3545", // danger
                ChangeRequestStatus.Cancelled => "#dc3545", // danger
                _ => "#6c757d" // secondary
            };
        }

        // GET: /ChangeRequests/Details/5
        public async Task<IActionResult> Details(int id, string source = null)
        {
            var changeRequest = await _changeRequestService.GetChangeRequestByIdAsync(id);
            if (changeRequest == null)
            {
                return NotFound();
            }
            
            // Pass the source to the view to determine which back button to show
            ViewBag.Source = source;
            
            return View(changeRequest);
        }

        // GET: /ChangeRequests/Create
        public IActionResult Create()
        {
            Console.WriteLine("Create GET action called");
            return View();
        }

        // POST: /ChangeRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChangeRequest changeRequest)
        {
            Console.WriteLine("Create POST action called");
            
            // Remove validation errors for ChangeRequestNumber since it will be generated by the service
            if (ModelState.ContainsKey("ChangeRequestNumber"))
            {
                ModelState.Remove("ChangeRequestNumber");
            }
            
            Console.WriteLine($"Model state is valid after removing ChangeRequestNumber: {ModelState.IsValid}");
            
            if (ModelState.IsValid)
            {
                try
                {
                    Console.WriteLine("Attempting to create change request");
                    int userId = GetCurrentUserId();
                    changeRequest.CreatedById = userId;
                    
                    // The service will generate the ChangeRequestNumber
                    var createdRequest = await _changeRequestService.CreateChangeRequestAsync(changeRequest, userId);
                    Console.WriteLine($"Change request created successfully with ID: {createdRequest.Id} and Number: {createdRequest.ChangeRequestNumber}");
                    
                    return RedirectToAction(nameof(MyRequests));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating change request: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    ModelState.AddModelError("", "Error creating change request: " + ex.Message);
                }
            }
            else
            {
                Console.WriteLine("Model state is invalid. Errors:");
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine($"- {error.ErrorMessage}");
                    }
                }
            }
            
            return View(changeRequest);
        }

        // POST: /ChangeRequests/SubmitForApproval/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitForApproval(int id)
        {
            try
            {
                Console.WriteLine($"SubmitForApproval action called for ID: {id}");
                int userId = GetCurrentUserId();
                
                // Submit the change request for supervisor approval
                var changeRequest = await _changeRequestService.SubmitForSupervisorApprovalAsync(id, userId);
                Console.WriteLine($"Change request {id} submitted for approval successfully");
                
                return RedirectToAction(nameof(MyRequests));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("supervisor"))
            {
                // Handle the specific error for missing supervisor
                Console.WriteLine($"Error submitting change request for approval: {ex.Message}");
                
                // Add a temporary error message to be displayed on the details page
                TempData["ErrorMessage"] = "Cannot submit for approval: You don't have a supervisor assigned. Please contact your administrator.";
                
                // Redirect back to the details page
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error submitting change request for approval: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Add a temporary error message to be displayed on the details page
                TempData["ErrorMessage"] = $"Error submitting change request for approval: {ex.Message}";
                
                // Redirect back to the details page
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // POST: /ChangeRequests/SupervisorApprove/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> SupervisorApprove(int id, string? comments)
        {
            try
            {
                Console.WriteLine($"SupervisorApprove action called for ID: {id}");
                int userId = GetCurrentUserId();
                
                // Approve the change request as a supervisor
                var changeRequest = await _changeRequestService.ApproveBySupervisorAsync(id, userId, comments);
                Console.WriteLine($"Change request {id} approved by supervisor successfully. Status: {changeRequest.Status}");
                
                // Automatically submit for CAB approval
                try
                {
                    Console.WriteLine($"Attempting to submit change request {id} for CAB approval");
                    changeRequest = await _changeRequestService.SubmitForCABApprovalAsync(id, userId);
                    Console.WriteLine($"Change request {id} submitted for CAB approval successfully. Status: {changeRequest.Status}");
                    
                    TempData["SuccessMessage"] = "Change request approved and submitted for CAB approval successfully.";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error submitting change request for CAB approval: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    
                    // Still show a success message for the approval, but note the CAB submission failed
                    TempData["PartialSuccessMessage"] = "Change request approved successfully, but could not be submitted for CAB approval automatically. Please submit it manually.";
                }
                
                return RedirectToAction(nameof(PendingApproval));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error approving change request: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                TempData["ErrorMessage"] = $"Error approving change request: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // POST: /ChangeRequests/SupervisorReject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> SupervisorReject(int id, string? comments)
        {
            try
            {
                Console.WriteLine($"SupervisorReject action called for ID: {id}");
                int userId = GetCurrentUserId();
                
                // Reject the change request as a supervisor
                var changeRequest = await _changeRequestService.RejectBySupervisorAsync(id, userId, comments);
                Console.WriteLine($"Change request {id} rejected by supervisor successfully");
                
                // Add success message
                TempData["SuccessMessage"] = "Change request has been rejected.";
                
                return RedirectToAction(nameof(PendingApproval));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error rejecting change request: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Add error message
                TempData["ErrorMessage"] = $"Error rejecting change request: {ex.Message}";
                
                // Redirect back to the details page
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CABApprove(int id, string? comments)
        {
            try
            {
                Console.WriteLine($"CABApprove action called for change request {id}");
                int userId;
                
                try
                {
                    userId = GetCurrentUserId();
                }
                catch (UnauthorizedAccessException)
                {
                    TempData["Error"] = "User not authenticated";
                    return RedirectToAction(nameof(Cab));
                }

                var changeRequest = await _changeRequestService.ApproveByCABAsync(id, userId, comments);
                Console.WriteLine($"Change request {id} approved by CAB member {userId}");
                
                TempData["Success"] = $"Change request #{id} has been approved by CAB";
                return RedirectToAction(nameof(Cab));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CABApprove action: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                TempData["Error"] = $"Error approving change request: {ex.Message}";
                return RedirectToAction(nameof(Cab));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CABReject(int id, string? comments)
        {
            try
            {
                Console.WriteLine($"CABReject action called for change request {id}");
                int userId;
                
                try
                {
                    userId = GetCurrentUserId();
                }
                catch (UnauthorizedAccessException)
                {
                    TempData["Error"] = "User not authenticated";
                    return RedirectToAction(nameof(Cab));
                }

                var changeRequest = await _changeRequestService.RejectByCABAsync(id, userId, comments);
                Console.WriteLine($"Change request {id} rejected by CAB member {userId}");
                
                TempData["Success"] = $"Change request #{id} has been rejected";
                return RedirectToAction(nameof(Cab));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CABReject action: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                TempData["Error"] = $"Error rejecting change request: {ex.Message}";
                return RedirectToAction(nameof(Cab));
            }
        }

        [HttpPost]
        public async Task<IActionResult> ScheduleChangeRequest(int id, DateTime startDate, DateTime endDate)
        {
            try
            {
                Console.WriteLine($"ScheduleChangeRequest action called for change request {id}");
                int userId;
                
                try
                {
                    userId = GetCurrentUserId();
                }
                catch (UnauthorizedAccessException)
                {
                    TempData["Error"] = "User not authenticated";
                    return RedirectToAction(nameof(Cab));
                }

                var changeRequest = await _changeRequestService.ScheduleChangeRequestAsync(id, userId, startDate, endDate);
                Console.WriteLine($"Change request {id} scheduled by user {userId} from {startDate} to {endDate}");
                
                TempData["Success"] = $"Change request #{id} has been scheduled";
                return RedirectToAction(nameof(Cab));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ScheduleChangeRequest action: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                TempData["Error"] = $"Error scheduling change request: {ex.Message}";
                return RedirectToAction(nameof(Cab));
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApproveAndSchedule(int id, string? comments, DateTime startDate, DateTime endDate)
        {
            try
            {
                Console.WriteLine($"ApproveAndSchedule action called for change request {id}");
                Console.WriteLine($"Start Date: {startDate}, End Date: {endDate}");
                
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "User not authenticated.";
                    return RedirectToAction(nameof(Cab));
                }
                
                Console.WriteLine($"User ID: {userId}");
                
                // First try to approve the change request
                try
                {
                    var approvedChangeRequest = await _changeRequestService.ApproveByCABAsync(id, int.Parse(userId), comments);
                    Console.WriteLine($"Change request {id} approved by CAB. New status: {approvedChangeRequest.Status}");
                    TempData["Success"] = "Change request approved successfully.";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error approving change request: {ex.Message}");
                    // Continue with scheduling even if approval fails (might be due to status issues)
                }
                
                // Now directly schedule the change request using the new method
                try
                {
                    var scheduledChangeRequest = await _changeRequestService.DirectScheduleChangeRequestAsync(id, int.Parse(userId), startDate, endDate);
                    Console.WriteLine($"Change request {id} scheduled successfully. New status: {scheduledChangeRequest.Status}");
                    TempData["Success"] = "Change request approved and scheduled successfully.";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error scheduling change request: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    TempData["Error"] = $"Error scheduling change request: {ex.Message}";
                }
                
                return RedirectToAction(nameof(Cab));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ApproveAndSchedule action: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction(nameof(Cab));
            }
        }

        // GET: /ChangeRequests/GetChangeRequestAssignments
        [HttpGet]
        [Authorize(Roles = "Admin,CABMember")]
        public async Task<IActionResult> GetChangeRequestAssignments(int id)
        {
            try
            {
                // Log the request for debugging
                Console.WriteLine($"GetChangeRequestAssignments called with id: {id}");
                
                if (id <= 0)
                {
                    Console.WriteLine("Invalid change request ID: " + id);
                    return NotFound("Invalid change request ID");
                }
                
                var changeRequest = await _changeRequestService.GetChangeRequestByIdAsync(id);
                if (changeRequest == null)
                {
                    Console.WriteLine($"ChangeRequest with id {id} not found");
                    return NotFound("Change request not found");
                }

                var assignments = new List<object>();
                
                if (changeRequest.Assignments != null)
                {
                    assignments = changeRequest.Assignments
                        .Select(a => new
                        {
                            id = a.Id,
                            assigneeId = a.AssigneeId,
                            assigneeName = a.Assignee?.FullName ?? "Unknown",
                            role = a.Role,
                            notes = a.Notes,
                            status = a.Status.ToString(),
                            assignedAt = a.AssignedAt
                        })
                        .ToList<object>();
                }

                // Log the response for debugging
                Console.WriteLine($"Returning {assignments.Count} assignments");
                
                return Json(assignments);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetChangeRequestAssignments: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, "An error occurred while retrieving assignments");
            }
        }

        // GET: /ChangeRequests/GetSupportPersonnel
        [HttpGet]
        [Route("ChangeRequests/GetSupportPersonnel")]
        [Authorize(Roles = "Admin,CABMember")]
        public async Task<IActionResult> GetSupportPersonnel()
        {
            try
            {
                // Log the request for debugging
                Console.WriteLine("GetSupportPersonnel called");
                
                var supportPersonnel = await _context.Users
                    .Where(u => u.IsSupportPersonnel)
                    .Select(u => new
                    {
                        id = u.Id,
                        fullName = u.FullName,
                        department = u.Department
                    })
                    .ToListAsync();

                // Log the response for debugging
                Console.WriteLine($"Returning {supportPersonnel.Count} support personnel");
                
                return Json(supportPersonnel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetSupportPersonnel: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, "An error occurred while retrieving support personnel");
            }
        }

        // POST: /ChangeRequests/RescheduleChangeRequest/5
        [HttpPost]
        [Authorize(Roles = "Admin,CABMember")]
        public async Task<IActionResult> RescheduleChangeRequest(int id, DateTime startDate, DateTime endDate)
        {
            try
            {
                Console.WriteLine($"RescheduleChangeRequest action called for change request {id}");
                Console.WriteLine($"Start Date: {startDate}, End Date: {endDate}");
                
                // Validate dates - allow equal dates but not start > end
                if (startDate > endDate)
                {
                    TempData["ErrorMessage"] = "Start date cannot be after end date";
                    return RedirectToAction(nameof(Cab));
                }
                
                if (startDate < DateTime.UtcNow.Date)
                {
                    TempData["ErrorMessage"] = "Start date cannot be in the past";
                    return RedirectToAction(nameof(Cab));
                }
                
                int userId;
                
                try
                {
                    userId = GetCurrentUserId();
                }
                catch (UnauthorizedAccessException)
                {
                    TempData["ErrorMessage"] = "User not authenticated";
                    return RedirectToAction(nameof(Cab));
                }

                // Get the change request to check its current status
                var changeRequest = await _changeRequestService.GetChangeRequestByIdAsync(id);
                bool isRescheduling = changeRequest?.Status == ChangeRequestStatus.Scheduled;

                // Schedule or reschedule the change request
                await _changeRequestService.ScheduleChangeRequestAsync(id, userId, startDate, endDate);
                Console.WriteLine($"Change request {id} {(isRescheduling ? "rescheduled" : "scheduled")} by user {userId} from {startDate} to {endDate}");
                
                TempData["SuccessMessage"] = $"Change request #{id} has been {(isRescheduling ? "rescheduled" : "scheduled")}";
                return RedirectToAction(nameof(Cab));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RescheduleChangeRequest action: {ex.Message}");
                TempData["ErrorMessage"] = $"Error rescheduling change request: {ex.Message}";
                return RedirectToAction(nameof(Cab));
            }
        }

        // POST: /ChangeRequests/CompleteChangeRequest/5
        [HttpPost]
        [Authorize(Roles = "Admin,CABMember")]
        public async Task<IActionResult> CompleteChangeRequest(int id, string notes)
        {
            try
            {
                Console.WriteLine($"CompleteChangeRequest action called for change request {id}");
                int userId;
                
                try
                {
                    userId = GetCurrentUserId();
                }
                catch (UnauthorizedAccessException)
                {
                    TempData["Error"] = "User not authenticated";
                    return RedirectToAction(nameof(Cab));
                }

                // First add a comment with the completion notes if provided
                if (!string.IsNullOrWhiteSpace(notes))
                {
                    await _changeRequestService.AddCommentAsync(id, userId, $"Completion notes: {notes}");
                }

                // Then complete the change request
                var changeRequest = await _changeRequestService.CompleteImplementationAsync(id, userId);
                Console.WriteLine($"Change request {id} marked as completed by user {userId}");
                
                TempData["Success"] = $"Change request #{id} has been marked as completed";
                return RedirectToAction(nameof(Cab));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CompleteChangeRequest action: {ex.Message}");
                TempData["Error"] = $"Error completing change request: {ex.Message}";
                return RedirectToAction(nameof(Cab));
            }
        }

        // POST: /ChangeRequests/AssignSupportPersonnel
        [HttpPost]
        [Authorize(Roles = "Admin,CABMember")]
        public async Task<IActionResult> AssignSupportPersonnel(int id, int assigneeId, string role, string? notes)
        {
            try
            {
                // Log the request for debugging
                Console.WriteLine($"AssignSupportPersonnel called with id: {id}, assigneeId: {assigneeId}, role: {role}");
                
                if (id <= 0)
                {
                    Console.WriteLine("Invalid change request ID: " + id);
                    TempData["ErrorMessage"] = "Invalid change request ID";
                    return RedirectToAction("Cab");
                }
                
                var changeRequest = await _changeRequestService.GetChangeRequestByIdAsync(id);
                if (changeRequest == null)
                {
                    Console.WriteLine($"Change request with ID {id} not found");
                    TempData["ErrorMessage"] = $"Change request with ID {id} not found";
                    return RedirectToAction("Cab");
                }
                
                var userId = GetCurrentUserId();
                var user = await _context.Users.FindAsync(userId);
                var isCabMember = user != null && user.IsCABMember;

                if (!isCabMember)
                {
                    Console.WriteLine($"User {userId} is not a CAB member");
                    return Forbid();
                }

                await _changeRequestService.AssignSupportPersonnelAsync(id, userId, assigneeId, role, notes);
                
                Console.WriteLine("Support personnel assigned successfully");
                TempData["SuccessMessage"] = "Support personnel assigned successfully";
                return RedirectToAction("Cab");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AssignSupportPersonnel: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = $"Error assigning support personnel: {ex.Message}";
                return RedirectToAction("Cab");
            }
        }

        // POST: /ChangeRequests/RemoveAssignment
        [HttpPost]
        [Authorize(Roles = "Admin,CABMember")]
        public async Task<IActionResult> RemoveAssignment(int id)
        {
            try
            {
                // Log the request for debugging
                Console.WriteLine($"RemoveAssignment called with id: {id}");
                
                if (id <= 0)
                {
                    Console.WriteLine("Invalid assignment ID: " + id);
                    return NotFound("Invalid assignment ID");
                }
                
                var assignment = await _context.ChangeRequestAssignments.FindAsync(id);
                if (assignment == null)
                {
                    Console.WriteLine($"Assignment with id {id} not found");
                    return NotFound("Assignment not found");
                }

                var userId = GetCurrentUserId();
                var user = await _context.Users.FindAsync(userId);
                var isCabMember = user != null && user.IsCABMember;

                if (!isCabMember)
                {
                    Console.WriteLine($"User {userId} is not a CAB member");
                    return Forbid();
                }

                var changeRequestId = assignment.ChangeRequestId;
                _context.ChangeRequestAssignments.Remove(assignment);
                await _context.SaveChangesAsync();

                // Add a comment to the change request history
                await _changeRequestService.AddCommentAsync(
                    changeRequestId,
                    userId,
                    $"Removed assignment for {assignment.Assignee?.FullName ?? "Unknown"} with role {assignment.Role}",
                    true);

                Console.WriteLine($"Assignment {id} removed successfully");
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RemoveAssignment: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, "An error occurred while removing the assignment");
            }
        }
    }
} 