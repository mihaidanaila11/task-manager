using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using outDO.Models;

namespace outDO.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        
        public DbSet<Project> Projects { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<Models.Task> Tasks { get; set; }

        public DbSet<ProjectMember> ProjectMembers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // definire primary key compus

            modelBuilder.Entity<ProjectMember>()
            .HasKey(ac => new { ac.UserId, ac.ProjectId });

            // definire relatii cu modelele Category si Article (FK)

            modelBuilder.Entity<ProjectMember>()
            .HasOne(ac => ac.User)
            .WithMany(ac => ac.ProjectMembers)
            .HasForeignKey(ac => ac.UserId);

            modelBuilder.Entity<ProjectMember>()
            .HasOne(ac => ac.Project)
            .WithMany(ac => ac.ProjectMembers)
            .HasForeignKey(ac => ac.ProjectId);
        }
        
    }
}

