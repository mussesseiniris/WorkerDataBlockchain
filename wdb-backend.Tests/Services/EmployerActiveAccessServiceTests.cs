using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using wdb_backend.Abstractions;
using wdb_backend.Common;
using wdb_backend.Data;
using wdb_backend.Models;
using wdb_backend.Services;

namespace wdb_backend.Tests.Services;

public class EmployerActiveAccessServiceTests
{
    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static EmployerActiveAccessServiceImpl CreateService(AppDbContext context)
    {
        return new EmployerActiveAccessServiceImpl(
            context,
            Mock.Of<ISupabaseStorageService>(),
            Mock.Of<IMediator>(),
            Mock.Of<IBlockchainService>(),
            Mock.Of<ILogger<EmployerActiveAccessServiceImpl>>());
    }

    [Fact]
    public async Task GetActiveAccessAsync_Throws_WhenEmployerDoesNotExist()
    {
        await using var context = CreateDbContext();
        var service = CreateService(context);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.GetActiveAccessAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetActiveAccessAsync_ReturnsOnlyActiveApprovedAccess()
    {
        await using var context = CreateDbContext();

        var employerId = Guid.NewGuid();
        var workerId = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var fieldId = Guid.NewGuid();
        var infoId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        context.Employers.Add(new Employer
        {
            Id = employerId,
            Name = "First Step Solutions",
            Email = "employer@test.com"
        });

        context.Workers.Add(new Worker
        {
            Id = workerId,
            Name = "Worker One",
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

        context.WorkerInfos.Add(new WorkerInfo
        {
            Id = infoId,
            WorkerId = workerId,
            FieldId = fieldId,
            Type = "text",
            Value = "Worker One"
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
            InfoId = infoId,
            Status = PermissionStatus.Approved,
            LastUpdatedAt = new DateTime(2026, 6, 2, 10, 0, 0, DateTimeKind.Utc)
        });

        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.GetActiveAccessAsync(employerId);

        Assert.Single(result);
        Assert.Equal(requestId, result[0].RequestId);
        Assert.Equal("Worker One", result[0].WorkerName);
        Assert.Equal("Site onboarding", result[0].Reason);

        var category = Assert.Single(result[0].Categories);
        Assert.Equal("BasicInfo", category.Name);

        var item = Assert.Single(category.Items);
        Assert.Equal(permissionId, item.PermissionId);
        Assert.Equal("Full Name", item.Label);
        Assert.Equal("text", item.Type);
        Assert.False(item.IsCustom);
    }

    [Fact]
    public async Task GetActiveAccessAsync_IgnoresExpiredRequests()
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
            Reason = "Expired request",
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            ExpiryDate = DateTime.UtcNow.AddDays(-1)
        });

        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.GetActiveAccessAsync(employerId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ViewRequestAsync_Throws_WhenRequestDoesNotBelongToEmployer()
    {
        await using var context = CreateDbContext();

        var employerId = Guid.NewGuid();
        var otherEmployerId = Guid.NewGuid();
        var workerId = Guid.NewGuid();
        var requestId = Guid.NewGuid();

        context.Workers.Add(new Worker
        {
            Id = workerId,
            Name = "Worker",
            Email = "worker@test.com"
        });

        context.Requests.Add(new Request
        {
            Id = requestId,
            EmployerId = otherEmployerId,
            WorkerId = workerId,
            Reason = "Private request",
            CreatedAt = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(5)
        });

        await context.SaveChangesAsync();

        var service = CreateService(context);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.ViewRequestAsync(employerId, requestId));
    }

    [Fact]
    public async Task ViewRequestAsync_Throws_WhenRequestExpired()
    {
        await using var context = CreateDbContext();

        var employerId = Guid.NewGuid();
        var workerId = Guid.NewGuid();
        var requestId = Guid.NewGuid();

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
            Reason = "Expired request",
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            ExpiryDate = DateTime.UtcNow.AddDays(-1)
        });

        await context.SaveChangesAsync();

        var service = CreateService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ViewRequestAsync(employerId, requestId));
    }
}
