using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using wdb_backend.Abstractions;
using wdb_backend.Common;
using wdb_backend.Data;
using wdb_backend.Models;
using wdb_backend.Services;

namespace wdb_backend.Tests.Services;

public class WorkerDashboardServiceTests
{
    private const string TestPassword = "Password123!";

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static WorkerDashboardServiceImpl CreateService(AppDbContext dbContext)
    {
        var blockchainServiceMock = new Mock<IBlockchainService>();
        var loggerMock = new Mock<ILogger<WorkerDashboardServiceImpl>>();

        return new WorkerDashboardServiceImpl(
            dbContext,
            blockchainServiceMock.Object,
            loggerMock.Object
        );
    }

    [Fact]
    public async Task GetDashboardAsync_ReturnsNull_WhenWorkerDoesNotExist()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        var result = await service.GetDashboardAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetDashboardAsync_ReturnsWorkerBasicInfo_WhenWorkerExists()
    {
        await using var dbContext = CreateDbContext();

        var worker = new Worker
        {
            Id = Guid.NewGuid(),
            Name = "Alan Brown",
            Email = "worker_001@test.com",
            Password = TestPassword,
            Verified = false,
            BlockchainAddress = null
        };

        dbContext.Workers.Add(worker);
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var result = await service.GetDashboardAsync(worker.Id);

        Assert.NotNull(result);
        Assert.Equal(worker.Id, result.Worker.Id);
        Assert.Equal("Alan Brown", result.Worker.Name);
        Assert.Equal("worker_001@test.com", result.Worker.Email);
        Assert.False(result.Worker.Verified);
        Assert.Null(result.Worker.BlockchainAddress);
    }

    [Fact]
    public async Task GetDashboardAsync_ReturnsSimplifiedSummaryCounts()
    {
        await using var dbContext = CreateDbContext();

        var workerId = Guid.NewGuid();
        var employerId = Guid.NewGuid();

        SeedWorker(dbContext, workerId);
        SeedEmployer(dbContext, employerId);

        var pendingRequestId = SeedRequest(dbContext, employerId, workerId, "Pending request");
        var approvedRequestId = SeedRequest(dbContext, employerId, workerId, "Approved request");
        var rejectedRequestId = SeedRequest(dbContext, employerId, workerId, "Rejected request");

        var pendingInfoId = SeedCustomWorkerInfo(dbContext, workerId, "Phone");
        var approvedInfoId = SeedCustomWorkerInfo(dbContext, workerId, "Address");
        var rejectedInfoId = SeedCustomWorkerInfo(dbContext, workerId, "PPE Requirement");

        SeedPermission(dbContext, pendingRequestId, workerId, pendingInfoId, PermissionStatus.Pending);
        SeedPermission(dbContext, approvedRequestId, workerId, approvedInfoId, PermissionStatus.Approved);
        SeedPermission(dbContext, rejectedRequestId, workerId, rejectedInfoId, PermissionStatus.Rejected);

        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var result = await service.GetDashboardAsync(workerId);

        Assert.NotNull(result);
        Assert.Equal(1, result.Summary.PendingReviews);
        Assert.Equal(2, result.Summary.ReviewedRequests);
        Assert.Equal(3, result.Summary.TotalRequests);
    }

    [Fact]
    public async Task GetDashboardAsync_ReturnsLatestThreeRequests()
    {
        await using var dbContext = CreateDbContext();

        var workerId = Guid.NewGuid();
        var employerId = Guid.NewGuid();

        SeedWorker(dbContext, workerId);
        SeedEmployer(dbContext, employerId);

        for (var i = 1; i <= 5; i++)
        {
            var requestId = SeedRequest(
                dbContext,
                employerId,
                workerId,
                $"Reason {i}",
                new DateTime(2026, 5, i, 10, 0, 0, DateTimeKind.Utc)
            );

            var infoId = SeedCustomWorkerInfo(dbContext, workerId, $"Info {i}");
            SeedPermission(dbContext, requestId, workerId, infoId, PermissionStatus.Pending);
        }

        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var result = await service.GetDashboardAsync(workerId);

        Assert.NotNull(result);
        Assert.Equal(3, result.LatestRequests.Count);
        Assert.Equal("Reason 5", result.LatestRequests[0].CheckPurpose);
        Assert.Equal("Reason 4", result.LatestRequests[1].CheckPurpose);
        Assert.Equal("Reason 3", result.LatestRequests[2].CheckPurpose);
    }

    private static void SeedWorker(AppDbContext dbContext, Guid workerId)
    {
        dbContext.Workers.Add(new Worker
        {
            Id = workerId,
            Name = "Alan Brown",
            Email = "worker_001@test.com",
            Password = TestPassword,
            Verified = false,
            BlockchainAddress = null
        });
    }

    private static void SeedEmployer(AppDbContext dbContext, Guid employerId)
    {
        dbContext.Employers.Add(new Employer
        {
            Id = employerId,
            Name = "First Step Solutions",
            Email = "employer_001@test.com",
            Password = TestPassword,
            Verified = true
        });
    }

    private static Guid SeedRequest(
        AppDbContext dbContext,
        Guid employerId,
        Guid workerId,
        string reason,
        DateTime? createdAt = null)
    {
        var requestId = Guid.NewGuid();

        dbContext.Requests.Add(new Request
        {
            Id = requestId,
            EmployerId = employerId,
            WorkerId = workerId,
            Reason = reason,
            CreatedAt = createdAt ?? DateTime.UtcNow
        });

        return requestId;
    }

    private static Guid SeedCustomWorkerInfo(AppDbContext dbContext, Guid workerId, string label)
    {
        var infoId = Guid.NewGuid();

        dbContext.WorkerInfos.Add(new WorkerInfo
        {
            Id = infoId,
            WorkerId = workerId,
            CustomLabel = label,
            Type = "text",
            Value = $"Value for {label}",
            CreatedAt = DateTime.UtcNow
        });

        return infoId;
    }

    private static void SeedPermission(
        AppDbContext dbContext,
        Guid requestId,
        Guid workerId,
        Guid infoId,
        int status)
    {
        dbContext.Permissions.Add(new Permission
        {
            Id = Guid.NewGuid(),
            RequestId = requestId,
            WorkerId = workerId,
            InfoId = infoId,
            Status = status,
            LastUpdatedAt = DateTime.UtcNow
        });
    }
}
