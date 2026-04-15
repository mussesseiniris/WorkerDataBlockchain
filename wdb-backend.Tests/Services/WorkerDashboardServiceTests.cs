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
}
