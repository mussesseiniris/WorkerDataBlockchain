using Microsoft.EntityFrameworkCore;
using wdb_backend.Models;

namespace wdb_backend.Data;

/// <summary>
/// Database context for the application. Manages database connections
/// and maps entity models to database tables via Supabase (PostgreSQL).
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Represents the workers table in the database.
    /// </summary>
    public DbSet<Worker> Workers { get; set; }

    public DbSet<Employer> Employers { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<Request> Requests { get; set; }
    public DbSet<WorkerInfo> WorkerInfos { get; set; }
}
