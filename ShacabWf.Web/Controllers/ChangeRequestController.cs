using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShacabWf.Web.Models;
using ShacabWf.Web.Services;
using System.Security.Claims;

namespace ShacabWf.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ChangeRequestController : ControllerBase
    {
        private readonly IChangeRequestService _changeRequestService;
        private readonly IAuthService _authService;

        public ChangeRequestController(IChangeRequestService changeRequestService, IAuthService authService)
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

        // GET: api/ChangeRequest
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChangeRequest>>> GetChangeRequests()
        {
            return Ok(await _changeRequestService.GetAllChangeRequestsAsync());
        }

        // GET: api/ChangeRequest/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ChangeRequest>> GetChangeRequest(int id)
        {
            var changeRequest = await _changeRequestService.GetChangeRequestByIdAsync(id);
            if (changeRequest == null)
            {
                return NotFound();
            }
            return Ok(changeRequest);
        }

        // GET: api/ChangeRequest/MyRequests
        [HttpGet("MyRequests")]
        public async Task<ActionResult<IEnumerable<ChangeRequest>>> GetMyChangeRequests()
        {
            int userId = GetCurrentUserId();
            return Ok(await _changeRequestService.GetChangeRequestsCreatedByUserAsync(userId));
        }

        // GET: api/ChangeRequest/AssignedToMe
        [HttpGet("AssignedToMe")]
        public async Task<ActionResult<IEnumerable<ChangeRequest>>> GetChangeRequestsAssignedToMe()
        {
            int userId = GetCurrentUserId();
            return Ok(await _changeRequestService.GetChangeRequestsAssignedToUserAsync(userId));
        }

        // GET: api/ChangeRequest/PendingMyApproval
        [HttpGet("PendingMyApproval")]
        public async Task<ActionResult<IEnumerable<ChangeRequest>>> GetChangeRequestsPendingMyApproval()
        {
            int userId = GetCurrentUserId();
            return Ok(await _changeRequestService.GetChangeRequestsPendingUserApprovalAsync(userId));
        }

        // POST: api/ChangeRequest
        [HttpPost]
        public async Task<ActionResult<ChangeRequest>> CreateChangeRequest(ChangeRequest changeRequest)
        {
            try
            {
                int userId = GetCurrentUserId();
                var createdChangeRequest = await _changeRequestService.CreateChangeRequestAsync(changeRequest, userId);
                return CreatedAtAction(nameof(GetChangeRequest), new { id = createdChangeRequest.Id }, createdChangeRequest);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/ChangeRequest/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateChangeRequest(int id, ChangeRequest changeRequest)
        {
            if (id != changeRequest.Id)
            {
                return BadRequest("ID mismatch");
            }

            try
            {
                int userId = GetCurrentUserId();
                await _changeRequestService.UpdateChangeRequestAsync(changeRequest, userId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/ChangeRequest/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChangeRequest(int id)
        {
            try
            {
                var result = await _changeRequestService.DeleteChangeRequestAsync(id);
                if (!result)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/ChangeRequest/5/SubmitForSupervisorApproval
        [HttpPost("{id}/SubmitForSupervisorApproval")]
        public async Task<ActionResult<ChangeRequest>> SubmitForSupervisorApproval(int id)
        {
            try
            {
                int userId = GetCurrentUserId();
                var changeRequest = await _changeRequestService.SubmitForSupervisorApprovalAsync(id, userId);
                return Ok(changeRequest);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/ChangeRequest/5/ApproveBySupervisor
        [HttpPost("{id}/ApproveBySupervisor")]
        public async Task<ActionResult<ChangeRequest>> ApproveBySupervisor(int id, [FromBody] ApprovalRequest request)
        {
            try
            {
                int userId = GetCurrentUserId();
                var changeRequest = await _changeRequestService.ApproveBySupervisorAsync(id, userId, request.Comments);
                return Ok(changeRequest);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/ChangeRequest/5/RejectBySupervisor
        [HttpPost("{id}/RejectBySupervisor")]
        public async Task<ActionResult<ChangeRequest>> RejectBySupervisor(int id, [FromBody] ApprovalRequest request)
        {
            try
            {
                int userId = GetCurrentUserId();
                var changeRequest = await _changeRequestService.RejectBySupervisorAsync(id, userId, request.Comments);
                return Ok(changeRequest);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/ChangeRequest/5/SubmitForCABApproval
        [HttpPost("{id}/SubmitForCABApproval")]
        public async Task<ActionResult<ChangeRequest>> SubmitForCABApproval(int id)
        {
            try
            {
                int userId = GetCurrentUserId();
                var changeRequest = await _changeRequestService.SubmitForCABApprovalAsync(id, userId);
                return Ok(changeRequest);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/ChangeRequest/5/ApproveByCAB
        [HttpPost("{id}/ApproveByCAB")]
        public async Task<ActionResult<ChangeRequest>> ApproveByCAB(int id, [FromBody] ApprovalRequest request)
        {
            try
            {
                int userId = GetCurrentUserId();
                var changeRequest = await _changeRequestService.ApproveByCABAsync(id, userId, request.Comments);
                return Ok(changeRequest);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/ChangeRequest/5/RejectByCAB
        [HttpPost("{id}/RejectByCAB")]
        public async Task<ActionResult<ChangeRequest>> RejectByCAB(int id, [FromBody] ApprovalRequest request)
        {
            try
            {
                int userId = GetCurrentUserId();
                var changeRequest = await _changeRequestService.RejectByCABAsync(id, userId, request.Comments);
                return Ok(changeRequest);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/ChangeRequest/5/Schedule
        [HttpPost("{id}/Schedule")]
        public async Task<ActionResult<ChangeRequest>> ScheduleChangeRequest(int id, [FromBody] ScheduleRequest request)
        {
            try
            {
                int userId = GetCurrentUserId();
                var changeRequest = await _changeRequestService.ScheduleChangeRequestAsync(id, userId, request.StartDate, request.EndDate);
                return Ok(changeRequest);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/ChangeRequest/5/AssignSupportPersonnel
        [HttpPost("{id}/AssignSupportPersonnel")]
        public async Task<ActionResult<ChangeRequest>> AssignSupportPersonnel(int id, [FromBody] AssignmentRequest request)
        {
            try
            {
                int userId = GetCurrentUserId();
                var changeRequest = await _changeRequestService.AssignSupportPersonnelAsync(id, userId, request.AssigneeId, request.Role, request.Notes);
                return Ok(changeRequest);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/ChangeRequest/5/StartImplementation
        [HttpPost("{id}/StartImplementation")]
        public async Task<ActionResult<ChangeRequest>> StartImplementation(int id)
        {
            try
            {
                int userId = GetCurrentUserId();
                var changeRequest = await _changeRequestService.StartImplementationAsync(id, userId);
                return Ok(changeRequest);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/ChangeRequest/5/CompleteImplementation
        [HttpPost("{id}/CompleteImplementation")]
        public async Task<ActionResult<ChangeRequest>> CompleteImplementation(int id)
        {
            try
            {
                int userId = GetCurrentUserId();
                var changeRequest = await _changeRequestService.CompleteImplementationAsync(id, userId);
                return Ok(changeRequest);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/ChangeRequest/5/FailImplementation
        [HttpPost("{id}/FailImplementation")]
        public async Task<ActionResult<ChangeRequest>> FailImplementation(int id, [FromBody] FailureRequest request)
        {
            try
            {
                int userId = GetCurrentUserId();
                var changeRequest = await _changeRequestService.FailImplementationAsync(id, userId, request.Reason);
                return Ok(changeRequest);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/ChangeRequest/5/Cancel
        [HttpPost("{id}/Cancel")]
        public async Task<ActionResult<ChangeRequest>> CancelChangeRequest(int id, [FromBody] CancellationRequest request)
        {
            try
            {
                int userId = GetCurrentUserId();
                var changeRequest = await _changeRequestService.CancelChangeRequestAsync(id, userId, request.Reason);
                return Ok(changeRequest);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/ChangeRequest/5/Comment
        [HttpPost("{id}/Comment")]
        public async Task<ActionResult<ChangeRequestComment>> AddComment(int id, [FromBody] CommentRequest request)
        {
            try
            {
                int userId = GetCurrentUserId();
                var comment = await _changeRequestService.AddCommentAsync(id, userId, request.Text, request.IsInternal);
                return Ok(comment);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/ChangeRequest/5/Comments
        [HttpGet("{id}/Comments")]
        public async Task<ActionResult<IEnumerable<ChangeRequestComment>>> GetComments(int id, [FromQuery] bool includeInternal = false)
        {
            try
            {
                var comments = await _changeRequestService.GetCommentsForChangeRequestAsync(id, includeInternal);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/ChangeRequest/5/History
        [HttpGet("{id}/History")]
        public async Task<ActionResult<IEnumerable<ChangeRequestHistory>>> GetHistory(int id)
        {
            try
            {
                var history = await _changeRequestService.GetHistoryForChangeRequestAsync(id);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    // Request models
    public class ApprovalRequest
    {
        public string? Comments { get; set; }
    }

    public class ScheduleRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class AssignmentRequest
    {
        public int AssigneeId { get; set; }
        public string Role { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class FailureRequest
    {
        public string Reason { get; set; } = string.Empty;
    }

    public class CancellationRequest
    {
        public string Reason { get; set; } = string.Empty;
    }

    public class CommentRequest
    {
        public string Text { get; set; } = string.Empty;
        public bool IsInternal { get; set; } = false;
    }
} 