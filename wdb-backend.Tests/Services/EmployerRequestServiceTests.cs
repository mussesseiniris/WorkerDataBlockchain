using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using wdb_backend.Abstractions;
using wdb_backend.Common;
using wdb_backend.Data;
using wdb_backend.DTOs;
using wdb_backend.Models;
using wdb_backend.Services;

namespace wdb_backend.Tests.Services;

public class EmployerRequestServiceTests
{
    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static EmployerRequestServiceImpl CreateService(AppDbContext context)
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<NotificationCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var blockchain = new Mock<IBlockchainService>();
        blockchain
            .Setup(b => b.LogCategoryTransactionAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<BlockchainAction>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("0xtest");

        return new EmployerRequestServiceImpl(
            context,
            mediator.Object,
            blockchain.Object,
            Mock.Of<ILogger<EmployerRequestServiceImpl>>());
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenWorkerDoesNotExist()
    {
        await using var context = CreateDbContext();
        var service = CreateService(context);

        var dto = new CreateEmployerRequestDto
        {
            WorkerEmail = "missing@test.com",
            Reason = "Site onboarding",
            PresetFieldIds = new List<Guid> { Guid.NewGuid() },
            CustomWorkerInfoIds = new List<Guid>()
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.CreateAsync(Guid.NewGuid(), dto));
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenNoRequestedItems()
    {
        await using var context = CreateDbContext();

        var workerEmail = "worker@test.com";

        context.Workers.Add(new Worker
        {
            Id = Guid.NewGuid(),
            Name = "Worker",
            Email = workerEmail
        });

        await context.SaveChangesAsync();

        var service = CreateService(context);

        var dto = new CreateEmployerRequestDto
        {
            WorkerEmail = workerEmail,
            Reason = "Empty request",
            PresetFieldIds = new List<Guid>(),
            CustomWorkerInfoIds = new List<Guid>(),
            CustomRequest = null
        };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync(Guid.NewGuid(), dto));
    }

    [Fact]
    public async Task CreateAsync_CreatesRequestWithNullExpiry()
    {
        await using var context = CreateDbContext();

        var employerId = Guid.NewGuid();
        var workerId = Guid.NewGuid();
        var fieldId = Guid.NewGuid();

        context.Employers.Add(new Employer
        {
            Id = employerId,
            Name = "Employer",
            Email = "employer@test.com",
            PrivateKey = "private-key",
            BlockchainAddress = "0xEmployer"
        });

        context.Workers.Add(new Worker
        {
            Id = workerId,
            Name = "Worker",
            Email = "worker@test.com",
            BlockchainAddress = "0xWorker"
        });

        context.Fields.Add(new Field
        {
            Id = fieldId,
            FieldName = "full_name",
            Label = "Full Name",
            AllowedType = "text"
        });

        await context.SaveChangesAsync();

        var service = CreateService(context);

        var dto = new CreateEmployerRequestDto
        {
            WorkerEmail = "worker@test.com",
            Reason = "Site onboarding",
            PresetFieldIds = new List<Guid> { fieldId },
            CustomWorkerInfoIds = new List<Guid>()
        };

        var result = await service.CreateAsync(employerId, dto);

        var request = await context.Requests
            .Include(r => r.Permissions)
            .FirstAsync(r => r.Id == result.RequestId);

        Assert.Equal(employerId, request.EmployerId);
        Assert.Equal(workerId, request.WorkerId);
        Assert.Equal("Site onboarding", request.Reason);
        Assert.Null(request.ExpiryDate);

        var permission = Assert.Single(request.Permissions);
        Assert.Equal(fieldId, permission.FieldId);
        Assert.Null(permission.InfoId);
        Assert.Equal(PermissionStatus.Pending, permission.Status);
    }

    [Fact]
    public async Task CreateAsync_DeduplicatesPresetFieldIds()
    {
        await using var context = CreateDbContext();

        var employerId = Guid.NewGuid();
        var workerId = Guid.NewGuid();
        var fieldId = Guid.NewGuid();

        context.Employers.Add(new Employer
        {
            Id = employerId,
            Name = "Employer",
            Email = "employer@test.com",
            PrivateKey = "private-key",
            BlockchainAddress = "0xEmployer"
        });

        context.Workers.Add(new Worker
        {
            Id = workerId,
            Name = "Worker",
            Email = "worker@test.com",
            BlockchainAddress = "0xWorker"
        });

        context.Fields.Add(new Field
        {
            Id = fieldId,
            FieldName = "full_name",
            Label = "Full Name",
            AllowedType = "text"
        });

        await context.SaveChangesAsync();

        var service = CreateService(context);

        var dto = new CreateEmployerRequestDto
        {
            WorkerEmail = "worker@test.com",
            Reason = "Duplicate field test",
            PresetFieldIds = new List<Guid> { fieldId, fieldId },
            CustomWorkerInfoIds = new List<Guid>()
        };

        var result = await service.CreateAsync(employerId, dto);

        var permissionCount = await context.Permissions
            .CountAsync(p => p.RequestId == result.RequestId);

        Assert.Equal(1, permissionCount);
    }

    [Fact]
    public async Task CreateAsync_CreatesCustomWorkerInfoPermission()
    {
        await using var context = CreateDbContext();

        var employerId = Guid.NewGuid();
        var workerId = Guid.NewGuid();
        var workerInfoId = Guid.NewGuid();

        context.Employers.Add(new Employer
        {
            Id = employerId,
            Name = "Employer",
            Email = "employer@test.com",
            PrivateKey = "private-key",
            BlockchainAddress = "0xEmployer"
        });

        context.Workers.Add(new Worker
        {
            Id = workerId,
            Name = "Worker",
            Email = "worker@test.com",
            BlockchainAddress = "0xWorker"
        });

        context.WorkerInfos.Add(new WorkerInfo
        {
            Id = workerInfoId,
            WorkerId = workerId,
            CustomLabel = "Emergency Contact",
            Type = "text",
            Value = "021000000"
        });

        await context.SaveChangesAsync();

        var service = CreateService(context);

        var dto = new CreateEmployerRequestDto
        {
            WorkerEmail = "worker@test.com",
            Reason = "Custom info request",
            PresetFieldIds = new List<Guid>(),
            CustomWorkerInfoIds = new List<Guid> { workerInfoId }
        };

        var result = await service.CreateAsync(employerId, dto);

        var permission = await context.Permissions
            .SingleAsync(p => p.RequestId == result.RequestId);

        Assert.Null(permission.FieldId);
        Assert.Equal(workerInfoId, permission.InfoId);
        Assert.Equal(PermissionStatus.Pending, permission.Status);
    }

    [Fact]
    public async Task CreateAsync_SavesCustomRequestAsPending()
    {
        await using var context = CreateDbContext();

        var employerId = Guid.NewGuid();
        var workerId = Guid.NewGuid();

        context.Employers.Add(new Employer
        {
            Id = employerId,
            Name = "Employer",
            Email = "employer@test.com",
            PrivateKey = "private-key",
            BlockchainAddress = "0xEmployer"
        });

        context.Workers.Add(new Worker
        {
            Id = workerId,
            Name = "Worker",
            Email = "worker@test.com",
            BlockchainAddress = "0xWorker"
        });

        await context.SaveChangesAsync();

        var service = CreateService(context);

        var dto = new CreateEmployerRequestDto
        {
            WorkerEmail = "worker@test.com",
            Reason = "Custom request",
            PresetFieldIds = new List<Guid>(),
            CustomWorkerInfoIds = new List<Guid>(),
            CustomRequest = "Please provide induction certificate"
        };

        var result = await service.CreateAsync(employerId, dto);

        var request = await context.Requests
            .SingleAsync(r => r.Id == result.RequestId);

        Assert.Equal("Please provide induction certificate", request.CustomRequest);
        Assert.Equal("pending", request.CustomRequestStatus);
        Assert.Null(request.ExpiryDate);
    }
}
