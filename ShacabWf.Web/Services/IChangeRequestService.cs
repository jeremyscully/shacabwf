using ShacabWf.Web.Models;

namespace ShacabWf.Web.Services
{
    /// <summary>
    /// Interface for change request service
    /// </summary>
    public interface IChangeRequestService
    {
        // Change Request CRUD operations
        Task<IEnumerable<ChangeRequest>> GetAllChangeRequestsAsync();
        Task<ChangeRequest?> GetChangeRequestByIdAsync(int id);
        Task<ChangeRequest> CreateChangeRequestAsync(ChangeRequest changeRequest, int userId);
        Task<ChangeRequest> UpdateChangeRequestAsync(ChangeRequest changeRequest, int userId);
        Task<bool> DeleteChangeRequestAsync(int id);

        // Workflow operations
        Task<ChangeRequest> SubmitForSupervisorApprovalAsync(int changeRequestId, int userId);
        Task<ChangeRequest> ApproveBySupervisorAsync(int changeRequestId, int supervisorId, string? comments);
        Task<ChangeRequest> RejectBySupervisorAsync(int changeRequestId, int supervisorId, string? comments);
        Task<ChangeRequest> SubmitForCABApprovalAsync(int changeRequestId, int userId);
        Task<ChangeRequest> ApproveByCABAsync(int changeRequestId, int cabMemberId, string? comments);
        Task<ChangeRequest> RejectByCABAsync(int changeRequestId, int cabMemberId, string? comments);
        Task<ChangeRequest> ScheduleChangeRequestAsync(int changeRequestId, int userId, DateTime startDate, DateTime endDate);
        Task<ChangeRequest> AssignSupportPersonnelAsync(int changeRequestId, int userId, int assigneeId, string role, string? notes);
        Task<ChangeRequest> StartImplementationAsync(int changeRequestId, int userId);
        Task<ChangeRequest> CompleteImplementationAsync(int changeRequestId, int userId);
        Task<ChangeRequest> FailImplementationAsync(int changeRequestId, int userId, string reason);
        Task<ChangeRequest> CancelChangeRequestAsync(int changeRequestId, int userId, string reason);

        // Comment operations
        Task<ChangeRequestComment> AddCommentAsync(int changeRequestId, int userId, string text, bool isInternal = false);
        Task<IEnumerable<ChangeRequestComment>> GetCommentsForChangeRequestAsync(int changeRequestId, bool includeInternal = false);

        // History operations
        Task<IEnumerable<ChangeRequestHistory>> GetHistoryForChangeRequestAsync(int changeRequestId);

        // User-specific operations
        Task<IEnumerable<ChangeRequest>> GetChangeRequestsCreatedByUserAsync(int userId);
        Task<IEnumerable<ChangeRequest>> GetChangeRequestsAssignedToUserAsync(int userId);
        Task<IEnumerable<ChangeRequest>> GetChangeRequestsPendingUserApprovalAsync(int userId);
        
        // Additional methods for dashboard functionality
        Task<IEnumerable<ChangeRequest>> GetPendingApprovalsForUserAsync(int userId);
        Task<IEnumerable<ChangeRequest>> GetChangeRequestsForCABAsync();
    }
} 