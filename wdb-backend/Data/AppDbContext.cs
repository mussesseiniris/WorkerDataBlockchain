using Microsoft.EntityFrameworkCore;
using wdb_backend.Models;

namespace wdb_backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Worker> Workers { get; set; }
    public DbSet<Employer> Employers { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Field> Fields { get; set; }
    public DbSet<WorkerInfo> WorkerInfos { get; set; }
    public DbSet<Request> Requests { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<wdb_backend.Models.Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(e =>
        {
            e.HasIndex(c => c.CategoryName).IsUnique();
        });

        modelBuilder.Entity<Field>(e =>
        {
            e.HasOne(f => f.Category)
             .WithMany(c => c.Fields)
             .HasForeignKey(f => f.CategoryId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(f => new { f.CategoryId, f.FieldName }).IsUnique();
            e.HasIndex(f => new { f.CategoryId, f.Label }).IsUnique();
        });

        modelBuilder.Entity<WorkerInfo>(e =>
        {
            e.HasOne(w => w.Worker)
             .WithMany()
             .HasForeignKey(w => w.WorkerId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(w => w.Field)
             .WithMany(f => f.WorkerInfos)
             .HasForeignKey(w => w.FieldId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(w => new { w.WorkerId, w.FieldId })
             .IsUnique()
             .HasFilter("field_id IS NOT NULL");

            e.HasIndex(w => new { w.WorkerId, w.CustomLabel })
             .IsUnique()
             .HasFilter("custom_label IS NOT NULL");

            e.ToTable(t => t.HasCheckConstraint(
                "CK_worker_info_field_xor_custom",
                "(field_id IS NOT NULL) <> (custom_label IS NOT NULL)"));

            e.Property(w => w.Type).IsRequired();
        });

        modelBuilder.Entity<Request>(e =>
        {
            // ExpiryDate is set by the worker during review.
            // It must be nullable while the request is still pending.
            e.Property(r => r.ExpiryDate).IsRequired(false);

            e.HasMany(r => r.Permissions)
             .WithOne(p => p.Request)
             .HasForeignKey(p => p.RequestId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Permission>(e =>
        {
            e.HasOne(p => p.Field)
             .WithMany()
             .HasForeignKey(p => p.FieldId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(p => p.WorkerInfo)
             .WithMany(w => w.Permissions)
             .HasForeignKey(p => p.InfoId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.Restrict);

            e.ToTable(t => t.HasCheckConstraint(
                "CK_permission_approved_has_info",
                "status NOT IN (1, 3) OR info_id IS NOT NULL"));

            e.ToTable(t => t.HasCheckConstraint(
                "CK_permission_field_or_info",
                "field_id IS NOT NULL OR info_id IS NOT NULL"));
        });

        modelBuilder.Entity<wdb_backend.Models.Notification>(e =>
        {
            e.HasOne(n => n.RecipientWorker)
             .WithMany()
             .HasForeignKey(n => n.RecipientWorkerId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(n => n.RecipientEmployer)
             .WithMany()
             .HasForeignKey(n => n.RecipientEmployerId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(n => n.Request)
             .WithMany()
             .HasForeignKey(n => n.RequestId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.SetNull);

            e.ToTable(t => t.HasCheckConstraint(
                "CK_notification_single_recipient",
                "(recipient_worker_id IS NOT NULL) <> (recipient_employer_id IS NOT NULL)"));

            e.ToTable(t => t.HasCheckConstraint(
                "CK_notification_type",
                "type IN ('NEW_REQUEST', 'DATA_ACCESSED', 'REQUEST_REVIEWED', 'ACCESS_REVOKED')"));
        });
    }
}
