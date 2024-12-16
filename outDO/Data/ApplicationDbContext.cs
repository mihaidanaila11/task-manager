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

        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectMember> ProjectMembers { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<Comment>  Comments { get; set; }
        public DbSet<outDO.Models.Task> Tasks { get; set; } //am pus asa ca imi dadea ceva abiguu
    }
}
