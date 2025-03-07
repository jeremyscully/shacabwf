using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShacabWf.Web.Data;
using System.Threading.Tasks;

namespace ShacabWf.Web.Controllers
{
    /// <summary>
    /// Controller for seeding test data
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class TestDataController : ControllerBase
    {
        private readonly TestDataSeeder _testDataSeeder;

        public TestDataController(TestDataSeeder testDataSeeder)
        {
            _testDataSeeder = testDataSeeder;
        }

        /// <summary>
        /// Seeds test users with known credentials
        /// </summary>
        /// <returns>The number of users created or updated</returns>
        [HttpPost("seed-users")]
        [AllowAnonymous] // Allow anonymous access for initial setup
        public async Task<IActionResult> SeedUsers()
        {
            var count = await _testDataSeeder.SeedTestUsersAsync();
            return Ok(new { message = $"Successfully created or updated {count} test users", count });
        }

        /// <summary>
        /// Seeds test change requests for user1
        /// </summary>
        /// <returns>The number of change requests created</returns>
        [HttpPost("seed-change-requests")]
        public async Task<IActionResult> SeedChangeRequests()
        {
            var count = await _testDataSeeder.SeedChangeRequestsForUser1Async();
            return Ok(new { message = $"Successfully created {count} change requests for user1", count });
        }
    }
} 