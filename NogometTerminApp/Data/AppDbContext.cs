using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NogometTerminApp.Data;

namespace NogometTerminApp.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Player> Players { get; set; }
        public DbSet<Term> Terms { get; set; }
        public DbSet<TermRegistration> TermRegistrations { get; set; }
    }
}
