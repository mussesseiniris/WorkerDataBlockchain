using Microsoft.EntityFrameworkCore;
using wdb_backend.Common;
using wdb_backend.Data;
using wdb_backend.Models;
using wdb_backend.Services;

namespace wdb_backend.Tests;

public class EmployerDashboardServiceTests
{
    private static AppDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetDashboardAsync_ShouldThrowUnauthorizedAccessException_WhenEmployerDoesNotExist()
    {
        using var dbContext = CreateDbContext(
            nameof(GetDashboardAsync_ShouldThrowUnauthorizedAccessException_WhenEmployerDoesNotExist)
        );

        var service = new EmployerDashboardServiceImpl(dbContext);

        var employerId = Guid.NewGuid();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.GetDashboardAsync(employerId)
        );
    }

    [Fact]
    public async Task GetDashboardAsync_ShouldReturnCompanyInformation_WhenEmployerExists()
    {
        using var dbContext = CreateDbContext(
            nameof(GetDashboardAsync_ShouldReturnCompanyInformation_WhenEmployerExists)
        );

        var employerId = Guid.NewGuid();

        dbContext.Employers.Add(new Employer
        {
            Id = employerId,
            Name = "BuildSafe Ltd",
            Email = "admin@buildsafe.nz",
            Password = "hashed-password",
            Verified = true,
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();

        var service = new EmployerDashboardServiceImpl(dbContext);

        var result = await service.GetDashboardAsync(employerId);

        Assert.NotNull(result);
        Assert.Equal("BuildSafe Ltd", result.Company.Name);
        Assert.Equal("admin@buildsafe.nz", result.Company.Email);
        Assert.True(result.Company.Verified);
    }

    [Fact]
    public async Task GetDashboardAsync_ShouldReturnPendingStatus_WhenAllPermissionsArePending()
    {
        using var dbContext = CreateDbContext(
            nameof(GetDashboardAsync_ShouldReturnPendingStatus_WhenAllPermissionsArePending)
        );

        var employerId = Guid.NewGuid();
        var workerId = Guid.NewGuid();
        var requestId = Guid.NewGuid();

        SeedEmployer(dbContext, employerId);
        SeedWorker(dbContext, workerId);

        var phoneInfoId = SeedWorkerInfo(dbContext, workerId, "Phone", "021000000");
        var addressInfoId = SeedWorkerInfo(dbContext, workerId, "Address", "123 Test Street");

        SeedRequest(dbContext, requestId, employerId, workerId, "Safety compliance check");

        SeedPermission(
            dbContext,
            requestId,
            workerId,
            phoneInfoId,
            PermissionStatus.Pending,
            DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddMinutes(-2)
        );

        SeedPermission(
            dbContext,
            requestId,
            workerId,
            addressInfoId,
            PermissionStatus.Pending,
            DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddMinutes(-1)
        );

        await dbContext.SaveChangesAsync();

        var service = new EmployerDashboardServiceImpl(dbContext);

        var result = await service.GetDashboardAsync(employerId);

        Assert.Equal(1, result.Summary.PendingRequests);
        Assert.Equal(0, result.Summary.AvailableRequests);
        Assert.Equal(0, result.Summary.PartialRequests);

        var request = Assert.Single(result.RecentRequests);
        Assert.Equal("Pending", request.Status);
        Assert.Equal("Will", request.WorkerName);
        Assert.Contains("Phone", request.RequestedFields);
        Assert.Contains("Address", request.RequestedFields);
    }

    [Fact]
    public async Task GetDashboardAsync_ShouldReturnAvailableStatus_WhenAllPermissionsAreApprovedAndActive()
    {
        using var dbContext = CreateDbContext(
            nameof(GetDashboardAsync_ShouldReturnAvailableStatus_WhenAllPermissionsAreApprovedAndActive)
        );

        var employerId = Guid.NewGuid();
        var workerId = Guid.NewGuid();
        var requestId = Guid.NewGuid();

        SeedEmployer(dbContext, employerId);
        SeedWorker(dbContext, workerId);

        var phoneInfoId = SeedWorkerInfo(dbContext, workerId, "Phone", "021000000");
        var addressInfoId = SeedWorkerInfo(dbContext, workerId, "Address", "123 Test Street");

        SeedRequest(dbContext, requestId, employerId, workerId, "Site onboarding");

        SeedPermission(
            dbContext,
            requestId,
            workerId,
            phoneInfoId,
            PermissionStatus.Approved,
            DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddMinutes(-2)
        );

        SeedPermission(
            dbContext,
            requestId,
            workerId,
            addressInfoId,
            PermissionStatus.Approved,
            DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddMinutes(-1)
        );

        await dbContext.SaveChangesAsync();

        var service = new EmployerDashboardServiceImpl(dbContext);

        var result = await service.GetDashboardAsync(employerId);

        Assert.Equal(0, result.Summary.PendingRequests);
        Assert.Equal(1, result.Summary.AvailableRequests);
        Assert.Equal(0, result.Summary.PartialRequests);

        var request = Assert.Single(result.RecentRequests);
        Assert.Equal("Available", request.Status);
    }

    [Fact]
    public async Task GetDashboardAsync_ShouldReturnUnavailableStatus_WhenAllPermissionsAreRejected()
    {
        using var dbContext = CreateDbContext(
            nameof(GetDashboardAsync_ShouldReturnUnavailableStatus_WhenAllPermissionsAreRejected)
        );

        var employerId = Guid.NewGuid();
        var workerId = Guid.NewGuid();
        var requestId = Guid.NewGuid();

        SeedEmployer(dbContext, employerId);
        SeedWorker(dbContext, workerId);

        var phoneInfoId = SeedWorkerInfo(dbContext, workerId, "Phone", "021000000");
        var addressInfoId = SeedWorkerInfo(dbContext, workerId, "Address", "123 Test Street");

        SeedRequest(dbContext, requestId, employerId, workerId, "Emergency preparedness");

        SeedPermission(
            dbContext,
            requestId,
            workerId,
            phoneInfoId,
            PermissionStatus.Rejected,
            DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddMinutes(-2)
        );

        SeedPermission(
            dbContext,
            requestId,
            workerId,
            addressInfoId,
            PermissionStatus.Rejected,
            DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddMinutes(-1)
        );

        await dbContext.SaveChangesAsync();

        var service = new EmployerDashboardServiceImpl(dbContext);

        var result = await service.GetDashboardAsync(employerId);

        Assert.Equal(0, result.Summary.PendingRequests);
        Assert.Equal(0, result.Summary.AvailableRequests);
        Assert.Equal(0, result.Summary.PartialRequests);

        var request = Assert.Single(result.RecentRequests);
        Assert.Equal("Unavailable", request.Status);
    }

    [Fact]
    public async Task GetDashboardAsync_ShouldReturnUnavailableStatus_WhenAllApprovedPermissionsAreExpired()
    {
        using var dbContext = CreateDbContext(
            nameof(GetDashboardAsync_ShouldReturnUnavailableStatus_WhenAllApprovedPermissionsAreExpired)
        );

        var employerId = Guid.NewGuid();
        var workerId = Guid.NewGuid();
        var requestId = Guid.NewGuid();

        SeedEmployer(dbContext, employerId);
        SeedWorker(dbContext, workerId);

        var phoneInfoId = SeedWorkerInfo(dbContext, workerId, "Phone", "021000000");
        var addressInfoId = SeedWorkerInfo(dbContext, workerId, "Address", "123 Test Street");

        SeedRequest(dbContext, requestId, employerId, workerId, "Expired access check");

        SeedPermission(
            dbContext,
            requestId,
            workerId,
            phoneInfoId,
            PermissionStatus.Approved,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddMinutes(-2)
        );

        SeedPermission(
            dbContext,
            requestId,
            workerId,
            addressInfoId,
            PermissionStatus.Approved,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddMinutes(-1)
        );

        await dbContext.SaveChangesAsync();

        var service = new EmployerDashboardServiceImpl(dbContext);

        var result = await service.GetDashboardAsync(employerId);

        Assert.Equal(0, result.Summary.PendingRequests);
        Assert.Equal(0, result.Summary.AvailableRequests);
        Assert.Equal(0, result.Summary.PartialRequests);

        var request = Assert.Single(result.RecentRequests);
        Assert.Equal("Unavailable", request.Status);
    }

    [Fact]
    public async Task GetDashboardAsync_ShouldReturnPartialStatus_WhenPermissionsHaveMixedResponses()
    {
        using var dbContext = CreateDbContext(
            nameof(GetDashboardAsync_ShouldReturnPartialStatus_WhenPermissionsHaveMixedResponses)
        );

        var employerId = Guid.NewGuid();
        var workerId = Guid.NewGuid();
        var requestId = Guid.NewGuid();

        SeedEmployer(dbContext, employerId);
        SeedWorker(dbContext, workerId);

        var phoneInfoId = SeedWorkerInfo(dbContext, workerId, "Phone", "021000000");
        var addressInfoId = SeedWorkerInfo(dbContext, workerId, "Address", "123 Test Street");

        SeedRequest(dbContext, requestId, employerId, workerId, "Mixed response check");

        SeedPermission(
            dbContext,
            requestId,
            workerId,
            phoneInfoId,
            PermissionStatus.Approved,
            DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddMinutes(-2)
        );

        SeedPermission(
            dbContext,
            requestId,
            workerId,
            addressInfoId,
            PermissionStatus.Rejected,
            DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddMinutes(-1)
        );

        await dbContext.SaveChangesAsync();

        var service = new EmployerDashboardServiceImpl(dbContext);

        var result = await service.GetDashboardAsync(employerId);

        Assert.Equal(0, result.Summary.PendingRequests);
        Assert.Equal(0, result.Summary.AvailableRequests);
        Assert.Equal(1, result.Summary.PartialRequests);

        var request = Assert.Single(result.RecentRequests);
        Assert.Equal("Partial", request.Status);
    }

    [Fact]
    public async Task GetDashboardAsync_ShouldOnlyReturnRequestsForCurrentEmployer()
    {
        using var dbContext = CreateDbContext(
            nameof(GetDashboardAsync_ShouldOnlyReturnRequestsForCurrentEmployer)
        );

        var employerId = Guid.NewGuid();
        var otherEmployerId = Guid.NewGuid();
        var workerId = Guid.NewGuid();

        var currentEmployerRequestId = Guid.NewGuid();
        var otherEmployerRequestId = Guid.NewGuid();

        SeedEmployer(dbContext, employerId, "Current Employer", "current@example.com");
        SeedEmployer(dbContext, otherEmployerId, "Other Employer", "other@example.com");
        SeedWorker(dbContext, workerId);

        var phoneInfoId = SeedWorkerInfo(dbContext, workerId, "Phone", "021000000");

        SeedRequest(dbContext, currentEmployerRequestId, employerId, workerId, "Current employer request");
        SeedRequest(dbContext, otherEmployerRequestId, otherEmployerId, workerId, "Other employer request");

        SeedPermission(
            dbContext,
            currentEmployerRequestId,
            workerId,
            phoneInfoId,
            PermissionStatus.Pending,
            DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddMinutes(-1)
        );

        SeedPermission(
            dbContext,
            otherEmployerRequestId,
            workerId,
            phoneInfoId,
            PermissionStatus.Pending,
            DateTime.UtcNow.AddMinutes(30),
            DateTime.UtcNow
        );

        await dbContext.SaveChangesAsync();

        var service = new EmployerDashboardServiceImpl(dbContext);

        var result = await service.GetDashboardAsync(employerId);

        var request = Assert.Single(result.RecentRequests);

        Assert.Equal("Current employer request", request.Reason);
        Assert.DoesNotContain(result.RecentRequests, r => r.Reason == "Other employer request");
    }

    private static void SeedEmployer(
        AppDbContext dbContext,
        Guid employerId,
        string name = "BuildSafe Ltd",
        string email = "admin@buildsafe.nz"
    )
    {
        dbContext.Employers.Add(new Employer
        {
            Id = employerId,
            Name = name,
            Email = email,
            Password = "hashed-password",
            Verified = true,
            CreatedAt = DateTime.UtcNow
        });
    }

    private static void SeedWorker(AppDbContext dbContext, Guid workerId)
    {
        dbContext.Workers.Add(new Worker
        {
            Id = workerId,
            Name = "Will",
            Email = "will@example.com",
            Password = "hashed-password",
            Verified = true,
            CreatedAt = DateTime.UtcNow
        });
    }

    private static Guid SeedWorkerInfo(
        AppDbContext dbContext,
        Guid workerId,
        string desc,
        string value
    )
    {
        var infoId = Guid.NewGuid();

        dbContext.WorkerInfos.Add(new WorkerInfo
        {
            Id = infoId,
            WorkerId = workerId,
            Desc = desc,
            Value = value,
            CreatedAt = DateTime.UtcNow
        });

        return infoId;
    }

    private static void SeedRequest(
        AppDbContext dbContext,
        Guid requestId,
        Guid employerId,
        Guid workerId,
        string reason
    )
    {
        dbContext.Requests.Add(new Request
        {
            Id = requestId,
            EmployerId = employerId,
            WorkerId = workerId,
            Reason = reason,
            CreatedAt = DateTime.UtcNow
        });
    }

    private static void SeedPermission(
        AppDbContext dbContext,
        Guid requestId,
        Guid workerId,
        Guid infoId,
        PermissionStatus status,
        DateTime expiryDate,
        DateTime lastUpdatedAt
    )
    {
        dbContext.Permissions.Add(new Permission
        {
            Id = Guid.NewGuid(),
            RequestId = requestId,
            WorkerId = workerId,
            InfoId = infoId,
            Status = status,
            ExpiryDate = expiryDate,
            LastUpdatedAt = lastUpdatedAt
        });
    }
}
