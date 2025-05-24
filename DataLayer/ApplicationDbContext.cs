using LinkShorter.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace LinkShorter.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<UrlDl>? Urls { get; set; }
    }
}
