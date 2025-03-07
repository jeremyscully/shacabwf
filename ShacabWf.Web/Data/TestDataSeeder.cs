using Microsoft.EntityFrameworkCore;
using ShacabWf.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShacabWf.Web.Data
{
    /// <summary>
    /// Provides methods to seed test data into the database
    /// </summary>
    public class TestDataSeeder
    {
        private readonly ApplicationDbContext _context;

        public TestDataSeeder(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Seeds test users with known credentials
        /// </summary>
        /// <returns>The number of users created or updated</returns>
        public async Task<int> SeedTestUsersAsync()
        {
            int count = 0;

            // Create or update admin user
            var admin = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
            if (admin == null)
            {
                admin = new User
                {
                    Username = "admin",
                    Email = "admin@example.com",
                    Password = "Admin123!",
                    FirstName = "Admin",
                    LastName = "User",
                    Department = "IT",
                    IsCABMember = true,
                    IsSupportPersonnel = true,
                    Roles = "User,Admin,CABMember,Support"
                };
                _context.Users.Add(admin);
                count++;
            }
            else
            {
                admin.Password = "Admin123!";
                admin.IsCABMember = true;
                admin.IsSupportPersonnel = true;
                admin.Roles = "User,Admin,CABMember,Support";
                _context.Users.Update(admin);
                count++;
            }

            // Create or update manager user
            var manager = await _context.Users.FirstOrDefaultAsync(u => u.Username == "manager");
            if (manager == null)
            {
                manager = new User
                {
                    Username = "manager",
                    Email = "manager@example.com",
                    Password = "Manager123!",
                    FirstName = "Manager",
                    LastName = "User",
                    Department = "Management",
                    IsCABMember = true,
                    IsSupportPersonnel = false,
                    Roles = "User,CABMember"
                };
                _context.Users.Add(manager);
                count++;
            }
            else
            {
                manager.Password = "Manager123!";
                manager.IsCABMember = true;
                manager.IsSupportPersonnel = false;
                manager.Roles = "User,CABMember";
                _context.Users.Update(manager);
                count++;
            }

            // Create or update user1
            var user1 = await _context.Users.FirstOrDefaultAsync(u => u.Username == "user1");
            if (user1 == null)
            {
                user1 = new User
                {
                    Username = "user1",
                    Email = "user1@example.com",
                    Password = "User123!",
                    FirstName = "Regular",
                    LastName = "User",
                    Department = "Operations",
                    IsCABMember = false,
                    IsSupportPersonnel = false,
                    Roles = "User"
                };
                _context.Users.Add(user1);
                count++;
            }
            else
            {
                user1.Password = "User123!";
                user1.IsCABMember = false;
                user1.IsSupportPersonnel = false;
                user1.Roles = "User";
                _context.Users.Update(user1);
                count++;
            }

            // Create or update support user
            var support = await _context.Users.FirstOrDefaultAsync(u => u.Username == "support");
            if (support == null)
            {
                support = new User
                {
                    Username = "support",
                    Email = "support@example.com",
                    Password = "Support123!",
                    FirstName = "Support",
                    LastName = "User",
                    Department = "IT Support",
                    IsCABMember = false,
                    IsSupportPersonnel = true,
                    Roles = "User,Support"
                };
                _context.Users.Add(support);
                count++;
            }
            else
            {
                support.Password = "Support123!";
                support.IsCABMember = false;
                support.IsSupportPersonnel = true;
                support.Roles = "User,Support";
                _context.Users.Update(support);
                count++;
            }

            // Set up supervisor relationships
            if (user1 != null && manager != null)
            {
                user1.SupervisorId = manager.Id;
                user1.Supervisor = manager;
            }

            await _context.SaveChangesAsync();
            return count;
        }

        /// <summary>
        /// Seeds test change requests for user1
        /// </summary>
        /// <returns>The number of change requests created</returns>
        public async Task<int> SeedChangeRequestsForUser1Async()
        {
            // Get user1
            var user1 = await _context.Users.FirstOrDefaultAsync(u => u.Username == "user1");
            if (user1 == null)
            {
                throw new InvalidOperationException("User 'user1' not found in the database");
            }

            // Get manager (supervisor)
            var manager = await _context.Users.FirstOrDefaultAsync(u => u.Username == "manager");
            if (manager == null)
            {
                throw new InvalidOperationException("User 'manager' not found in the database");
            }

            // Get admin (CAB member)
            var admin = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
            if (admin == null)
            {
                throw new InvalidOperationException("User 'admin' not found in the database");
            }

            // Get support
            var support = await _context.Users.FirstOrDefaultAsync(u => u.Username == "support");
            if (support == null)
            {
                throw new InvalidOperationException("User 'support' not found in the database");
            }

            // Clear existing change requests for user1
            var existingChangeRequests = await _context.ChangeRequests
                .Where(cr => cr.CreatedById == user1.Id)
                .ToListAsync();

            foreach (var cr in existingChangeRequests)
            {
                // Remove related records
                var history = await _context.ChangeRequestHistory.Where(h => h.ChangeRequestId == cr.Id).ToListAsync();
                var comments = await _context.ChangeRequestComments.Where(c => c.ChangeRequestId == cr.Id).ToListAsync();
                var assignments = await _context.ChangeRequestAssignments.Where(a => a.ChangeRequestId == cr.Id).ToListAsync();
                var approvals = await _context.ChangeRequestApprovals.Where(a => a.ChangeRequestId == cr.Id).ToListAsync();

                _context.ChangeRequestHistory.RemoveRange(history);
                _context.ChangeRequestComments.RemoveRange(comments);
                _context.ChangeRequestAssignments.RemoveRange(assignments);
                _context.ChangeRequestApprovals.RemoveRange(approvals);
            }

            _context.ChangeRequests.RemoveRange(existingChangeRequests);
            await _context.SaveChangesAsync();

            // Date variables
            var today = DateTime.Today;
            var oneMonthFromNow = today.AddMonths(1);
            var twoWeeksFromNow = today.AddDays(14);
            var oneWeekFromNow = today.AddDays(7);
            var threeDaysFromNow = today.AddDays(3);
            var yesterday = today.AddDays(-1);
            var lastWeek = today.AddDays(-7);

            // Create change requests
            var changeRequests = new List<ChangeRequest>
            {
                // 1. Draft change request (created today)
                new ChangeRequest
                {
                    ChangeRequestNumber = $"CR-{today.Year}-00001",
                    Title = "Update Server Configuration",
                    Description = "Update the configuration settings on the application server to improve performance.",
                    Justification = "Current configuration is causing performance issues during peak hours.",
                    RiskAssessment = "Low risk as changes can be reverted if issues arise.",
                    BackoutPlan = "Revert to previous configuration settings if performance degrades.",
                    Status = ChangeRequestStatus.Draft,
                    Priority = ChangeRequestPriority.Medium,
                    Type = ChangeRequestType.Normal,
                    Impact = ChangeRequestImpact.Medium,
                    CreatedAt = today,
                    CreatedById = user1.Id
                },

                // 2. Submitted for supervisor approval (created last week)
                new ChangeRequest
                {
                    ChangeRequestNumber = $"CR-{today.Year}-00002",
                    Title = "Database Index Optimization",
                    Description = "Add new indexes to improve query performance on the customer database.",
                    Justification = "Queries are currently taking too long to execute, affecting user experience.",
                    RiskAssessment = "Medium risk as new indexes may affect write performance.",
                    BackoutPlan = "Remove the new indexes and revert to previous configuration.",
                    Status = ChangeRequestStatus.SubmittedForSupervisorApproval,
                    Priority = ChangeRequestPriority.High,
                    Type = ChangeRequestType.Normal,
                    Impact = ChangeRequestImpact.Medium,
                    CreatedAt = lastWeek,
                    UpdatedAt = threeDaysFromNow,
                    CreatedById = user1.Id
                },

                // 3. Supervisor approved (created last week, scheduled for next week)
                new ChangeRequest
                {
                    ChangeRequestNumber = $"CR-{today.Year}-00003",
                    Title = "Security Patch Installation",
                    Description = "Install the latest security patches on all production servers.",
                    Justification = "Critical security vulnerabilities need to be addressed immediately.",
                    RiskAssessment = "Medium risk as patches may cause compatibility issues with existing software.",
                    BackoutPlan = "Uninstall patches and restore from backup if issues arise.",
                    Status = ChangeRequestStatus.SupervisorApproved,
                    Priority = ChangeRequestPriority.Critical,
                    Type = ChangeRequestType.Normal,
                    Impact = ChangeRequestImpact.High,
                    CreatedAt = lastWeek,
                    UpdatedAt = yesterday,
                    ScheduledStartDate = oneWeekFromNow,
                    ScheduledEndDate = oneWeekFromNow,
                    CreatedById = user1.Id
                },

                // 4. CAB Approved (created 2 weeks ago, scheduled for tomorrow)
                new ChangeRequest
                {
                    ChangeRequestNumber = $"CR-{today.Year}-00004",
                    Title = "Network Firewall Update",
                    Description = "Update firewall rules to enhance security and block recent attack vectors.",
                    Justification = "Recent security audit identified potential vulnerabilities in our network.",
                    RiskAssessment = "Medium risk as rule changes may affect legitimate traffic.",
                    BackoutPlan = "Revert to previous firewall configuration if issues arise.",
                    Status = ChangeRequestStatus.CABApproved,
                    Priority = ChangeRequestPriority.High,
                    Type = ChangeRequestType.Normal,
                    Impact = ChangeRequestImpact.High,
                    CreatedAt = today.AddDays(-14),
                    UpdatedAt = yesterday,
                    ScheduledStartDate = today.AddDays(1),
                    ScheduledEndDate = today.AddDays(1),
                    CreatedById = user1.Id
                },

                // 5. Scheduled (created 3 weeks ago, scheduled for next week)
                new ChangeRequest
                {
                    ChangeRequestNumber = $"CR-{today.Year}-00005",
                    Title = "Application Server Upgrade",
                    Description = "Upgrade application servers to the latest version.",
                    Justification = "Current version will reach end-of-life next quarter.",
                    RiskAssessment = "High risk as this is a major version upgrade.",
                    BackoutPlan = "Rollback to previous version using system backups.",
                    Status = ChangeRequestStatus.Scheduled,
                    Priority = ChangeRequestPriority.Medium,
                    Type = ChangeRequestType.Normal,
                    Impact = ChangeRequestImpact.Medium,
                    CreatedAt = today.AddDays(-21),
                    UpdatedAt = yesterday,
                    ScheduledStartDate = oneWeekFromNow,
                    ScheduledEndDate = today.AddDays(8),
                    CreatedById = user1.Id
                },

                // 6. In Progress (created 2 weeks ago, started yesterday)
                new ChangeRequest
                {
                    ChangeRequestNumber = $"CR-{today.Year}-00006",
                    Title = "Storage Capacity Expansion",
                    Description = "Add additional storage capacity to the data warehouse.",
                    Justification = "Current storage is at 85% capacity and needs to be expanded.",
                    RiskAssessment = "Low risk as this is an additive change.",
                    BackoutPlan = "No backout plan needed as this is an expansion.",
                    Status = ChangeRequestStatus.InProgress,
                    Priority = ChangeRequestPriority.Medium,
                    Type = ChangeRequestType.Normal,
                    Impact = ChangeRequestImpact.Low,
                    CreatedAt = today.AddDays(-14),
                    UpdatedAt = yesterday,
                    ScheduledStartDate = yesterday,
                    ScheduledEndDate = threeDaysFromNow,
                    CreatedById = user1.Id
                },

                // 7. Completed (created 3 weeks ago, completed yesterday)
                new ChangeRequest
                {
                    ChangeRequestNumber = $"CR-{today.Year}-00007",
                    Title = "Email Server Configuration",
                    Description = "Update email server configuration to improve deliverability.",
                    Justification = "Some emails are being marked as spam by recipient servers.",
                    RiskAssessment = "Low risk as changes are incremental and can be reverted.",
                    BackoutPlan = "Revert to previous configuration if deliverability worsens.",
                    Status = ChangeRequestStatus.Completed,
                    Priority = ChangeRequestPriority.Low,
                    Type = ChangeRequestType.Normal,
                    Impact = ChangeRequestImpact.Low,
                    CreatedAt = today.AddDays(-21),
                    UpdatedAt = yesterday,
                    ScheduledStartDate = today.AddDays(-3),
                    ScheduledEndDate = today.AddDays(-2),
                    ImplementedAt = yesterday,
                    CreatedById = user1.Id
                },

                // 8. Failed (created 2 weeks ago, failed yesterday)
                new ChangeRequest
                {
                    ChangeRequestNumber = $"CR-{today.Year}-00008",
                    Title = "Database Migration",
                    Description = "Migrate database to new cloud platform.",
                    Justification = "Current database platform is being deprecated.",
                    RiskAssessment = "High risk due to complexity of migration.",
                    BackoutPlan = "Revert to original database if migration fails.",
                    Status = ChangeRequestStatus.Failed,
                    Priority = ChangeRequestPriority.High,
                    Type = ChangeRequestType.Normal,
                    Impact = ChangeRequestImpact.High,
                    CreatedAt = today.AddDays(-14),
                    UpdatedAt = yesterday,
                    ScheduledStartDate = today.AddDays(-2),
                    ScheduledEndDate = yesterday,
                    CreatedById = user1.Id
                },

                // 9. Emergency change (created yesterday, scheduled for today)
                new ChangeRequest
                {
                    ChangeRequestNumber = $"CR-{today.Year}-00009",
                    Title = "Critical Security Hotfix",
                    Description = "Apply emergency security hotfix to address zero-day vulnerability.",
                    Justification = "Systems are vulnerable to active exploit in the wild.",
                    RiskAssessment = "Medium risk but necessary due to active threats.",
                    BackoutPlan = "Revert patch if it causes critical system issues.",
                    Status = ChangeRequestStatus.CABApproved,
                    Priority = ChangeRequestPriority.Critical,
                    Type = ChangeRequestType.Emergency,
                    Impact = ChangeRequestImpact.High,
                    CreatedAt = yesterday,
                    UpdatedAt = today,
                    ScheduledStartDate = today,
                    ScheduledEndDate = today,
                    CreatedById = user1.Id
                },

                // 10. Future change (created today, scheduled for one month from now)
                new ChangeRequest
                {
                    ChangeRequestNumber = $"CR-{today.Year}-00010",
                    Title = "Annual System Maintenance",
                    Description = "Perform annual system maintenance and upgrades.",
                    Justification = "Regular maintenance required to keep systems running optimally.",
                    RiskAssessment = "Medium risk due to multiple systems being affected.",
                    BackoutPlan = "Each component has its own backout plan documented in the maintenance guide.",
                    Status = ChangeRequestStatus.Scheduled,
                    Priority = ChangeRequestPriority.Medium,
                    Type = ChangeRequestType.Normal,
                    Impact = ChangeRequestImpact.Medium,
                    CreatedAt = today,
                    UpdatedAt = today,
                    ScheduledStartDate = oneMonthFromNow,
                    ScheduledEndDate = oneMonthFromNow.AddDays(2),
                    CreatedById = user1.Id
                }
            };

            // Add change requests to database
            await _context.ChangeRequests.AddRangeAsync(changeRequests);
            await _context.SaveChangesAsync();

            // Add approvals
            var cr3 = await _context.ChangeRequests.FirstOrDefaultAsync(cr => cr.ChangeRequestNumber == $"CR-{today.Year}-00003");
            var cr4 = await _context.ChangeRequests.FirstOrDefaultAsync(cr => cr.ChangeRequestNumber == $"CR-{today.Year}-00004");

            if (cr3 != null)
            {
                // Supervisor approval for CR-00003
                await _context.ChangeRequestApprovals.AddAsync(new ChangeRequestApproval
                {
                    ChangeRequestId = cr3.Id,
                    ApproverId = manager.Id,
                    Type = ApprovalType.Supervisor,
                    Status = ApprovalStatus.Approved,
                    Comments = "Approved. Please proceed with caution.",
                    RequestedAt = lastWeek,
                    ActionedAt = yesterday
                });
            }

            if (cr4 != null)
            {
                // CAB approval for CR-00004
                await _context.ChangeRequestApprovals.AddAsync(new ChangeRequestApproval
                {
                    ChangeRequestId = cr4.Id,
                    ApproverId = admin.Id,
                    Type = ApprovalType.CAB,
                    Status = ApprovalStatus.Approved,
                    Comments = "Approved by CAB. Schedule during maintenance window.",
                    RequestedAt = today.AddDays(-3),
                    ActionedAt = yesterday
                });
            }

            // Add assignments
            var cr5 = await _context.ChangeRequests.FirstOrDefaultAsync(cr => cr.ChangeRequestNumber == $"CR-{today.Year}-00005");
            var cr6 = await _context.ChangeRequests.FirstOrDefaultAsync(cr => cr.ChangeRequestNumber == $"CR-{today.Year}-00006");

            if (cr5 != null)
            {
                // Assign CR-00005 to support
                await _context.ChangeRequestAssignments.AddAsync(new ChangeRequestAssignment
                {
                    ChangeRequestId = cr5.Id,
                    AssigneeId = support.Id,
                    Role = "Implementer",
                    Notes = "Please implement according to the change plan.",
                    AssignedAt = yesterday,
                    Status = AssignmentStatus.Assigned
                });
            }

            if (cr6 != null)
            {
                // Assign CR-00006 to support
                await _context.ChangeRequestAssignments.AddAsync(new ChangeRequestAssignment
                {
                    ChangeRequestId = cr6.Id,
                    AssigneeId = support.Id,
                    Role = "Implementer",
                    Notes = "Please implement according to the change plan.",
                    AssignedAt = today.AddDays(-2),
                    Status = AssignmentStatus.InProgress
                });
            }

            // Add comments
            if (cr3 != null)
            {
                // Comment on CR-00003
                await _context.ChangeRequestComments.AddAsync(new ChangeRequestComment
                {
                    ChangeRequestId = cr3.Id,
                    CommenterId = user1.Id,
                    Text = "Added additional details to the implementation plan.",
                    IsInternal = false,
                    CreatedAt = today.AddDays(-3)
                });
            }

            if (cr4 != null)
            {
                // Comment on CR-00004
                await _context.ChangeRequestComments.AddAsync(new ChangeRequestComment
                {
                    ChangeRequestId = cr4.Id,
                    CommenterId = manager.Id,
                    Text = "Please ensure all stakeholders are notified before implementation.",
                    IsInternal = false,
                    CreatedAt = today.AddDays(-2)
                });

                // Internal comment on CR-00004
                await _context.ChangeRequestComments.AddAsync(new ChangeRequestComment
                {
                    ChangeRequestId = cr4.Id,
                    CommenterId = admin.Id,
                    Text = "This change needs careful monitoring during implementation.",
                    IsInternal = true,
                    CreatedAt = yesterday
                });
            }

            // Add history entries
            if (cr3 != null)
            {
                // History for CR-00003
                await _context.ChangeRequestHistory.AddRangeAsync(new[]
                {
                    new ChangeRequestHistory
                    {
                        ChangeRequestId = cr3.Id,
                        UserId = user1.Id,
                        ActionType = "Created",
                        Description = "Change request created",
                        CreatedAt = lastWeek
                    },
                    new ChangeRequestHistory
                    {
                        ChangeRequestId = cr3.Id,
                        UserId = user1.Id,
                        ActionType = "Submitted",
                        Description = "Submitted for supervisor approval",
                        CreatedAt = today.AddDays(-4)
                    },
                    new ChangeRequestHistory
                    {
                        ChangeRequestId = cr3.Id,
                        UserId = manager.Id,
                        ActionType = "Approved",
                        Description = "Approved by supervisor",
                        CreatedAt = yesterday
                    }
                });
            }

            if (cr4 != null)
            {
                // History for CR-00004
                await _context.ChangeRequestHistory.AddRangeAsync(new[]
                {
                    new ChangeRequestHistory
                    {
                        ChangeRequestId = cr4.Id,
                        UserId = user1.Id,
                        ActionType = "Created",
                        Description = "Change request created",
                        CreatedAt = today.AddDays(-14)
                    },
                    new ChangeRequestHistory
                    {
                        ChangeRequestId = cr4.Id,
                        UserId = user1.Id,
                        ActionType = "Submitted",
                        Description = "Submitted for supervisor approval",
                        CreatedAt = today.AddDays(-5)
                    },
                    new ChangeRequestHistory
                    {
                        ChangeRequestId = cr4.Id,
                        UserId = manager.Id,
                        ActionType = "Approved",
                        Description = "Approved by supervisor",
                        CreatedAt = today.AddDays(-3)
                    },
                    new ChangeRequestHistory
                    {
                        ChangeRequestId = cr4.Id,
                        UserId = user1.Id,
                        ActionType = "Submitted",
                        Description = "Submitted for CAB approval",
                        CreatedAt = today.AddDays(-2)
                    },
                    new ChangeRequestHistory
                    {
                        ChangeRequestId = cr4.Id,
                        UserId = admin.Id,
                        ActionType = "Approved",
                        Description = "Approved by CAB",
                        CreatedAt = yesterday
                    }
                });
            }

            await _context.SaveChangesAsync();

            return changeRequests.Count;
        }
    }
} 