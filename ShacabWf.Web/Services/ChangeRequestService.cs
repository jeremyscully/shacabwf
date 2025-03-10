using ShacabWf.Web.Models;
using ShacabWf.Web.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ShacabWf.Web.Services
{
    public class ChangeRequestService : IChangeRequestService
    {
        private readonly ApplicationDbContext _context;

        public ChangeRequestService(ApplicationDbContext context)
        {
            _context = context;
        }

        // CRUD Operations
        public async Task<IEnumerable<ChangeRequest>> GetAllChangeRequestsAsync()
        {
            return await _context.ChangeRequests
                .Include(cr => cr.CreatedBy)
                .Include(cr => cr.Approvals)
                .Include(cr => cr.Assignments)
                .ToListAsync();
        }

        public async Task<ChangeRequest?> GetChangeRequestByIdAsync(int id)
        {
            // First get the basic change request
            var changeRequest = await _context.ChangeRequests
                .FirstOrDefaultAsync(cr => cr.Id == id);

            if (changeRequest == null)
            {
                return null;
            }

            // Then load related entities separately to avoid nullability warnings
            await _context.Entry(changeRequest)
                .Reference(cr => cr.CreatedBy)
                .LoadAsync();

            await _context.Entry(changeRequest)
                .Collection(cr => cr.Approvals)
                .LoadAsync();

            foreach (var approval in changeRequest.Approvals)
            {
                await _context.Entry(approval)
                    .Reference(a => a.Approver)
                    .LoadAsync();
            }

            await _context.Entry(changeRequest)
                .Collection(cr => cr.Assignments)
                .LoadAsync();

            foreach (var assignment in changeRequest.Assignments)
            {
                await _context.Entry(assignment)
                    .Reference(a => a.Assignee)
                    .LoadAsync();
            }

            await _context.Entry(changeRequest)
                .Collection(cr => cr.Comments)
                .LoadAsync();

            foreach (var comment in changeRequest.Comments)
            {
                await _context.Entry(comment)
                    .Reference(c => c.Commenter)
                    .LoadAsync();
            }

            await _context.Entry(changeRequest)
                .Collection(cr => cr.History)
                .LoadAsync();

            foreach (var history in changeRequest.History)
            {
                await _context.Entry(history)
                    .Reference(h => h.User)
                    .LoadAsync();
            }

            return changeRequest;
        }

        public async Task<ChangeRequest> CreateChangeRequestAsync(ChangeRequest changeRequest, int userId)
        {
            // Generate a unique change request number
            changeRequest.ChangeRequestNumber = GenerateChangeRequestNumber();
            changeRequest.CreatedById = userId;
            changeRequest.CreatedAt = DateTime.UtcNow;
            changeRequest.Status = ChangeRequestStatus.Draft;

            _context.ChangeRequests.Add(changeRequest);
            await _context.SaveChangesAsync();

            // Add history entry
            await AddHistoryEntryAsync(changeRequest.Id, userId, "Created", "Change request created", null, SerializeObject(changeRequest));

            return changeRequest;
        }

        public async Task<ChangeRequest> UpdateChangeRequestAsync(ChangeRequest changeRequest, int userId)
        {
            var existingChangeRequest = await _context.ChangeRequests.FindAsync(changeRequest.Id);
            if (existingChangeRequest == null)
            {
                throw new KeyNotFoundException($"Change request with ID {changeRequest.Id} not found");
            }

            // Store previous state for history
            var previousState = SerializeObject(existingChangeRequest);

            // Update only allowed fields based on current status
            if (existingChangeRequest.Status == ChangeRequestStatus.Draft)
            {
                existingChangeRequest.Title = changeRequest.Title;
                existingChangeRequest.Description = changeRequest.Description;
                existingChangeRequest.Justification = changeRequest.Justification;
                existingChangeRequest.RiskAssessment = changeRequest.RiskAssessment;
                existingChangeRequest.BackoutPlan = changeRequest.BackoutPlan;
                existingChangeRequest.Priority = changeRequest.Priority;
                existingChangeRequest.Type = changeRequest.Type;
                existingChangeRequest.Impact = changeRequest.Impact;
                existingChangeRequest.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Add history entry
                await AddHistoryEntryAsync(existingChangeRequest.Id, userId, "Updated", "Change request updated", previousState, SerializeObject(existingChangeRequest));
            }
            else
            {
                throw new InvalidOperationException($"Cannot update change request in status {existingChangeRequest.Status}");
            }

            return existingChangeRequest;
        }

        public async Task<bool> DeleteChangeRequestAsync(int id)
        {
            var changeRequest = await _context.ChangeRequests.FindAsync(id);
            if (changeRequest == null)
            {
                return false;
            }

            // Only allow deletion of draft change requests
            if (changeRequest.Status != ChangeRequestStatus.Draft)
            {
                throw new InvalidOperationException($"Cannot delete change request in status {changeRequest.Status}");
            }

            _context.ChangeRequests.Remove(changeRequest);
            await _context.SaveChangesAsync();
            return true;
        }

        // Workflow Operations
        public async Task<ChangeRequest> SubmitForSupervisorApprovalAsync(int changeRequestId, int userId)
        {
            var changeRequest = await GetChangeRequestByIdAsync(changeRequestId);
            if (changeRequest == null)
            {
                throw new KeyNotFoundException($"Change request with ID {changeRequestId} not found");
            }

            // Verify the change request is in draft status
            if (changeRequest.Status != ChangeRequestStatus.Draft)
            {
                throw new InvalidOperationException($"Cannot submit change request in status {changeRequest.Status} for supervisor approval");
            }

            // Verify the user is the creator of the change request
            if (changeRequest.CreatedById != userId)
            {
                throw new UnauthorizedAccessException("Only the creator can submit the change request for approval");
            }

            // Store previous state for history
            var previousState = SerializeObject(changeRequest);

            // Update status
            changeRequest.Status = ChangeRequestStatus.SubmittedForSupervisorApproval;
            changeRequest.UpdatedAt = DateTime.UtcNow;

            // Get the supervisor of the user
            var user = await _context.Users
                .Include(u => u.Supervisor)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.SupervisorId == null)
            {
                throw new InvalidOperationException("User does not have a supervisor assigned");
            }

            // Create approval request for supervisor
            var approval = new ChangeRequestApproval
            {
                ChangeRequestId = changeRequestId,
                ApproverId = user.SupervisorId.Value,
                Type = ApprovalType.Supervisor,
                Status = ApprovalStatus.Pending,
                RequestedAt = DateTime.UtcNow
            };

            _context.ChangeRequestApprovals.Add(approval);
            await _context.SaveChangesAsync();

            // Add history entry
            await AddHistoryEntryAsync(changeRequestId, userId, "Submitted", "Change request submitted for supervisor approval", previousState, SerializeObject(changeRequest));

            return changeRequest;
        }

        public async Task<ChangeRequest> ApproveBySupervisorAsync(int changeRequestId, int supervisorId, string? comments)
        {
            var changeRequest = await GetChangeRequestByIdAsync(changeRequestId);
            if (changeRequest == null)
            {
                throw new KeyNotFoundException($"Change request with ID {changeRequestId} not found");
            }

            // Verify the change request is in the correct status
            if (changeRequest.Status != ChangeRequestStatus.SubmittedForSupervisorApproval)
            {
                throw new InvalidOperationException($"Cannot approve change request in status {changeRequest.Status}");
            }

            // Find the pending supervisor approval
            var approval = await _context.ChangeRequestApprovals
                .FirstOrDefaultAsync(a => a.ChangeRequestId == changeRequestId && 
                                         a.ApproverId == supervisorId && 
                                         a.Type == ApprovalType.Supervisor && 
                                         a.Status == ApprovalStatus.Pending);

            if (approval == null)
            {
                throw new InvalidOperationException("No pending supervisor approval found for this change request");
            }

            // Store previous state for history
            var previousState = SerializeObject(changeRequest);

            // Update approval
            approval.Status = ApprovalStatus.Approved;
            approval.Comments = comments;
            approval.ActionedAt = DateTime.UtcNow;

            // Update change request status
            changeRequest.Status = ChangeRequestStatus.SupervisorApproved;
            changeRequest.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Add history entry
            await AddHistoryEntryAsync(changeRequestId, supervisorId, "Approved", "Change request approved by supervisor", previousState, SerializeObject(changeRequest));

            return changeRequest;
        }

        public async Task<ChangeRequest> RejectBySupervisorAsync(int changeRequestId, int supervisorId, string? comments)
        {
            var changeRequest = await GetChangeRequestByIdAsync(changeRequestId);
            if (changeRequest == null)
            {
                throw new KeyNotFoundException($"Change request with ID {changeRequestId} not found");
            }

            // Verify the change request is in the correct status
            if (changeRequest.Status != ChangeRequestStatus.SubmittedForSupervisorApproval)
            {
                throw new InvalidOperationException($"Cannot reject change request in status {changeRequest.Status}");
            }

            // Find the pending supervisor approval
            var approval = await _context.ChangeRequestApprovals
                .FirstOrDefaultAsync(a => a.ChangeRequestId == changeRequestId && 
                                         a.ApproverId == supervisorId && 
                                         a.Type == ApprovalType.Supervisor && 
                                         a.Status == ApprovalStatus.Pending);

            if (approval == null)
            {
                throw new InvalidOperationException("No pending supervisor approval found for this change request");
            }

            // Store previous state for history
            var previousState = SerializeObject(changeRequest);

            // Update approval
            approval.Status = ApprovalStatus.Rejected;
            approval.Comments = comments;
            approval.ActionedAt = DateTime.UtcNow;

            // Update change request status
            changeRequest.Status = ChangeRequestStatus.SupervisorRejected;
            changeRequest.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Add history entry
            await AddHistoryEntryAsync(changeRequestId, supervisorId, "Rejected", "Change request rejected by supervisor", previousState, SerializeObject(changeRequest));

            return changeRequest;
        }

        public async Task<ChangeRequest> SubmitForCABApprovalAsync(int changeRequestId, int userId)
        {
            Console.WriteLine($"SubmitForCABApprovalAsync called for change request ID: {changeRequestId} by user ID: {userId}");
            
            // Get the change request
            var changeRequest = await GetChangeRequestByIdAsync(changeRequestId);
            if (changeRequest == null)
            {
                throw new InvalidOperationException($"Change request with ID {changeRequestId} not found");
            }
            
            Console.WriteLine($"Current status of change request {changeRequestId}: {changeRequest.Status}");
            
            // Temporarily comment out this check for debugging
            // if (changeRequest.Status != ChangeRequestStatus.SupervisorApproved)
            // {
            //     throw new InvalidOperationException($"Change request must be in status {ChangeRequestStatus.SupervisorApproved} to be submitted for CAB approval");
            // }
            
            // Update the status
            var previousStatus = changeRequest.Status;
            changeRequest.Status = ChangeRequestStatus.SubmittedForCABApproval;
            
            // Add a history entry
            var previousState = SerializeObject(new { Status = previousStatus });
            var newState = SerializeObject(new { Status = ChangeRequestStatus.SubmittedForCABApproval });
            
            await AddHistoryEntryAsync(
                changeRequestId,
                userId,
                "SubmitForCABApproval",
                "Change request submitted for CAB approval",
                previousState,
                newState
            );
            
            // Save the changes
            await _context.SaveChangesAsync();
            
            Console.WriteLine($"Change request {changeRequestId} status updated from {previousStatus} to {changeRequest.Status}");
            
            return changeRequest;
        }

        public async Task<ChangeRequest> ApproveByCABAsync(int changeRequestId, int cabMemberId, string? comments)
        {
            var changeRequest = await GetChangeRequestByIdAsync(changeRequestId);
            if (changeRequest == null)
            {
                throw new KeyNotFoundException($"Change request with ID {changeRequestId} not found");
            }

            // Verify the change request is in the correct status
            if (changeRequest.Status != ChangeRequestStatus.SubmittedForCABApproval)
            {
                throw new InvalidOperationException($"Cannot approve change request in status {changeRequest.Status}");
            }

            // Verify the user is a CAB member
            var cabMember = await _context.Users.FindAsync(cabMemberId);
            if (cabMember == null || !cabMember.IsCABMember)
            {
                throw new UnauthorizedAccessException("Only CAB members can approve change requests in this stage");
            }

            // Find the pending CAB approval for this member
            var approval = await _context.ChangeRequestApprovals
                .FirstOrDefaultAsync(a => a.ChangeRequestId == changeRequestId && 
                                         a.ApproverId == cabMemberId && 
                                         a.Type == ApprovalType.CAB && 
                                         a.Status == ApprovalStatus.Pending);

            if (approval == null)
            {
                throw new InvalidOperationException("No pending CAB approval found for this change request and CAB member");
            }

            // Store previous state for history
            var previousState = SerializeObject(changeRequest);

            // Update approval
            approval.Status = ApprovalStatus.Approved;
            approval.Comments = comments;
            approval.ActionedAt = DateTime.UtcNow;

            // Check if all CAB members have approved
            var allCabApprovals = await _context.ChangeRequestApprovals
                .Where(a => a.ChangeRequestId == changeRequestId && a.Type == ApprovalType.CAB)
                .ToListAsync();

            var pendingApprovals = allCabApprovals.Count(a => a.Status == ApprovalStatus.Pending);
            
            // If this was the last pending approval, update the change request status
            if (pendingApprovals == 1) // This one is being approved now
            {
                changeRequest.Status = ChangeRequestStatus.CABApproved;
                changeRequest.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // Add history entry
            await AddHistoryEntryAsync(changeRequestId, cabMemberId, "Approved", "Change request approved by CAB member", previousState, SerializeObject(changeRequest));

            return changeRequest;
        }

        public async Task<ChangeRequest> RejectByCABAsync(int changeRequestId, int cabMemberId, string? comments)
        {
            var changeRequest = await GetChangeRequestByIdAsync(changeRequestId);
            if (changeRequest == null)
            {
                throw new KeyNotFoundException($"Change request with ID {changeRequestId} not found");
            }

            // Verify the change request is in the correct status
            if (changeRequest.Status != ChangeRequestStatus.SubmittedForCABApproval)
            {
                throw new InvalidOperationException($"Cannot reject change request in status {changeRequest.Status}");
            }

            // Verify the user is a CAB member
            var cabMember = await _context.Users.FindAsync(cabMemberId);
            if (cabMember == null || !cabMember.IsCABMember)
            {
                throw new UnauthorizedAccessException("Only CAB members can reject change requests in this stage");
            }

            // Find the pending CAB approval for this member
            var approval = await _context.ChangeRequestApprovals
                .FirstOrDefaultAsync(a => a.ChangeRequestId == changeRequestId && 
                                         a.ApproverId == cabMemberId && 
                                         a.Type == ApprovalType.CAB && 
                                         a.Status == ApprovalStatus.Pending);

            if (approval == null)
            {
                throw new InvalidOperationException("No pending CAB approval found for this change request and CAB member");
            }

            // Store previous state for history
            var previousState = SerializeObject(changeRequest);

            // Update approval
            approval.Status = ApprovalStatus.Rejected;
            approval.Comments = comments;
            approval.ActionedAt = DateTime.UtcNow;

            // Update change request status - any rejection means the whole request is rejected
            changeRequest.Status = ChangeRequestStatus.CABRejected;
            changeRequest.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Add history entry
            await AddHistoryEntryAsync(changeRequestId, cabMemberId, "Rejected", "Change request rejected by CAB member", previousState, SerializeObject(changeRequest));

            return changeRequest;
        }

        public async Task<ChangeRequest> ScheduleChangeRequestAsync(int changeRequestId, int userId, DateTime startDate, DateTime endDate)
        {
            var changeRequest = await GetChangeRequestByIdAsync(changeRequestId);
            if (changeRequest == null)
            {
                throw new KeyNotFoundException($"Change request with ID {changeRequestId} not found");
            }

            // Verify the change request is in the correct status
            // Allow both CABApproved and already Scheduled change requests
            if (changeRequest.Status != ChangeRequestStatus.CABApproved && 
                changeRequest.Status != ChangeRequestStatus.Scheduled &&
                changeRequest.Status != ChangeRequestStatus.Rescheduled)
            {
                throw new InvalidOperationException($"Cannot schedule change request in status {changeRequest.Status}");
            }

            // Verify the user is a CAB member
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.IsCABMember)
            {
                throw new UnauthorizedAccessException("Only CAB members can schedule change requests");
            }

            // Validate dates - allow equal dates but not start > end
            if (startDate > endDate)
            {
                throw new ArgumentException("Start date cannot be after end date");
            }

            if (startDate < DateTime.UtcNow)
            {
                throw new ArgumentException("Start date cannot be in the past");
            }

            // Store previous state for history
            var previousState = SerializeObject(changeRequest);
            
            // Determine if this is a reschedule or initial schedule
            bool isReschedule = changeRequest.Status == ChangeRequestStatus.Scheduled || changeRequest.Status == ChangeRequestStatus.Rescheduled;
            string actionType = isReschedule ? "Rescheduled" : "Scheduled";

            // Update change request
            changeRequest.Status = isReschedule ? ChangeRequestStatus.Rescheduled : ChangeRequestStatus.Scheduled;
            changeRequest.ScheduledStartDate = startDate;
            changeRequest.ScheduledEndDate = endDate;
            changeRequest.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Add history entry
            string description = $"Change request {actionType.ToLower()} from {startDate:g} to {endDate:g}";
            
            await AddHistoryEntryAsync(
                changeRequestId, 
                userId, 
                actionType, 
                description, 
                previousState, 
                SerializeObject(changeRequest)
            );

            return changeRequest;
        }

        public async Task<ChangeRequest> AssignSupportPersonnelAsync(int changeRequestId, int userId, int assigneeId, string role, string? notes)
        {
            var changeRequest = await GetChangeRequestByIdAsync(changeRequestId);
            if (changeRequest == null)
            {
                throw new KeyNotFoundException($"Change request with ID {changeRequestId} not found");
            }

            // Verify the change request is in the correct status
            if (changeRequest.Status != ChangeRequestStatus.Scheduled && 
                changeRequest.Status != ChangeRequestStatus.InProgress &&
                changeRequest.Status != ChangeRequestStatus.CABApproved)
            {
                throw new InvalidOperationException($"Cannot assign support personnel to change request in status {changeRequest.Status}");
            }

            // Verify the user is a CAB member
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.IsCABMember)
            {
                throw new UnauthorizedAccessException("Only CAB members can assign support personnel");
            }

            // Verify the assignee is support personnel
            var assignee = await _context.Users.FindAsync(assigneeId);
            if (assignee == null || !assignee.IsSupportPersonnel)
            {
                throw new ArgumentException("Assignee must be support personnel");
            }

            // Store previous state for history
            var previousState = SerializeObject(changeRequest);

            // Create assignment
            var assignment = new ChangeRequestAssignment
            {
                ChangeRequestId = changeRequestId,
                AssigneeId = assigneeId,
                Role = role,
                Notes = notes,
                Status = AssignmentStatus.Assigned,
                AssignedAt = DateTime.UtcNow
            };

            _context.ChangeRequestAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            // Add history entry
            await AddHistoryEntryAsync(
                changeRequestId, 
                userId, 
                "Assigned", 
                $"Support personnel {assignee.FullName} assigned with role: {role}", 
                previousState, 
                SerializeObject(changeRequest)
            );

            return changeRequest;
        }

        public async Task<ChangeRequest> StartImplementationAsync(int changeRequestId, int userId)
        {
            var changeRequest = await GetChangeRequestByIdAsync(changeRequestId);
            if (changeRequest == null)
            {
                throw new KeyNotFoundException($"Change request with ID {changeRequestId} not found");
            }

            // Verify the change request is in the correct status
            if (changeRequest.Status != ChangeRequestStatus.Scheduled)
            {
                throw new InvalidOperationException($"Cannot start implementation for change request in status {changeRequest.Status}");
            }

            // Verify the user is assigned to this change request
            var isAssigned = await _context.ChangeRequestAssignments
                .AnyAsync(a => a.ChangeRequestId == changeRequestId && 
                              a.AssigneeId == userId && 
                              a.Status == AssignmentStatus.Assigned);

            if (!isAssigned)
            {
                throw new UnauthorizedAccessException("Only assigned support personnel can start implementation");
            }

            // Store previous state for history
            var previousState = SerializeObject(changeRequest);

            // Update change request
            changeRequest.Status = ChangeRequestStatus.InProgress;
            changeRequest.UpdatedAt = DateTime.UtcNow;

            // Update all assignments for this user to InProgress
            var assignments = await _context.ChangeRequestAssignments
                .Where(a => a.ChangeRequestId == changeRequestId && 
                           a.AssigneeId == userId && 
                           a.Status == AssignmentStatus.Assigned)
                .ToListAsync();

            foreach (var assignment in assignments)
            {
                assignment.Status = AssignmentStatus.InProgress;
            }

            await _context.SaveChangesAsync();

            // Add history entry
            await AddHistoryEntryAsync(
                changeRequestId, 
                userId, 
                "Started", 
                "Implementation started", 
                previousState, 
                SerializeObject(changeRequest)
            );

            return changeRequest;
        }

        public async Task<ChangeRequest> CompleteImplementationAsync(int changeRequestId, int userId)
        {
            var changeRequest = await GetChangeRequestByIdAsync(changeRequestId);
            if (changeRequest == null)
            {
                throw new KeyNotFoundException($"Change request with ID {changeRequestId} not found");
            }

            // Verify the change request is in the correct status
            if (changeRequest.Status != ChangeRequestStatus.InProgress && changeRequest.Status != ChangeRequestStatus.Scheduled)
            {
                throw new InvalidOperationException($"Cannot complete implementation for change request in status {changeRequest.Status}");
            }

            // Check if the user is a CAB member
            var user = await _context.Users.FindAsync(userId);
            bool isCABMember = user != null && user.IsCABMember;

            // If not a CAB member, verify the user is assigned to this change request
            if (!isCABMember)
            {
                var isAssigned = await _context.ChangeRequestAssignments
                    .AnyAsync(a => a.ChangeRequestId == changeRequestId && 
                                a.AssigneeId == userId && 
                                a.Status == AssignmentStatus.InProgress);

                if (!isAssigned)
                {
                    throw new UnauthorizedAccessException("Only assigned support personnel or CAB members can complete implementation");
                }
            }

            // Store previous state for history
            var previousState = SerializeObject(changeRequest);

            // Update change request
            changeRequest.Status = ChangeRequestStatus.Completed;
            changeRequest.UpdatedAt = DateTime.UtcNow;
            changeRequest.ImplementedAt = DateTime.UtcNow;

            // Update all assignments for this change request to Completed
            var assignments = await _context.ChangeRequestAssignments
                .Where(a => a.ChangeRequestId == changeRequestId)
                .ToListAsync();

            foreach (var assignment in assignments)
            {
                assignment.Status = AssignmentStatus.Completed;
            }

            await _context.SaveChangesAsync();

            // Add history entry
            await AddHistoryEntryAsync(
                changeRequestId, 
                userId, 
                "Completed", 
                "Implementation completed successfully", 
                previousState, 
                SerializeObject(changeRequest)
            );

            return changeRequest;
        }

        public async Task<ChangeRequest> FailImplementationAsync(int changeRequestId, int userId, string reason)
        {
            var changeRequest = await GetChangeRequestByIdAsync(changeRequestId);
            if (changeRequest == null)
            {
                throw new KeyNotFoundException($"Change request with ID {changeRequestId} not found");
            }

            // Verify the change request is in the correct status
            if (changeRequest.Status != ChangeRequestStatus.InProgress)
            {
                throw new InvalidOperationException($"Cannot fail implementation for change request in status {changeRequest.Status}");
            }

            // Verify the user is assigned to this change request
            var isAssigned = await _context.ChangeRequestAssignments
                .AnyAsync(a => a.ChangeRequestId == changeRequestId && 
                              a.AssigneeId == userId && 
                              a.Status == AssignmentStatus.InProgress);

            if (!isAssigned)
            {
                throw new UnauthorizedAccessException("Only assigned support personnel can fail implementation");
            }

            // Store previous state for history
            var previousState = SerializeObject(changeRequest);

            // Update change request
            changeRequest.Status = ChangeRequestStatus.Failed;
            changeRequest.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Add comment with failure reason
            await AddCommentAsync(changeRequestId, userId, $"Implementation failed: {reason}", false);

            // Add history entry
            await AddHistoryEntryAsync(
                changeRequestId, 
                userId, 
                "Failed", 
                $"Implementation failed: {reason}", 
                previousState, 
                SerializeObject(changeRequest)
            );

            return changeRequest;
        }

        public async Task<ChangeRequest> CancelChangeRequestAsync(int changeRequestId, int userId, string reason)
        {
            var changeRequest = await GetChangeRequestByIdAsync(changeRequestId);
            if (changeRequest == null)
            {
                throw new KeyNotFoundException($"Change request with ID {changeRequestId} not found");
            }

            // Cannot cancel completed or failed change requests
            if (changeRequest.Status == ChangeRequestStatus.Completed || 
                changeRequest.Status == ChangeRequestStatus.Failed)
            {
                throw new InvalidOperationException($"Cannot cancel change request in status {changeRequest.Status}");
            }

            // Verify the user is authorized (creator, supervisor, or CAB member)
            var user = await _context.Users.FindAsync(userId);
            bool isAuthorized = user != null && (
                changeRequest.CreatedById == userId || 
                user.IsCABMember || 
                await _context.Users.AnyAsync(u => u.Id == changeRequest.CreatedById && u.SupervisorId == userId)
            );

            if (!isAuthorized)
            {
                throw new UnauthorizedAccessException("Only the creator, supervisor, or CAB members can cancel a change request");
            }

            // Store previous state for history
            var previousState = SerializeObject(changeRequest);

            // Update change request
            changeRequest.Status = ChangeRequestStatus.Cancelled;
            changeRequest.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Add comment with cancellation reason
            await AddCommentAsync(changeRequestId, userId, $"Change request cancelled: {reason}", false);

            // Add history entry
            await AddHistoryEntryAsync(
                changeRequestId, 
                userId, 
                "Cancelled", 
                $"Change request cancelled: {reason}", 
                previousState, 
                SerializeObject(changeRequest)
            );

            return changeRequest;
        }

        // Comment Operations
        public async Task<ChangeRequestComment> AddCommentAsync(int changeRequestId, int userId, string text, bool isInternal = false)
        {
            var changeRequest = await _context.ChangeRequests.FindAsync(changeRequestId);
            if (changeRequest == null)
            {
                throw new KeyNotFoundException($"Change request with ID {changeRequestId} not found");
            }

            // If internal comment, verify the user is a CAB member
            if (isInternal)
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null || !user.IsCABMember)
                {
                    throw new UnauthorizedAccessException("Only CAB members can add internal comments");
                }
            }

            var comment = new ChangeRequestComment
            {
                ChangeRequestId = changeRequestId,
                CommenterId = userId,
                Text = text,
                IsInternal = isInternal,
                CreatedAt = DateTime.UtcNow
            };

            _context.ChangeRequestComments.Add(comment);
            await _context.SaveChangesAsync();

            // Add history entry
            await AddHistoryEntryAsync(
                changeRequestId,
                userId,
                "CommentAdded",
                isInternal ? "Internal comment added" : "Comment added",
                null,
                null
            );

            return comment;
        }

        public async Task<IEnumerable<ChangeRequestComment>> GetCommentsForChangeRequestAsync(int changeRequestId, bool includeInternal = false)
        {
            var query = _context.ChangeRequestComments
                .Include(c => c.Commenter)
                .Where(c => c.ChangeRequestId == changeRequestId);

            if (!includeInternal)
            {
                query = query.Where(c => !c.IsInternal);
            }

            return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        }

        // History Operations
        public async Task<IEnumerable<ChangeRequestHistory>> GetHistoryForChangeRequestAsync(int changeRequestId)
        {
            return await _context.ChangeRequestHistory
                .Include(h => h.User)
                .Where(h => h.ChangeRequestId == changeRequestId)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();
        }

        // User-specific Operations
        public async Task<IEnumerable<ChangeRequest>> GetChangeRequestsCreatedByUserAsync(int userId)
        {
            return await _context.ChangeRequests
                .Include(cr => cr.CreatedBy)
                .Where(cr => cr.CreatedById == userId)
                .OrderByDescending(cr => cr.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ChangeRequest>> GetChangeRequestsAssignedToUserAsync(int userId)
        {
            var assignments = await _context.ChangeRequestAssignments
                .Include(a => a.ChangeRequest)
                    .ThenInclude(cr => cr.CreatedBy)
                .Where(a => a.AssigneeId == userId)
                .ToListAsync();

            return assignments
                .Select(a => a.ChangeRequest)
                .Where(cr => cr != null)
                .Distinct()
                .OrderByDescending(cr => cr.UpdatedAt ?? cr.CreatedAt)
                .ToList();
        }

        public async Task<IEnumerable<ChangeRequest>> GetChangeRequestsPendingUserApprovalAsync(int userId)
        {
            var approvals = await _context.ChangeRequestApprovals
                .Include(a => a.ChangeRequest)
                    .ThenInclude(cr => cr.CreatedBy)
                .Where(a => a.ApproverId == userId && a.Status == ApprovalStatus.Pending)
                .ToListAsync();

            return approvals
                .Select(a => a.ChangeRequest)
                .Where(cr => cr != null)
                .Distinct()
                .OrderByDescending(cr => cr.UpdatedAt ?? cr.CreatedAt)
                .ToList();
        }

        public async Task<IEnumerable<ChangeRequest>> GetPendingApprovalsForUserAsync(int userId)
        {
            // Get user information to determine their role
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return Enumerable.Empty<ChangeRequest>();
            }

            // Get user roles from the database or from the user object
            List<string> userRoles = new List<string>();
            if (user.Roles != null)
            {
                // If Roles is a string property that contains comma-separated roles
                userRoles = user.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            // Check if user is a CAB member
            bool isCABMember = user.IsCABMember || userRoles.Contains("CABMember");
            
            // Check if user is a manager/supervisor
            bool isManager = userRoles.Contains("Manager") || userRoles.Contains("Admin");

            // Get change requests pending approval based on user's role
            var query = _context.ChangeRequestApprovals
                .Include(a => a.ChangeRequest)
                    .ThenInclude(cr => cr.CreatedBy)
                .Where(a => a.Status == ApprovalStatus.Pending);

            if (isCABMember && isManager)
            {
                // Both CAB member and manager can see all pending approvals
                query = query.Where(a => 
                    a.ApproverId == userId || 
                    a.Type == ApprovalType.CAB || 
                    a.Type == ApprovalType.Supervisor);
            }
            else if (isCABMember)
            {
                // CAB members see CAB approvals and their own approvals
                query = query.Where(a => 
                    a.ApproverId == userId || 
                    a.Type == ApprovalType.CAB);
            }
            else if (isManager)
            {
                // Managers see supervisor approvals and their own approvals
                query = query.Where(a => 
                    a.ApproverId == userId || 
                    a.Type == ApprovalType.Supervisor);
            }
            else
            {
                // Regular users only see their own approvals
                query = query.Where(a => a.ApproverId == userId);
            }

            var approvals = await query.ToListAsync();

            return approvals
                .Select(a => a.ChangeRequest)
                .Where(cr => cr != null)
                .Distinct()
                .OrderByDescending(cr => cr.UpdatedAt ?? cr.CreatedAt)
                .ToList();
        }

        public async Task<IEnumerable<ChangeRequest>> GetChangeRequestsForCABAsync()
        {
            Console.WriteLine("GetChangeRequestsForCABAsync called");
            
            // Get all change requests that are either:
            // 1. Supervisor approved (waiting to be submitted to CAB)
            // 2. Submitted for CAB approval
            // 3. Scheduled or Rescheduled
            // 4. Recently approved by CAB (within the last 7 days)
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
            
            var requests = await _context.ChangeRequests
                .Include(cr => cr.CreatedBy)
                .Include(cr => cr.Approvals)
                .Include(cr => cr.Assignments)
                .Where(cr => 
                    (cr.Status == ChangeRequestStatus.SupervisorApproved) ||
                    (cr.Status == ChangeRequestStatus.SubmittedForCABApproval) ||
                    (cr.Status == ChangeRequestStatus.Scheduled) ||
                    (cr.Status == ChangeRequestStatus.Rescheduled) ||
                    (cr.Status == ChangeRequestStatus.CABApproved &&
                        cr.UpdatedAt >= sevenDaysAgo)
                )
                .OrderByDescending(cr => cr.UpdatedAt)
                .ToListAsync();
            
            Console.WriteLine($"Retrieved {requests.Count} total change requests from database");
            Console.WriteLine($"SupervisorApproved: {requests.Count(cr => cr.Status == ChangeRequestStatus.SupervisorApproved)}");
            Console.WriteLine($"SubmittedForCABApproval: {requests.Count(cr => cr.Status == ChangeRequestStatus.SubmittedForCABApproval)}");
            Console.WriteLine($"Scheduled: {requests.Count(cr => cr.Status == ChangeRequestStatus.Scheduled)}");
            Console.WriteLine($"Rescheduled: {requests.Count(cr => cr.Status == ChangeRequestStatus.Rescheduled)}");
            Console.WriteLine($"CABApproved (recent): {requests.Count(cr => cr.Status == ChangeRequestStatus.CABApproved)}");
            
            Console.WriteLine($"Retrieved {requests.Count} change requests for CAB Dashboard");
            foreach (var cr in requests)
            {
                Console.WriteLine($"Change Request ID: {cr.Id}, Status: {cr.Status}, Title: {cr.Title}");
            }
            
            Console.WriteLine($"Found {requests.Count(cr => cr.Status == ChangeRequestStatus.SubmittedForCABApproval || cr.Status == ChangeRequestStatus.SupervisorApproved)} change requests pending CAB approval");
            
            return requests;
        }

        public async Task<ChangeRequest> DirectScheduleChangeRequestAsync(int changeRequestId, int userId, DateTime startDate, DateTime endDate)
        {
            Console.WriteLine($"DirectScheduleChangeRequestAsync called for change request {changeRequestId}");
            
            // Get the change request
            var changeRequest = await _context.ChangeRequests
                .FirstOrDefaultAsync(cr => cr.Id == changeRequestId);
        
            if (changeRequest == null)
            {
                throw new KeyNotFoundException($"Change request with ID {changeRequestId} not found");
            }
        
            Console.WriteLine($"Retrieved change request {changeRequestId}. Current status: {changeRequest.Status}");
        
            // Store the previous state for history
            var previousState = SerializeObject(changeRequest);
        
            // Update the change request status and scheduled dates
            Console.WriteLine($"Scheduling change request {changeRequestId} from {startDate:g} to {endDate:g}");
            changeRequest.Status = ChangeRequestStatus.Scheduled;
            changeRequest.ScheduledStartDate = startDate;
            changeRequest.ScheduledEndDate = endDate;
            changeRequest.UpdatedAt = DateTime.UtcNow;
        
            try
            {
                await _context.SaveChangesAsync();
                Console.WriteLine($"Change request {changeRequestId} scheduled successfully. New status: {changeRequest.Status}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving changes: {ex.Message}");
                throw;
            }
        
            // Add history entry
            await AddHistoryEntryAsync(
                changeRequestId,
                userId,
                "Scheduled",
                $"Change request scheduled from {startDate:g} to {endDate:g}",
                previousState,
                SerializeObject(changeRequest)
            );
        
            return changeRequest;
        }

        // Helper methods
        private string GenerateChangeRequestNumber()
        {
            // Format: CR-YYYY-NNNNN
            var year = DateTime.UtcNow.Year;
            var count = _context.ChangeRequests.Count() + 1;
            return $"CR-{year}-{count:D5}";
        }

        private string? SerializeObject(object obj)
        {
            return JsonSerializer.Serialize(obj, new JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            });
        }

        private async Task<ChangeRequestHistory> AddHistoryEntryAsync(int changeRequestId, int userId, string actionType, string description, string? previousState, string? newState)
        {
            var historyEntry = new ChangeRequestHistory
            {
                ChangeRequestId = changeRequestId,
                UserId = userId,
                ActionType = actionType,
                Description = description,
                PreviousState = previousState,
                NewState = newState,
                CreatedAt = DateTime.UtcNow
            };

            _context.ChangeRequestHistory.Add(historyEntry);
            await _context.SaveChangesAsync();
            return historyEntry;
        }
    }
}
