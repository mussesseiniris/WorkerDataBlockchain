using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using wdb_backend.Models;
using wdb_backend.Abstractions;
using wdb_backend.Common;
using wdb_backend.Controllers;

namespace wdb_backend.Tests;

public class ActiveAccessRowTest
{

    private static WorkerController CreateController(
        Mock<IPermissionService> permissionService,
        Mock<IRequestService> requestService,
        Mock<IWorkerInfoService> workerInfoService,
        Mock<IEmployerService> employerService)
    {
        return new WorkerController(
            permissionService.Object,
            requestService.Object,
            workerInfoService.Object,
            employerService.Object
        );
    }


    // Request with no approved permissions is skipped

    [Fact]
    public async Task GetActiveAccess_RequestWithNoApprovedPermissions_IsSkipped()
    {

        var workerId = Guid.NewGuid();
        var requestWithPerms = Guid.NewGuid();
        var requestWithout = Guid.NewGuid();
        var employerId = Guid.NewGuid();

        var mockPermissionService = new Mock<IPermissionService>();
        var mockRequestService = new Mock<IRequestService>();
        var mockWorkerInfoService = new Mock<IWorkerInfoService>();
        var mockEmployerService = new Mock<IEmployerService>();

        mockRequestService
            .Setup(s => s.GetAllByWorkerIdAsync(workerId))
            .ReturnsAsync(new List<Request>
            {
                new Request { Id = requestWithPerms, EmployerId = employerId, Reason = "Has perms", CreatedAt = DateTime.UtcNow },
                new Request { Id = requestWithout,   EmployerId = employerId, Reason = "No perms",  CreatedAt = DateTime.UtcNow }
            });

        mockPermissionService
            .Setup(s => s.GetAllByWorkerIdAsync(workerId, 1))
            .ReturnsAsync(new List<Permission>
            {
                new Permission { Id = Guid.NewGuid(), WorkerId = workerId, RequestId = requestWithPerms, Status = (PermissionStatus)1 }
            });

        mockWorkerInfoService.Setup(s => s.GetAllAsync(workerId)).ReturnsAsync(new List<WorkerInfo>());
        mockEmployerService.Setup(s => s.GetEmployerInfoAsync(employerId)).ReturnsAsync(new Employer { Name = "Acme Corp" });

        var controller = CreateController(mockPermissionService, mockRequestService, mockWorkerInfoService, mockEmployerService);

        // Act
        var result = await controller.GetActiveAccess(workerId);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var rows = Assert.IsType<List<WorkerController.ActiveAccessRow>>(ok.Value);

        Assert.Single(rows);
        Assert.Equal("Has perms", rows[0].Reason);
    }

    //All requests have no approved permissions

    [Fact]
    public async Task GetActiveAccess_AllRequestsHaveNoApprovedPermissions_ReturnsEmptyList()
    {

        var workerId = Guid.NewGuid();
        var employerId = Guid.NewGuid();

        var mockPermissionService = new Mock<IPermissionService>();
        var mockRequestService = new Mock<IRequestService>();
        var mockWorkerInfoService = new Mock<IWorkerInfoService>();
        var mockEmployerService = new Mock<IEmployerService>();

        mockRequestService
            .Setup(s => s.GetAllByWorkerIdAsync(workerId))
            .ReturnsAsync(new List<Request>
            {
                new Request { Id = Guid.NewGuid(), EmployerId = employerId, Reason = "Reason 1", CreatedAt = DateTime.UtcNow },
                new Request { Id = Guid.NewGuid(), EmployerId = employerId, Reason = "Reason 2", CreatedAt = DateTime.UtcNow }
            });

        mockPermissionService
            .Setup(s => s.GetAllByWorkerIdAsync(workerId, 1))
            .ReturnsAsync(new List<Permission>());

        mockWorkerInfoService.Setup(s => s.GetAllAsync(workerId)).ReturnsAsync(new List<WorkerInfo>());

        var controller = CreateController(mockPermissionService, mockRequestService, mockWorkerInfoService, mockEmployerService);

        // Act
        var result = await controller.GetActiveAccess(workerId);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var rows = Assert.IsType<List<WorkerController.ActiveAccessRow>>(ok.Value);

        Assert.Empty(rows);
    }

