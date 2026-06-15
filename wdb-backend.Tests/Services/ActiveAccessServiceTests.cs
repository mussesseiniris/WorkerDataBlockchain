using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using wdb_backend.Abstractions;
using wdb_backend.Common;
using wdb_backend.Data;
using wdb_backend.Models;
using wdb_backend.Services;

namespace wdb_backend.Tests.Services;

public class ActiveAccessServiceTests
{
    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static ActiveAccessServiceImpl CreateService(
        AppDbContext context,
        Mock<IBlockchainService>? blockchainMock = null)
    {
        return new ActiveAccessServiceImpl(
            context,
            blockchainMock?.Object ?? Mock.Of<IBlockchainService>(),
            Mock.Of<ILogger<ActiveAccessServiceImpl>>());
    }

    [Fact]
    public async Task GetActiveAccessAsync_ReturnsOnlyActiveApprovedPermissions()
    {
        await using var context = CreateDbContext();

        var workerId = Guid.NewGuid();
        var employerId = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var fieldId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        context.Employers.Add(new Employer
        {
            Id = employerId,
            Name = "Acme Corp",
            Email = "acme@example.com"
        });

        context.Workers.Add(new Worker
        {
            Id = workerId,
            Name = "Worker One",
            Email = "worker@example.com"
        });

        context.Categories.Add(new Category
        {
            Id = categoryId,
            CategoryName = "PersonalInformation"
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
            Reason = "Site onboarding",
            CreatedAt = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc),
            ExpiryDate = DateTime.UtcNow.AddDays(7)
        });

        context.Permissions.Add(new Permission
        {
            Id = permissionId,
            RequestId = requestId,
            WorkerId = workerId,
            FieldId = fieldId,
            Status = PermissionStatus.Approved,
            LastUpdatedAt = new DateTime(2026, 6, 2, 10, 0, 0, DateTimeKind.Utc)
        });

        await context.SaveChangesAsync();

        var service = CreateService(context);

        var rows = await service.GetActiveAccessAsync(workerId);

        Assert.Single(rows);
        Assert.Equal(requestId, rows[0].RequestId);
        Assert.Equal("Acme Corp", rows[0].CompanyName);
        Assert.Equal("Site onboarding", rows[0].Reason);

