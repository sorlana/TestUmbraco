using Emax.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Emax.Domain
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<EmailRequestItem> EmailRequestItems { get; set; }
        public DbSet<CallRequestItem> CallRequestItems { get; set; }

    }
}
