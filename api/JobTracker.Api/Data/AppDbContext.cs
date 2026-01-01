using JobTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<Attachment> Attachments => Set<Attachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // -----------------------------
        // AppUser
        // -----------------------------
        modelBuilder.Entity<AppUser>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<AppUser>()
            .Property(u => u.CreatedAtUtc)
            .HasColumnType("timestamptz");

        // -----------------------------
        // JobApplication
        // -----------------------------
        modelBuilder.Entity<JobApplication>()
            .HasOne(j => j.User)
            .WithMany(u => u.JobApplications)
            .HasForeignKey(j => j.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<JobApplication>()
            .Property(j => j.CreatedAtUtc)
            .HasColumnType("timestamptz");

        modelBuilder.Entity<JobApplication>()
            .Property(j => j.UpdatedAtUtc)
            .HasColumnType("timestamptz");

        // Indexes for list/search/status queries
        modelBuilder.Entity<JobApplication>()
            .HasIndex(j => new { j.UserId, j.UpdatedAtUtc });

        modelBuilder.Entity<JobApplication>()
            .HasIndex(j => new { j.UserId, j.Status });

        modelBuilder.Entity<JobApplication>()
            .HasIndex(j => new { j.UserId, j.CreatedAtUtc });

        // -----------------------------
        // Attachment
        // -----------------------------
        modelBuilder.Entity<Attachment>()
            .HasOne(a => a.JobApplication)
            .WithMany(j => j.Attachments)
            .HasForeignKey(a => a.JobApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Attachment>()
            .HasIndex(a => a.JobApplicationId);
    }
}
