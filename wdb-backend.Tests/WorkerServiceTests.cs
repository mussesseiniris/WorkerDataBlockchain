using Moq;
using wdb_backend.Abstractions;
using wdb_backend.Models;
using wdb_backend.Services;
using wdb_backend.Common;
using wdb_backend.Data;
using Microsoft.EntityFrameworkCore;

namespace wdb_backend.Tests;

public class WorkerServiceTests
{
    // ── Helpers ───────────────────────────────────────────────────────────

    private static AppDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    // ── WorkerRepo ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByEmailAsync_EmailExists_ReturnsWorker()
    {
        // Arrange
        using var db = CreateDbContext(nameof(GetByEmailAsync_EmailExists_ReturnsWorker));
        db.Workers.Add(new Worker { Name = "test1", Email = "test1@email.com", CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var repo = new WorkerRepoImpl(db);

        // Act
        var result = await repo.GetByEmailAsync("test1@email.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test1@email.com", result.Email);
    }

    [Fact]
    public async Task GetByEmailAsync_EmailNotExists_ReturnsNull()
    {
        // Arrange
        using var db = CreateDbContext(nameof(GetByEmailAsync_EmailNotExists_ReturnsNull));
        var repo = new WorkerRepoImpl(db);

        // Act
        var result = await repo.GetByEmailAsync("notexists@email.com");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task EmailExistsAsync_EmailExists_ReturnsTrue()
    {
        // Arrange
        using var db = CreateDbContext(nameof(EmailExistsAsync_EmailExists_ReturnsTrue));
        db.Workers.Add(new Worker { Name = "test", Email = "exists@email.com", CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var repo = new WorkerRepoImpl(db);

        // Act
        var result = await repo.EmailExistsAsync("exists@email.com");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task EmailExistsAsync_EmailNotExists_ReturnsFalse()
    {
        // Arrange
        using var db = CreateDbContext(nameof(EmailExistsAsync_EmailNotExists_ReturnsFalse));
        var repo = new WorkerRepoImpl(db);

        // Act
        var result = await repo.EmailExistsAsync("ghost@email.com");

        // Assert
        Assert.False(result);
    }

    // ── PermissionService ─────────────────────────────────────────────────

    [Fact]
    public async Task GetAllByWorkerIdAsync_WithStatusFilter_ReturnsPendingOnly()
    {
        // Arrange
        using var db = CreateDbContext(nameof(GetAllByWorkerIdAsync_WithStatusFilter_ReturnsPendingOnly));
        var workerId = Guid.NewGuid();
        var requestId = Guid.NewGuid();

        db.Requests.Add(new Request
        {
            Id = requestId,
            WorkerId = workerId,
            EmployerId = Guid.NewGuid(),
            Reason = "test"
        });

        db.Permissions.AddRange(
            new Permission { WorkerId = workerId, RequestId = requestId, Status = PermissionStatus.Pending, LastUpdatedAt = DateTime.UtcNow },
            new Permission { WorkerId = workerId, RequestId = requestId, Status = PermissionStatus.Pending, LastUpdatedAt = DateTime.UtcNow },
            new Permission { WorkerId = workerId, RequestId = requestId, Status = PermissionStatus.Approved, LastUpdatedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var repo = new PermissionRepoImpl(db);
        var service = new PermissionServiceImpl(repo);

        // Act
        var result = await service.GetAllByWorkerIdAsync(workerId, (int)PermissionStatus.Pending);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.Equal(PermissionStatus.Pending, p.Status));
    }

    [Fact]
    public async Task GetAllByWorkerIdAsync_NoFilter_ReturnsAll()
    {
        // Arrange
        using var db = CreateDbContext(nameof(GetAllByWorkerIdAsync_NoFilter_ReturnsAll));
        var workerId = Guid.NewGuid();
        var requestId = Guid.NewGuid();

        db.Requests.Add(new Request
        {
            Id = requestId,
            WorkerId = workerId,
            EmployerId = Guid.NewGuid(),
            Reason = "test"
        });

        db.Permissions.AddRange(
            new Permission { WorkerId = workerId, RequestId = requestId, Status = PermissionStatus.Pending, LastUpdatedAt = DateTime.UtcNow },
            new Permission { WorkerId = workerId, RequestId = requestId, Status = PermissionStatus.Approved, LastUpdatedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var repo = new PermissionRepoImpl(db);
        var service = new PermissionServiceImpl(repo);

        // Act
        var result = await service.GetAllByWorkerIdAsync(workerId);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task UpdateAsync_ChangesStatusAndTimestamp()
    {
        // Arrange
        using var db = CreateDbContext(nameof(UpdateAsync_ChangesStatusAndTimestamp));
        var workerId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        var originalTimestamp = DateTime.UtcNow.AddSeconds(-10);

        db.Requests.Add(new Request
        {
            Id = requestId,
            WorkerId = workerId,
            EmployerId = Guid.NewGuid(),
            Reason = "test"
        });

        db.Permissions.Add(new Permission
        {
            Id = permissionId,
            WorkerId = workerId,
            RequestId = requestId,
            Status = PermissionStatus.Pending,
            LastUpdatedAt = originalTimestamp
        });
        await db.SaveChangesAsync();

        var repo = new PermissionRepoImpl(db);
        var service = new PermissionServiceImpl(repo);

        // Act
        var result = await service.UpdateAsync(permissionId, (int)PermissionStatus.Approved);

        // Assert
        Assert.Equal(PermissionStatus.Approved, result.Status);
        Assert.True(result.LastUpdatedAt > originalTimestamp);
    }

    [Fact]
    public async Task UpdateAsync_TerminalStatus_ThrowsInvalidOperationException()
    {
        // Arrange
        using var db = CreateDbContext(nameof(UpdateAsync_TerminalStatus_ThrowsInvalidOperationException));
        var permissionId = Guid.NewGuid();
        var workerId = Guid.NewGuid();
        var requestId = Guid.NewGuid();

        db.Requests.Add(new Request
        {
            Id = requestId,
            WorkerId = workerId,
            EmployerId = Guid.NewGuid(),
            Reason = "test"
        });

        db.Permissions.Add(new Permission
        {
            Id = permissionId,
            WorkerId = workerId,
            RequestId = requestId,
            Status = PermissionStatus.Rejected,
            LastUpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var repo = new PermissionRepoImpl(db);
        var service = new PermissionServiceImpl(repo);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateAsync(permissionId, (int)PermissionStatus.Approved));
    }

    // ── RequestService ────────────────────────────────────────────────────

    [Fact]
    public async Task GetByRequestIdAsync_Exists_ReturnsRequest()
    {
        // Arrange
        using var db = CreateDbContext(nameof(GetByRequestIdAsync_Exists_ReturnsRequest));
        var requestId = Guid.NewGuid();
        var reason = "Need your data";

        db.Requests.Add(new Request
        {
            Id = requestId,
            WorkerId = Guid.NewGuid(),
            EmployerId = Guid.NewGuid(),
            Reason = reason
        });
        await db.SaveChangesAsync();

        var repo = new RequestRepoImpl(db);
        var service = new RequestServiceImpl(repo);

        // Act
        var result = await service.GetByRequestIdAsync(requestId);

        // Assert
        Assert.Equal(requestId, result.Id);
        Assert.Equal(reason, result.Reason);
    }

    [Fact]
    public async Task GetByRequestIdAsync_NotExists_ThrowsKeyNotFoundException()
    {
        // Arrange
        using var db = CreateDbContext(nameof(GetByRequestIdAsync_NotExists_ThrowsKeyNotFoundException));
        var repo = new RequestRepoImpl(db);
        var service = new RequestServiceImpl(repo);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.GetByRequestIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAllByWorkerIdAsync_ReturnsCorrectRequests()
    {
        // Arrange
        using var db = CreateDbContext(nameof(GetAllByWorkerIdAsync_ReturnsCorrectRequests));
        var workerId = Guid.NewGuid();

        db.Requests.AddRange(
            new Request { WorkerId = workerId, EmployerId = Guid.NewGuid(), Reason = "reason 1" },
            new Request { WorkerId = workerId, EmployerId = Guid.NewGuid(), Reason = "reason 2" },
            new Request { WorkerId = Guid.NewGuid(), EmployerId = Guid.NewGuid(), Reason = "other worker" }
        );
        await db.SaveChangesAsync();

        var repo = new RequestRepoImpl(db);
        var service = new RequestServiceImpl(repo);

        // Act
        var result = await service.GetAllByWorkerIdAsync(workerId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Equal(workerId, r.WorkerId));
    }
}
