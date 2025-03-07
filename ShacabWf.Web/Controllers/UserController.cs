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
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;

        public UserController(ApplicationDbContext context, IAuthService authService)
        {
            _context = context;
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

        // GET: api/User
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/User/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _authService.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // GET: api/User/CABMembers
        [HttpGet("CABMembers")]
        public async Task<ActionResult<IEnumerable<User>>> GetCABMembers()
        {
            return await _context.Users
                .Where(u => u.IsCABMember)
                .ToListAsync();
        }

        // GET: api/User/SupportPersonnel
        [HttpGet("SupportPersonnel")]
        public async Task<ActionResult<IEnumerable<User>>> GetSupportPersonnel()
        {
            return await _context.Users
                .Where(u => u.IsSupportPersonnel)
                .ToListAsync();
        }

        // GET: api/User/Supervisors
        [HttpGet("Supervisors")]
        public async Task<ActionResult<IEnumerable<User>>> GetSupervisors()
        {
            // Get all users who are supervisors (have direct reports)
            var supervisorIds = await _context.Users
                .Where(u => u.SupervisorId != null)
                .Select(u => u.SupervisorId)
                .Distinct()
                .ToListAsync();

            return await _context.Users
                .Where(u => supervisorIds.Contains(u.Id))
                .ToListAsync();
        }

        // GET: api/User/5/DirectReports
        [HttpGet("{id}/DirectReports")]
        public async Task<ActionResult<IEnumerable<User>>> GetDirectReports(int id)
        {
            var user = await _authService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return await _context.Users
                .Where(u => u.SupervisorId == id)
                .ToListAsync();
        }

        // GET: api/User/Current
        [HttpGet("Current")]
        public async Task<ActionResult<User>> GetCurrentUser()
        {
            try
            {
                int userId = GetCurrentUserId();
                var user = await _authService.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return NotFound("User not found");
                }

                return user;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
} 