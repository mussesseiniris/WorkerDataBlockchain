using Microsoft.EntityFrameworkCore;
using wdb_backend.Common;
using wdb_backend.Data;
using wdb_backend.Models;
using wdb_backend.Services;

namespace wdb_backend.Tests.Services;

public class EmployerSentRequestServiceTests
{
    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetSentRequestsAsync_Throws_WhenEmployerDoesNotExist()
    {
        await using var context = CreateDbContext();
        var service = new EmployerSentRequestServiceImpl(context);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.GetSentRequestsAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetSentRequestsAsync_ReturnsEmployerRequestsOnly()
    {
        await using var context = CreateDbContext();

        var employerId = Guid.NewGuid();
        var otherEmployerId = Guid.NewGuid();
        var workerId = Guid.NewGuid();

        context.Employers.AddRange(
            new Employer { Id = employerId, Name = "Employer A", Email = "a@test.com" },
            new Employer { Id = otherEmployerId, Name = "Employer B", Email = "b@test.com" });

        context.Workers.Add(new Worker
        {
            Id = workerId,
            Name = "Worker One",
            Email = "worker@test.com"
        });

        var request = new Request
        {
            Id = Guid.NewGuid(),
            EmployerId = employerId,
            WorkerId = workerId,
            Reason = "Site onboarding",
            CreatedAt = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc),
            ExpiryDate = null
        };

        var otherRequest = new Request
        {
            Id = Guid.NewGuid(),
            EmployerId = otherEmployerId,
            WorkerId = workerId,
            Reason = "Other employer request",
            CreatedAt = new DateTime(2026, 6, 2, 10, 0, 0, DateTimeKind.Utc),
            ExpiryDate = null
        };

        context.Requests.AddRange(request, otherRequest);
        await context.SaveChangesAsync();

        var service = new EmployerSentRequestServiceImpl(context);

        var result = await service.GetSentRequestsAsync(employerId);

        Assert.Single(result);
        Assert.Equal(request.Id, result[0].RequestId);
        Assert.Equal("Worker One", result[0].WorkerName);
        Assert.Equal("worker@test.com", result[0].WorkerEmail);
        Assert.Equal("Site onboarding", result[0].Reason);
    }

    [Fact]
    public async Task GetSentRequestsAsync_UsesMinValue_WhenExpiryDateIsNull()
    {
        await using var context = CreateDbContext();

        var employerId = Guid.NewGuid();
        var workerId = Guid.NewGuid();

        context.Employers.Add(new Employer
        {
            Id = employerId,
            Name = "Employer",
            Email = "employer@test.com"
        });

        context.Workers.Add(new Worker
        {
            Id = workerId,
            Name = "Worker",
            Email = "worker@test.com"
        });

        context.Requests.Add(new Request
        {
            Id = Guid.NewGuid(),
            EmployerId = employerId,
            WorkerId = workerId,
            Reason = "Pending request",
            CreatedAt = DateTime.UtcNow,
            ExpiryDate = null
        });

        await context.SaveChangesAsync();

        var service = new EmployerSentRequestServiceImpl(context);

        var result = await service.GetSentRequestsAsync(employerId);

        Assert.Single(result);
        Assert.Equal(DateTime.MinValue, result[0].ExpiryDate);
    }

    [Fact]
    public async Task GetSentRequestsAsync_ReturnsPermissionItems_WithPresetFieldCategoryAndLabel()
    {
        await using var context = CreateDbContext();

        var employerId = Guid.NewGuid();
        var workerId = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var fieldId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        context.Employers.Add(new Employer
        {
            Id = employerId,
            Name = "Employer",
            Email = "employer@test.com"
        });

        context.Workers.Add(new Worker
        {
            Id = workerId,
            Name = "Worker",
            Email = "worker@test.com"
        });

        context.Categories.Add(new Category
        {
            Id = categoryId,
            CategoryName = "BasicInfo"
        });

        context.Fields.Add(new Field
        {
            Id = fieldId,
            CategoryId = categoryId,
            FieldName = "full_name",
            Label = "Full Name",
            AllowedType = "text"
        });

        context.Requests.Add(new Request
        {
            Id = requestId,
            EmployerId = employerId,
            WorkerId = workerId,
            Reason = "Identity check",
            CreatedAt = DateTime.UtcNow,
            ExpiryDate = null
        });

        context.Permissions.Add(new Permission
        {
            Id = permissionId,
            RequestId = requestId,
            WorkerId = workerId,
            FieldId = fieldId,
            InfoId = null,
            Status = PermissionStatus.Pending,
            LastUpdatedAt = new DateTime(2026, 6, 2, 10, 0, 0, DateTimeKind.Utc)
        });

        await context.SaveChangesAsync();

        var service = new EmployerSentRequestServiceImpl(context);

        var result = await service.GetSentRequestsAsync(employerId);

        var item = Assert.Single(result[0].Items);
        Assert.Equal(permissionId, item.PermissionId);
        Assert.Equal("BasicInfo", item.CategoryName);
        Assert.Equal("Full Name", item.Label);
        Assert.Equal(PermissionStatus.Pending, item.Status);
        Assert.False(item.IsCustom);
    }

