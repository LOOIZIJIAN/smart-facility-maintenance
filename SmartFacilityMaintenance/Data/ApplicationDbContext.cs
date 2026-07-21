using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartFacilityMaintenance.Models;

namespace SmartFacilityMaintenance.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<MaintenanceRequest> MaintenanceRequests => Set<MaintenanceRequest>();
    public DbSet<RequestAttachment> RequestAttachments => Set<RequestAttachment>();
    public DbSet<RequestActivityLog> RequestActivityLogs => Set<RequestActivityLog>();
    public DbSet<Enquiry> Enquiries => Set<Enquiry>();
    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // A request links to a user twice (reporter + assignee), so turn off
        // cascade delete to avoid SQL Server's multiple-cascade-paths error.
        builder.Entity<MaintenanceRequest>()
            .HasOne(r => r.ReportedBy)
            .WithMany(u => u.ReportedRequests)
            .HasForeignKey(r => r.ReportedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<MaintenanceRequest>()
            .HasOne(r => r.AssignedTo)
            .WithMany(u => u.AssignedRequests)
            .HasForeignKey(r => r.AssignedToId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<MaintenanceRequest>()
            .HasOne(r => r.Category)
            .WithMany(c => c.Requests)
            .HasForeignKey(r => r.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<MaintenanceRequest>()
            .HasOne(r => r.Building)
            .WithMany(b => b.Requests)
            .HasForeignKey(r => r.BuildingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<RequestActivityLog>()
            .HasOne(l => l.PerformedBy)
            .WithMany()
            .HasForeignKey(l => l.PerformedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Enquiry>()
            .HasOne(e => e.RaisedBy)
            .WithMany()
            .HasForeignKey(e => e.RaisedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<MaintenanceRequest>()
            .Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Entity<MaintenanceRequest>()
            .Property(r => r.Priority)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Entity<Notification>()
            .Property(n => n.Title)
            .HasMaxLength(100);

        builder.Entity<Notification>()
            .Property(n => n.Message)
            .HasMaxLength(500);

        builder.Entity<Notification>()
            .Property(n => n.CreatedDate)
            .HasDefaultValueSql("GETDATE()");
    }
}