        var item = Assert.Single(rows[0].WorkerInfo);
        Assert.Equal(permissionId, item.PermissionId);
        Assert.Equal("Full Name", item.DataType);
        Assert.Equal("PersonalInformation", item.Category);
        Assert.Equal("Personal Information", item.CategoryLabel);
    }

    [Fact]
    public async Task GetActiveAccessAsync_SkipsRequestWithNoApprovedPermissions()
    {
        await using var context = CreateDbContext();

        var workerId = Guid.NewGuid();
        var employerId = Guid.NewGuid();

        context.Employers.Add(new Employer
        {
            Id = employerId,
            Name = "Acme Corp",
            Email = "acme@example.com"
        });

        context.Workers.Add(new Worker
        {
            Id = workerId,
            Name = "Worker One",
            Email = "worker@example.com"
        });

        context.Requests.Add(new Request
        {
            Id = Guid.NewGuid(),
            EmployerId = employerId,
            WorkerId = workerId,
            Reason = "Pending only",
            CreatedAt = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(7)
        });

        await context.SaveChangesAsync();

        var service = CreateService(context);

        var rows = await service.GetActiveAccessAsync(workerId);

        Assert.Empty(rows);
    }

    [Fact]
    public async Task GetActiveAccessAsync_SkipsExpiredRequest()
    {
        await using var context = CreateDbContext();

        var workerId = Guid.NewGuid();
        var employerId = Guid.NewGuid();
        var requestId = Guid.NewGuid();

        context.Employers.Add(new Employer
        {
            Id = employerId,
            Name = "Acme Corp",
            Email = "acme@example.com"
        });

        context.Workers.Add(new Worker
        {
            Id = workerId,
            Name = "Worker One",
            Email = "worker@example.com"
        });

        context.Requests.Add(new Request
        {
            Id = requestId,
            EmployerId = employerId,
            WorkerId = workerId,
            Reason = "Expired request",
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            ExpiryDate = DateTime.UtcNow.AddDays(-1)
        });

        context.Permissions.Add(new Permission
        {
            Id = Guid.NewGuid(),
            RequestId = requestId,
            WorkerId = workerId,
            Status = PermissionStatus.Approved,
            LastUpdatedAt = DateTime.UtcNow.AddDays(-5)
        });

        await context.SaveChangesAsync();

        var service = CreateService(context);

        var rows = await service.GetActiveAccessAsync(workerId);

        Assert.Empty(rows);
    }

    [Fact]
    public async Task GetActiveAccessAsync_EmployerNotFound_CompanyNameIsUnknown()
    {
        await using var context = CreateDbContext();

        var workerId = Guid.NewGuid();
        var missingEmployerId = Guid.NewGuid();
        var requestId = Guid.NewGuid();

        context.Workers.Add(new Worker
        {
            Id = workerId,
            Name = "Worker One",
            Email = "worker@example.com"
        });

        context.Requests.Add(new Request
        {
            Id = requestId,
            EmployerId = missingEmployerId,
            WorkerId = workerId,
            Reason = "Reason",
            CreatedAt = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(7)
        });

        context.Permissions.Add(new Permission
        {
            Id = Guid.NewGuid(),
            RequestId = requestId,
            WorkerId = workerId,
            Status = PermissionStatus.Approved,
            LastUpdatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        var service = CreateService(context);

        var rows = await service.GetActiveAccessAsync(workerId);

        Assert.Single(rows);
        Assert.Equal("Unknown", rows[0].CompanyName);
    }

    [Fact]
    public async Task GetActiveAccessAsync_LabelFallsBackToUnknown()
    {
        await using var context = CreateDbContext();

        var workerId = Guid.NewGuid();
        var employerId = Guid.NewGuid();
        var requestId = Guid.NewGuid();

        context.Employers.Add(new Employer
        {
            Id = employerId,
            Name = "Acme Corp",
            Email = "acme@example.com"
        });

        context.Workers.Add(new Worker
        {
            Id = workerId,
            Name = "Worker One",
            Email = "worker@example.com"
        });

        context.Requests.Add(new Request
        {
            Id = requestId,
            EmployerId = employerId,
            WorkerId = workerId,
            Reason = "Reason",
            CreatedAt = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(7)
        });

        context.Permissions.Add(new Permission
        {
            Id = Guid.NewGuid(),
            RequestId = requestId,
            WorkerId = workerId,
            Status = PermissionStatus.Approved,
            LastUpdatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        var service = CreateService(context);

        var rows = await service.GetActiveAccessAsync(workerId);

        var item = Assert.Single(rows[0].WorkerInfo);
        Assert.Equal("Unknown", item.DataType);
        Assert.Equal("OtherInformation", item.Category);
        Assert.Equal("Other Information", item.CategoryLabel);
    }

    [Fact]
    public async Task GetActiveAccessAsync_FiltersByCompany()
    {
        await using var context = CreateDbContext();

        var workerId = Guid.NewGuid();
        var employerA = Guid.NewGuid();
        var employerB = Guid.NewGuid();

        context.Employers.AddRange(
            new Employer { Id = employerA, Name = "Acme Corp", Email = "a@example.com" },
            new Employer { Id = employerB, Name = "Beta Ltd", Email = "b@example.com" });

        context.Workers.Add(new Worker
        {
            Id = workerId,
            Name = "Worker One",
            Email = "worker@example.com"
        });

        var requestA = Guid.NewGuid();
        var requestB = Guid.NewGuid();

        context.Requests.AddRange(
            new Request
            {
                Id = requestA,
                EmployerId = employerA,
                WorkerId = workerId,
                Reason = "Acme reason",
                CreatedAt = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(7)
            },
            new Request
            {
                Id = requestB,
                EmployerId = employerB,
                WorkerId = workerId,
                Reason = "Beta reason",
                CreatedAt = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(7)
            });

        context.Permissions.AddRange(
            new Permission
            {
                Id = Guid.NewGuid(),
                RequestId = requestA,
                WorkerId = workerId,
                Status = PermissionStatus.Approved,
                LastUpdatedAt = DateTime.UtcNow
            },
            new Permission
            {
                Id = Guid.NewGuid(),
                RequestId = requestB,
                WorkerId = workerId,
                Status = PermissionStatus.Approved,
                LastUpdatedAt = DateTime.UtcNow
            });

        await context.SaveChangesAsync();

        var service = CreateService(context);

        var rows = await service.GetActiveAccessAsync(workerId, company: "acme");

        Assert.Single(rows);
        Assert.Equal("Acme Corp", rows[0].CompanyName);
    }

    [Fact]
    public async Task GetActiveAccessAsync_FiltersByDataTypeOrCategory()
    {
        await using var context = CreateDbContext();

        var workerId = Guid.NewGuid();
        var employerId = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var fieldId = Guid.NewGuid();

        context.Employers.Add(new Employer
        {
            Id = employerId,
            Name = "Acme Corp",
            Email = "acme@example.com"
        });

        context.Workers.Add(new Worker
        {
            Id = workerId,
            Name = "Worker One",
            Email = "worker@example.com"
        });

        context.Categories.Add(new Category
        {
            Id = categoryId,
            CategoryName = "MedicalInformation"
        });

        context.Fields.Add(new Field
        {
            Id = fieldId,
            CategoryId = categoryId,
            FieldName = "ppe_requirements",
            Label = "PPE Requirements",
            AllowedType = "text"
        });

        context.Requests.Add(new Request
        {
            Id = requestId,
            EmployerId = employerId,
            WorkerId = workerId,
            Reason = "Safety check",
            CreatedAt = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(7)
        });

        context.Permissions.Add(new Permission
        {
            Id = Guid.NewGuid(),
            RequestId = requestId,
            WorkerId = workerId,
            FieldId = fieldId,
            Status = PermissionStatus.Approved,
            LastUpdatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        var service = CreateService(context);

        var rows = await service.GetActiveAccessAsync(workerId, dataType: "Medical Information");

        Assert.Single(rows);
        Assert.Equal("Medical Information", rows[0].WorkerInfo[0].CategoryLabel);
    }

    [Fact]
    public async Task RevokeRequestAccessAsync_RevokesApprovedPermissions_AndAddsNotification()
    {
        await using var context = CreateDbContext();

        var workerId = Guid.NewGuid();
        var employerId = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        context.Workers.Add(new Worker
        {
            Id = workerId,
            Name = "Worker One",
            Email = "worker@example.com",
            PrivateKey = "worker-private-key",
            BlockchainAddress = "0xWorker"
        });

        context.Employers.Add(new Employer
        {
            Id = employerId,
            Name = "Acme Corp",
            Email = "acme@example.com",
            BlockchainAddress = "0xEmployer"
        });

        context.Requests.Add(new Request
        {
            Id = requestId,
            EmployerId = employerId,
            WorkerId = workerId,
            Reason = "Site onboarding",
            CreatedAt = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(7)
        });

        context.Permissions.Add(new Permission
        {
            Id = permissionId,
            RequestId = requestId,
            WorkerId = workerId,
            Status = PermissionStatus.Approved,
            LastUpdatedAt = DateTime.UtcNow.AddDays(-1)
        });

        var blockchainMock = new Mock<IBlockchainService>();
        blockchainMock
            .Setup(b => b.LogCategoryTransactionAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                BlockchainAction.PermissionRevoked,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("0xtest");

        await context.SaveChangesAsync();

        var service = CreateService(context, blockchainMock);

        await service.RevokeRequestAccessAsync(workerId, requestId);

        var permission = await context.Permissions.SingleAsync(p => p.Id == permissionId);
        Assert.Equal(PermissionStatus.Revoked, permission.Status);

        var notification = await context.Notifications.SingleAsync();
        Assert.Equal(employerId, notification.RecipientEmployerId);
        Assert.Equal("ACCESS_REVOKED", notification.Type);
        Assert.Equal(requestId, notification.RequestId);
        Assert.False(notification.IsRead);

        blockchainMock.Verify(b => b.LogCategoryTransactionAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            requestId.ToString(),
            "RequestAccess",
            It.IsAny<string>(),
            It.IsAny<string>(),
            BlockchainAction.PermissionRevoked,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RevokeRequestAccessAsync_Throws_WhenRequestNotFound()
    {
        await using var context = CreateDbContext();

        var service = CreateService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.RevokeRequestAccessAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task RevokeRequestAccessAsync_Throws_WhenRequestExpired()
    {
        await using var context = CreateDbContext();

        var workerId = Guid.NewGuid();
        var employerId = Guid.NewGuid();
        var requestId = Guid.NewGuid();

        context.Requests.Add(new Request
        {
            Id = requestId,
            EmployerId = employerId,
            WorkerId = workerId,
            Reason = "Expired",
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            ExpiryDate = DateTime.UtcNow.AddDays(-1)
        });

        await context.SaveChangesAsync();

        var service = CreateService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RevokeRequestAccessAsync(workerId, requestId));
    }

    [Fact]
    public async Task RevokeRequestAccessAsync_Throws_WhenNoApprovedPermissions()
    {
        await using var context = CreateDbContext();

        var workerId = Guid.NewGuid();
        var employerId = Guid.NewGuid();
        var requestId = Guid.NewGuid();

        context.Requests.Add(new Request
        {
            Id = requestId,
            EmployerId = employerId,
            WorkerId = workerId,
            Reason = "No approved permissions",
            CreatedAt = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(7)
        });

        context.Permissions.Add(new Permission
        {
            Id = Guid.NewGuid(),
            RequestId = requestId,
            WorkerId = workerId,
            Status = PermissionStatus.Pending,
            LastUpdatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        var service = CreateService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RevokeRequestAccessAsync(workerId, requestId));
    }
}
