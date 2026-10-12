
using Microsoft.EntityFrameworkCore;
using SubTrack.Models;

namespace SubTrack.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Subscription> Subscriptions =>
            Set<Subscription>();

        public DbSet<ApplicationUser> Users => Set<ApplicationUser>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Subscription>()
                .HasIndex(subscription => subscription.UserId);
        }
    }
}
