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
            .HasColumnType("timestamptz")
            .IsRequired();

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
            .HasColumnType("timestamptz")
            .IsRequired();

        modelBuilder.Entity<JobApplication>()
            .Property(j => j.UpdatedAtUtc)
            .HasColumnType("timestamptz")
            .IsRequired();

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

        modelBuilder.Entity<Attachment>()
            .Property(a => a.CreatedAtUtc)
            .HasColumnType("timestamptz")
            .IsRequired();

        modelBuilder.Entity<Attachment>()
            .Property(a => a.DeletedAtUtc)
            .HasColumnType("timestamptz"); // nullable
    }

    // -----------------------------
    // UTC enforcement + auto timestamps
    // -----------------------------
    public override int SaveChanges()
    {
        NormalizeUtcDateTimes();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        NormalizeUtcDateTimes();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void NormalizeUtcDateTimes()
    {
        // Keep JobApplication created/updated timestamps consistent
        foreach (var entry in ChangeTracker.Entries<JobApplication>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = DateTime.UtcNow;
                entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
            }
        }

        // Ensure all DateTime / DateTime? values are stored as UTC
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified)) continue;

            foreach (var prop in entry.Properties)
            {
                var t = prop.Metadata.ClrType;
                if (t != typeof(DateTime) && t != typeof(DateTime?)) continue;

                if (prop.CurrentValue is not DateTime dt) continue;

                if (dt.Kind == DateTimeKind.Unspecified)
                    prop.CurrentValue = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                else if (dt.Kind == DateTimeKind.Local)
                    prop.CurrentValue = dt.ToUniversalTime();
            }
        }
    }
}
