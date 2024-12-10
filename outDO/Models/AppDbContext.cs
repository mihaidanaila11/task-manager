using Microsoft.EntityFrameworkCore;

namespace outDO.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext>
                            options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
    }
}
