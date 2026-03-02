using Microsoft.EntityFrameworkCore;
using ProxmoxDashboard.Models;

namespace ProxmoxDashboard.Data;


public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)
    {
    }

    public DbSet<ProxmoxServer> ProxmoxServers { get; set; }
    public DbSet<CachedVM> CachedVMs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CachedVM>()
            .HasOne(c => c.ProxmoxServer)
            .WithMany(s => s.CachedVMs)
            .HasForeignKey(c => c.ProxmoxServerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
