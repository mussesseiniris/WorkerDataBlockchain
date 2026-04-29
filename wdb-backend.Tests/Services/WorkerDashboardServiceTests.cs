using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using wdb_backend.Data;
using wdb_backend.Models;
using wdb_backend.Services;

namespace wdb_backend.Tests;

public class WorkerDashboardServiceTests
{
    private static AppDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetDashboardAsync_ShouldReturnNull_WhenWorkerDoesNotExist()
    {
        using var dbContext = CreateDbContext(nameof(GetDashboardAsync_ShouldReturnNull_WhenWorkerDoesNotExist));
        var service = new WorkerDashboardServiceImpl(dbContext);

        var workerId = Guid.NewGuid();

        var result = await service.GetDashboardAsync(workerId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetDashboardAsync_ShouldReturnWorkerBasicInfo_WhenWorkerExists()
    {
        // Arrange
        using var dbContext = CreateDbContext(nameof(GetDashboardAsync_ShouldReturnWorkerBasicInfo_WhenWorkerExists));

        var workerId = Guid.NewGuid();

        dbContext.Workers.Add(new Worker
        {
            Id = workerId,
            Name = "user",
            Email = "user@example.com",
            Password = "hashed-password",
            Verified = true,
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();

        var service = new WorkerDashboardServiceImpl(dbContext);

        // Act
        var result = await service.GetDashboardAsync(workerId);

        // Assert
        Assert.NotNull(result);

        var json = JsonSerializer.Serialize(result);

        Assert.Contains(workerId.ToString(), json);
        Assert.Contains("\"name\":\"user\"", json);
        Assert.Contains("\"email\":\"user@example.com\"", json);
        Assert.Contains("\"verified\":true", json);
    }

    [Fact]
    public async Task GetDashboardAsync_ShouldReturnLatestRequests_WhenWorkerHasRequests()
    {
        // Arrange
        using var dbContext = CreateDbContext(nameof(GetDashboardAsync_ShouldReturnLatestRequests_WhenWorkerHasRequests));

        var workerId = Guid.NewGuid();
        var otherWorkerId = Guid.NewGuid();

        var employer1Id = Guid.NewGuid();
        var employer2Id = Guid.NewGuid();

        dbContext.Workers.AddRange(
            new Worker
            {
                Id = workerId,
                Name = "user",
                Email = "user@example.com",
                Password = "hashed-password",
                Verified = true,
                CreatedAt = DateTime.UtcNow
            },
            new Worker
            {
                Id = otherWorkerId,
                Name = "other",
                Email = "other@example.com",
                Password = "hashed-password",
                Verified = true,
                CreatedAt = DateTime.UtcNow
            }
        );

        dbContext.Employers.AddRange(
            new Employer
            {
                Id = employer1Id,
                Name = "First Step Solutions",
                Email = "contact@firststepsolutions.nz",
                Password = "hashed-password",
                Verified = true,
                CreatedAt = DateTime.UtcNow
            },
            new Employer
            {
                Id = employer2Id,
                Name = "BuildSafe Ltd",
                Email = "admin@buildsafe.nz",
                Password = "hashed-password",
                Verified = true,
                CreatedAt = DateTime.UtcNow
            }
        );

        dbContext.Requests.AddRange(
            new Request
            {
                Id = Guid.NewGuid(),
                WorkerId = workerId,
                EmployerId = employer1Id,
                Reason = "Site onboarding",
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new Request
            {
                Id = Guid.NewGuid(),
                WorkerId = workerId,
                EmployerId = employer2Id,
                Reason = "PPE compliance check",
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            },
            new Request
            {
                Id = Guid.NewGuid(),
                WorkerId = otherWorkerId,
                EmployerId = employer1Id,
                Reason = "Other worker request",
                CreatedAt = DateTime.UtcNow
            }
        );

        await dbContext.SaveChangesAsync();

        var service = new WorkerDashboardServiceImpl(dbContext);

        // Act
        var result = await service.GetDashboardAsync(workerId);

        // Assert
        Assert.NotNull(result);

        var json = JsonSerializer.Serialize(result);

        Assert.Contains("\"latestRequests\":[", json);
        Assert.Contains("First Step Solutions", json);
        Assert.Contains("BuildSafe Ltd", json);
        Assert.Contains("Site onboarding", json);
        Assert.Contains("PPE compliance check", json);

        Assert.DoesNotContain("Other worker request", json);
    }

    [Fact]
    public async Task GetDashboardAsync_ShouldReturnOnlyLatestFiveRequests_InDescendingOrder()
    {
        // Arrange
        using var dbContext = CreateDbContext(nameof(GetDashboardAsync_ShouldReturnOnlyLatestFiveRequests_InDescendingOrder));

        var workerId = Guid.NewGuid();
        var employerId = Guid.NewGuid();

        dbContext.Workers.Add(new Worker
        {
            Id = workerId,
            Name = "user",
            Email = "user@example.com",
            Password = "hashed-password",
            Verified = true,
            CreatedAt = DateTime.UtcNow
        });

        dbContext.Employers.Add(new Employer
        {
            Id = employerId,
            Name = "First Step Solutions",
            Email = "contact@firststepsolutions.nz",
            Password = "hashed-password",
            Verified = true,
            CreatedAt = DateTime.UtcNow
        });

        var requestIds = new List<Guid>();

        for (int i = 0; i < 6; i++)
        {
            var requestId = Guid.NewGuid();
            requestIds.Add(requestId);

            dbContext.Requests.Add(new Request
            {
                Id = requestId,
                WorkerId = workerId,
                EmployerId = employerId,
                Reason = $"Reason {i}",
                CreatedAt = DateTime.UtcNow.AddHours(-i)
            });
        }

        await dbContext.SaveChangesAsync();

        var service = new WorkerDashboardServiceImpl(dbContext);

        // Act
        var result = await service.GetDashboardAsync(workerId);

        // Assert
        Assert.NotNull(result);

        var json = JsonSerializer.Serialize(result);

        // Only 5 requests should be returned
        var requestCount = json.Split("requestId").Length - 1;
        Assert.Equal(5, requestCount);

        // The newest five should be kept, and the oldest one (Reason 5) should be excluded
        Assert.Contains("Reason 0", json);
        Assert.Contains("Reason 1", json);
        Assert.Contains("Reason 2", json);
        Assert.Contains("Reason 3", json);
        Assert.Contains("Reason 4", json);
        Assert.DoesNotContain("Reason 5", json);

        // The requests should be ordered from newest to oldest: Reason 0 should appear before Reason 4
        var index0 = json.IndexOf("Reason 0", StringComparison.Ordinal);
        var index4 = json.IndexOf("Reason 4", StringComparison.Ordinal);

        Assert.True(index0 < index4);
    }

    [Fact]
    public async Task GetDashboardAsync_ShouldReturnEmptyBlockchainRecords_WhenNoBlockchainDataExists()
    {
        // Arrange
        using var dbContext = CreateDbContext(nameof(GetDashboardAsync_ShouldReturnEmptyBlockchainRecords_WhenNoBlockchainDataExists));

        var workerId = Guid.NewGuid();

        dbContext.Workers.Add(new Worker
        {
            Id = workerId,
            Name = "user",
            Email = "user@example.com",
            Password = "hashed-password",
            Verified = true,
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();

        var service = new WorkerDashboardServiceImpl(dbContext);

        // Act
        var result = await service.GetDashboardAsync(workerId);

        // Assert
        Assert.NotNull(result);

        var json = JsonSerializer.Serialize(result);

        Assert.Contains("\"blockchainRecords\":[]", json);
    }
}