    [Fact]
    public async Task GetSentRequestsAsync_ReturnsCustomWorkerInfoItem()
    {
        await using var context = CreateDbContext();

        var employerId = Guid.NewGuid();
        var workerId = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        var workerInfoId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        context.Employers.Add(new Employer
        {
            Id = employerId,
            Name = "Employer",
            Email = "employer@test.com"
        });

        context.Workers.Add(new Worker
        {
            Id = workerId,
            Name = "Worker",
            Email = "worker@test.com"
        });

        context.WorkerInfos.Add(new WorkerInfo
        {
            Id = workerInfoId,
            WorkerId = workerId,
            CustomLabel = "Emergency Contact",
            Type = "text",
            Value = "021000000"
        });

        context.Requests.Add(new Request
        {
            Id = requestId,
            EmployerId = employerId,
            WorkerId = workerId,
            Reason = "Custom check",
            CreatedAt = DateTime.UtcNow,
            ExpiryDate = null
        });

        context.Permissions.Add(new Permission
        {
            Id = permissionId,
            RequestId = requestId,
            WorkerId = workerId,
            FieldId = null,
            InfoId = workerInfoId,
            Status = PermissionStatus.Approved,
            LastUpdatedAt = new DateTime(2026, 6, 3, 10, 0, 0, DateTimeKind.Utc)
        });

        await context.SaveChangesAsync();

        var service = new EmployerSentRequestServiceImpl(context);

        var result = await service.GetSentRequestsAsync(employerId);

        var item = Assert.Single(result[0].Items);
        Assert.Equal(permissionId, item.PermissionId);
        Assert.Equal("OtherInformation", item.CategoryName);
        Assert.Equal("Emergency Contact", item.Label);
        Assert.Equal(PermissionStatus.Approved, item.Status);
        Assert.True(item.IsCustom);
    }

    [Fact]
    public async Task GetSentRequestsAsync_UsesLatestPermissionUpdateAsLastUpdatedAt()
    {
        await using var context = CreateDbContext();

        var employerId = Guid.NewGuid();
        var workerId = Guid.NewGuid();
        var requestId = Guid.NewGuid();

        var createdAt = new DateTime(2026, 6, 1, 9, 0, 0, DateTimeKind.Utc);
        var latestUpdate = new DateTime(2026, 6, 5, 9, 0, 0, DateTimeKind.Utc);

        context.Employers.Add(new Employer
        {
            Id = employerId,
            Name = "Employer",
            Email = "employer@test.com"
        });

        context.Workers.Add(new Worker
        {
            Id = workerId,
            Name = "Worker",
            Email = "worker@test.com"
        });

        context.Requests.Add(new Request
        {
            Id = requestId,
            EmployerId = employerId,
            WorkerId = workerId,
            Reason = "Update check",
            CreatedAt = createdAt,
            ExpiryDate = null
        });

        context.Permissions.AddRange(
            new Permission
            {
                Id = Guid.NewGuid(),
                RequestId = requestId,
                WorkerId = workerId,
                Status = PermissionStatus.Pending,
                LastUpdatedAt = new DateTime(2026, 6, 2, 9, 0, 0, DateTimeKind.Utc)
            },
            new Permission
            {
                Id = Guid.NewGuid(),
                RequestId = requestId,
                WorkerId = workerId,
                Status = PermissionStatus.Approved,
                LastUpdatedAt = latestUpdate
            });

        await context.SaveChangesAsync();

        var service = new EmployerSentRequestServiceImpl(context);

        var result = await service.GetSentRequestsAsync(employerId);

        Assert.Single(result);
        Assert.Equal(latestUpdate, result[0].LastUpdatedAt);
    }

    [Fact]
    public async Task GetSentRequestsAsync_OrdersRequestsByCreatedAtDescending()
    {
        await using var context = CreateDbContext();

        var employerId = Guid.NewGuid();
        var workerId = Guid.NewGuid();

        var olderRequestId = Guid.NewGuid();
        var newerRequestId = Guid.NewGuid();

        context.Employers.Add(new Employer
        {
            Id = employerId,
            Name = "Employer",
            Email = "employer@test.com"
        });

        context.Workers.Add(new Worker
        {
            Id = workerId,
            Name = "Worker",
            Email = "worker@test.com"
        });

        context.Requests.AddRange(
            new Request
            {
                Id = olderRequestId,
                EmployerId = employerId,
                WorkerId = workerId,
                Reason = "Older request",
                CreatedAt = new DateTime(2026, 6, 1, 9, 0, 0, DateTimeKind.Utc),
                ExpiryDate = null
            },
            new Request
            {
                Id = newerRequestId,
                EmployerId = employerId,
                WorkerId = workerId,
                Reason = "Newer request",
                CreatedAt = new DateTime(2026, 6, 5, 9, 0, 0, DateTimeKind.Utc),
                ExpiryDate = null
            });

        await context.SaveChangesAsync();

        var service = new EmployerSentRequestServiceImpl(context);

        var result = await service.GetSentRequestsAsync(employerId);

        Assert.Equal(2, result.Count);
        Assert.Equal(newerRequestId, result[0].RequestId);
        Assert.Equal(olderRequestId, result[1].RequestId);
    }
}
