using Microsoft.EntityFrameworkCore;
using wdb_backend.Common;
using wdb_backend.Data;
using wdb_backend.Models;
using wdb_backend.Services;

namespace wdb_backend.Tests.Services;

public class EmployerDashboardServiceTests
{
    private const string TestPassword = "Password123!";

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetDashboardAsync_ThrowsUnauthorizedAccessException_WhenEmployerDoesNotExist()
    {
        await using var dbContext = CreateDbContext();
        var service = new EmployerDashboardServiceImpl(dbContext);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.GetDashboardAsync(Guid.NewGuid())
        );
    }

    [Fact]
    public async Task GetDashboardAsync_ReturnsCompanyInformation_WhenEmployerExists()
    {
        await using var dbContext = CreateDbContext();

        var employerId = Guid.NewGuid();

        SeedEmployer(dbContext, employerId, "BuildSafe Ltd", "admin@buildsafe.nz");
        await dbContext.SaveChangesAsync();

        var service = new EmployerDashboardServiceImpl(dbContext);

        var result = await service.GetDashboardAsync(employerId);

        Assert.Equal("BuildSafe Ltd", result.Company.Name);
        Assert.Equal("admin@buildsafe.nz", result.Company.Email);
        Assert.True(result.Company.Verified);
    }

    [Fact]
    public async Task GetDashboardAsync_ReturnsSimplifiedSummaryCounts()
    {
        await using var dbContext = CreateDbContext();

        var employerId = Guid.NewGuid();
        var workerId = Guid.NewGuid();

        SeedEmployer(dbContext, employerId);
        SeedWorker(dbContext, workerId);

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

        var service = new EmployerDashboardServiceImpl(dbContext);

        var result = await service.GetDashboardAsync(employerId);

        Assert.Equal(1, result.Summary.PendingRequests);
        Assert.Equal(2, result.Summary.ReviewedRequests);
        Assert.Equal(3, result.Summary.TotalRequests);
    }

    [Fact]
    public async Task GetDashboardAsync_ReturnsLatestThreeRequests()
    {
        await using var dbContext = CreateDbContext();

        var employerId = Guid.NewGuid();
        var workerId = Guid.NewGuid();

        SeedEmployer(dbContext, employerId);
        SeedWorker(dbContext, workerId);

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

        var service = new EmployerDashboardServiceImpl(dbContext);

        var result = await service.GetDashboardAsync(employerId);

        Assert.Equal(3, result.RecentRequests.Count);
        Assert.Equal("Reason 5", result.RecentRequests[0].Reason);
        Assert.Equal("Reason 4", result.RecentRequests[1].Reason);
        Assert.Equal("Reason 3", result.RecentRequests[2].Reason);
    }

    [Fact]
    public async Task GetDashboardAsync_ReturnsPartiallyApprovedStatus_ForMixedPermissionResponses()
    {
        await using var dbContext = CreateDbContext();

        var employerId = Guid.NewGuid();
        var workerId = Guid.NewGuid();
        var requestId = SeedRequest(dbContext, employerId, workerId, "Mixed response check");

        SeedEmployer(dbContext, employerId);
        SeedWorker(dbContext, workerId);

        var approvedInfoId = SeedCustomWorkerInfo(dbContext, workerId, "Phone");
        var rejectedInfoId = SeedCustomWorkerInfo(dbContext, workerId, "Address");

        SeedPermission(dbContext, requestId, workerId, approvedInfoId, PermissionStatus.Approved);
        SeedPermission(dbContext, requestId, workerId, rejectedInfoId, PermissionStatus.Rejected);

        await dbContext.SaveChangesAsync();

        var service = new EmployerDashboardServiceImpl(dbContext);

        var result = await service.GetDashboardAsync(employerId);

        var request = Assert.Single(result.RecentRequests);
        Assert.Equal("PartiallyApproved", request.Status);
        Assert.Equal(0, result.Summary.PendingRequests);
        Assert.Equal(1, result.Summary.ReviewedRequests);
        Assert.Equal(1, result.Summary.TotalRequests);
    }

    private static void SeedEmployer(
        AppDbContext dbContext,
        Guid employerId,
        string name = "First Step Solutions",
        string email = "employer_001@test.com")
    {
        dbContext.Employers.Add(new Employer
        {
            Id = employerId,
            Name = name,
            Email = email,
            Password = TestPassword,
            Verified = true
        });
    }

    private static void SeedWorker(AppDbContext dbContext, Guid workerId)
    {
        dbContext.Workers.Add(new Worker
        {
            Id = workerId,
            Name = "Ben Carmen",
            Email = "worker_001@test.com",
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
