using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using outDO.Models;

namespace outDO.Data
{
    //PASUL 3: users and roles
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }


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

        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectMember> ProjectMembers { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<Comment>  Comments { get; set; }
        public DbSet<outDO.Models.Task> Tasks { get; set; } //am pus asa ca imi dadea ceva abiguu
        public DbSet<BannedEmail> BannedEmails { get; set; }
    }
}