    // WorkerInfo not found → Label is "Unknown" 

    [Fact]
    public async Task GetActiveAccess_WorkerInfoNotFound_LabelIsUnknown()
    {
        // Arrange
        var workerId = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        var employerId = Guid.NewGuid();

        var mockPermissionService = new Mock<IPermissionService>();
        var mockRequestService = new Mock<IRequestService>();
        var mockWorkerInfoService = new Mock<IWorkerInfoService>();
        var mockEmployerService = new Mock<IEmployerService>();

        mockRequestService
            .Setup(s => s.GetAllByWorkerIdAsync(workerId))
            .ReturnsAsync(new List<Request>
            {
                new Request { Id = requestId, EmployerId = employerId, Reason = "Reason", CreatedAt = DateTime.UtcNow }
            });

        mockPermissionService
            .Setup(s => s.GetAllByWorkerIdAsync(workerId, 1))
            .ReturnsAsync(new List<Permission>
            {
                new Permission { Id = Guid.NewGuid(), WorkerId = workerId, RequestId = requestId, InfoId = Guid.NewGuid(), Status = (PermissionStatus)1 }
            });

        // empty — InfoId won't match anything
        mockWorkerInfoService.Setup(s => s.GetAllAsync(workerId)).ReturnsAsync(new List<WorkerInfo>());
        mockEmployerService.Setup(s => s.GetEmployerInfoAsync(employerId)).ReturnsAsync(new Employer { Name = "Acme Corp" });

        var controller = CreateController(mockPermissionService, mockRequestService, mockWorkerInfoService, mockEmployerService);

        // Act
        var result = await controller.GetActiveAccess(workerId);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var rows = Assert.IsType<List<WorkerController.ActiveAccessRow>>(ok.Value);

        Assert.Equal("Unknown", rows[0].WorkerInfo[0].Label);
    }

    //Employer not found → Company is "Unknown"

    [Fact]
    public async Task GetActiveAccess_EmployerNotFound_CompanyIsUnknown()
    {
        // Arrange
        var workerId = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        var employerId = Guid.NewGuid();

        var mockPermissionService = new Mock<IPermissionService>();
        var mockRequestService = new Mock<IRequestService>();
        var mockWorkerInfoService = new Mock<IWorkerInfoService>();
        var mockEmployerService = new Mock<IEmployerService>();

        mockRequestService
            .Setup(s => s.GetAllByWorkerIdAsync(workerId))
            .ReturnsAsync(new List<Request>
            {
                new Request { Id = requestId, EmployerId = employerId, Reason = "Reason", CreatedAt = DateTime.UtcNow }
            });

        mockPermissionService
            .Setup(s => s.GetAllByWorkerIdAsync(workerId, 1))
            .ReturnsAsync(new List<Permission>
            {
                new Permission { Id = Guid.NewGuid(), WorkerId = workerId, RequestId = requestId, Status = (PermissionStatus)1 }
            });

        mockWorkerInfoService.Setup(s => s.GetAllAsync(workerId)).ReturnsAsync(new List<WorkerInfo>());

        // employer returns null
        mockEmployerService.Setup(s => s.GetEmployerInfoAsync(employerId)).ReturnsAsync((Employer?)null);

        var controller = CreateController(mockPermissionService, mockRequestService, mockWorkerInfoService, mockEmployerService);

        // Act
        var result = await controller.GetActiveAccess(workerId);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var rows = Assert.IsType<List<WorkerController.ActiveAccessRow>>(ok.Value);

        Assert.Equal("Unknown", rows[0].Company);
    }

}