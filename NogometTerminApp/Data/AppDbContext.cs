using Microsoft.EntityFrameworkCore;

namespace NogometTerminApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Player> Players { get; set; }
        public DbSet<Term> Terms { get; set; }
        public DbSet<TermRegistration> TermRegistrations { get; set; }
    }
}
