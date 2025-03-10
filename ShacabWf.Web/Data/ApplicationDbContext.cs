using Microsoft.EntityFrameworkCore;
using ShacabWf.Web.Models;

namespace ShacabWf.Web.Data
{
    /// <summary>
    /// Database context for the application
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<ChangeRequest> ChangeRequests { get; set; }
        public DbSet<ChangeRequestApproval> ChangeRequestApprovals { get; set; }
        public DbSet<ChangeRequestAssignment> ChangeRequestAssignments { get; set; }
        public DbSet<ChangeRequestComment> ChangeRequestComments { get; set; }
        public DbSet<ChangeRequestHistory> ChangeRequestHistory { get; set; }
        public DbSet<SupportDocument> SupportDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>()
                .HasMany(u => u.DirectReports)
                .WithOne(u => u.Supervisor)
                .HasForeignKey(u => u.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure ChangeRequest entity
            modelBuilder.Entity<ChangeRequest>()
                .HasOne(cr => cr.CreatedBy)
                .WithMany(u => u.CreatedChangeRequests)
                .HasForeignKey(cr => cr.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure ChangeRequestApproval entity
            modelBuilder.Entity<ChangeRequestApproval>()
                .HasOne(cra => cra.ChangeRequest)
                .WithMany(cr => cr.Approvals)
                .HasForeignKey(cra => cra.ChangeRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChangeRequestApproval>()
                .HasOne(cra => cra.Approver)
                .WithMany()
                .HasForeignKey(cra => cra.ApproverId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure ChangeRequestAssignment entity
            modelBuilder.Entity<ChangeRequestAssignment>()
                .HasOne(cra => cra.ChangeRequest)
                .WithMany(cr => cr.Assignments)
                .HasForeignKey(cra => cra.ChangeRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChangeRequestAssignment>()
                .HasOne(cra => cra.Assignee)
                .WithMany(u => u.Assignments)
                .HasForeignKey(cra => cra.AssigneeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure ChangeRequestComment entity
            modelBuilder.Entity<ChangeRequestComment>()
                .HasOne(crc => crc.ChangeRequest)
                .WithMany(cr => cr.Comments)
                .HasForeignKey(crc => crc.ChangeRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChangeRequestComment>()
                .HasOne(crc => crc.Commenter)
                .WithMany()
                .HasForeignKey(crc => crc.CommenterId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure ChangeRequestHistory entity
            modelBuilder.Entity<ChangeRequestHistory>()
                .HasOne(crh => crh.ChangeRequest)
                .WithMany(cr => cr.History)
                .HasForeignKey(crh => crh.ChangeRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChangeRequestHistory>()
                .HasOne(crh => crh.User)
                .WithMany()
                .HasForeignKey(crh => crh.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure SupportDocument entity
            modelBuilder.Entity<SupportDocument>()
                .HasOne(sd => sd.UploadedBy)
                .WithMany()
                .HasForeignKey(sd => sd.UploadedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed users
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@example.com",
                    Password = "Admin123!",
                    FirstName = "Admin",
                    LastName = "User",
                    Department = "IT",
                    IsCABMember = true,
                    IsSupportPersonnel = true
                },
                new User
                {
                    Id = 2,
                    Username = "manager",
                    Email = "manager@example.com",
                    Password = "Manager123!",
                    FirstName = "Manager",
                    LastName = "User",
                    Department = "IT",
                    IsCABMember = true
                },
                new User
                {
                    Id = 3,
                    Username = "user1",
                    Email = "user1@example.com",
                    Password = "User123!",
                    FirstName = "Regular",
                    LastName = "User",
                    Department = "Finance",
                    SupervisorId = 2
                },
                new User
                {
                    Id = 4,
                    Username = "support",
                    Email = "support@example.com",
                    Password = "Support123!",
                    FirstName = "Support",
                    LastName = "User",
                    Department = "IT Support",
                    IsSupportPersonnel = true
                }
            );
        }
    }
} 