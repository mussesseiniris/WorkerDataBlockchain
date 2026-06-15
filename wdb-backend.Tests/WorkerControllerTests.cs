
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using wdb_backend.Abstractions;
using wdb_backend.Common;
using wdb_backend.Controllers;
using wdb_backend.DTOs;
using wdb_backend.Models;

namespace wdb_backend.Tests;

public class WorkerControllerTests
{
    private readonly Mock<IPermissionService> _mockPermission;
    private readonly Mock<IRequestService> _mockRequest;
    private readonly Mock<IWorkerInfoService> _mockWorkerInfo;
    private readonly Mock<IEmployerService> _mockEmployer;
    private readonly Mock<IActiveAccessService> _mockActiveAccess;
    private readonly WorkerController _controller;
    private readonly Guid _workerId = Guid.NewGuid();

    public WorkerControllerTests()
    {
        _mockPermission = new Mock<IPermissionService>();
        _mockRequest = new Mock<IRequestService>();
        _mockWorkerInfo = new Mock<IWorkerInfoService>();
        _mockEmployer = new Mock<IEmployerService>();
        _mockActiveAccess = new Mock<IActiveAccessService>();

        _controller = new WorkerController(
            _mockPermission.Object,
            _mockRequest.Object,
            _mockWorkerInfo.Object,
            _mockEmployer.Object,
            _mockActiveAccess.Object
        );
    }

    // ── GetPermissions ────────────────────────────────────────────────────

    [Fact]
    public async Task GetPermissions_ReturnsOk_WhenPermissionsExist()
    {
        // Arrange
        var permissions = new List<Permission>
        {
            new Permission { Id = Guid.NewGuid(), Status = PermissionStatus.Pending }
        };
        _mockPermission
            .Setup(s => s.GetAllByWorkerIdAsync(_workerId, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        // Act
        var result = await _controller.GetPermissions(_workerId);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(permissions, ok.Value);
    }

    [Fact]
    public async Task GetPermissions_ReturnsNotFound_WhenNull()
    {
        // Arrange
        _ = _mockPermission
            .Setup(s => s.GetAllByWorkerIdAsync(_workerId, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<Permission>?)null);

        // Act
        var result = await _controller.GetPermissions(_workerId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    // ── GetRequests ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetRequests_ReturnsOk_WithMappedRows()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var employerId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();
        var infoId = Guid.NewGuid();

        var requests = new List<Request>
        {
            new Request
            {
                Id = requestId,
                EmployerId = employerId,
                WorkerId = _workerId,
                Reason = "Employment check",
                CreatedAt = DateTime.UtcNow
            }
        };

        var permissions = new List<Permission>
        {
            new Permission
            {
                Id = permissionId,
                RequestId = requestId,
                InfoId = infoId,
                Status = PermissionStatus.Pending
            }
        };

        var workerInfos = new List<WorkerInfo>
        {
            new WorkerInfo
            {
                Id = infoId,
                CustomLabel = "Address",
                Value = "Wellington",
                Type = "text"
            }
        };

        var employer = new Employer { Id = employerId, Name = "Test Corp" };

        _mockRequest
            .Setup(s => s.GetAllByWorkerIdAsync(_workerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(requests);
        _mockPermission
            .Setup(s => s.GetAllByWorkerIdAsync(_workerId, -1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);
        _mockWorkerInfo
            .Setup(s => s.GetAllAsync(_workerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workerInfos);
        _mockEmployer
            .Setup(s => s.GetEmployerInfoAsync(employerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employer);

        // Act
        var result = await _controller.GetRequests(_workerId);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var rows = Assert.IsType<List<WorkerController.RequestRowResponse>>(ok.Value);
        Assert.Single(rows);
        Assert.Equal("Test Corp", rows[0].Company);
        Assert.Equal("Employment check", rows[0].Reason);
    }

    [Fact]
    public async Task GetRequests_ReturnsOk_WithEmptyList_WhenNoRequests()
    {
        // Arrange
        _mockRequest
            .Setup(s => s.GetAllByWorkerIdAsync(_workerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Request>());
        _mockPermission
            .Setup(s => s.GetAllByWorkerIdAsync(_workerId, -1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Permission>());
        _mockWorkerInfo
            .Setup(s => s.GetAllAsync(_workerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WorkerInfo>());

        // Act
        var result = await _controller.GetRequests(_workerId);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var rows = Assert.IsType<List<WorkerController.RequestRowResponse>>(ok.Value);
        Assert.Empty(rows);
    }

    // ── GetActiveAccess ───────────────────────────────────────────────────

    [Fact]
    public async Task GetActiveAccess_ReturnsOk_WithEmptyList()
    {
        // Arrange
        _mockActiveAccess
            .Setup(s => s.GetActiveAccessAsync(_workerId, null, null))
            .ReturnsAsync(new List<ActiveAccessDto>());

        // Act
        var result = await _controller.GetActiveAccess(_workerId);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var value = Assert.IsType<List<ActiveAccessDto>>(ok.Value);
        Assert.Empty(value);
    }

    [Fact]
    public async Task GetActiveAccess_WithFilters_CallsServiceWithCorrectParams()
    {
        // Arrange
        var company = "TestCorp";
        var dataType = "address";

        var expected = new List<ActiveAccessDto>
        {
            new ActiveAccessDto
            {
                RequestId = Guid.NewGuid(),
                CompanyName = "TestCorp",
                GrantedAt = DateTime.UtcNow,
                Reason = "Onboarding",
                WorkerInfo = new List<ActiveAccessInfoDto>
                {
                    new ActiveAccessInfoDto
                    {
                        PermissionId = Guid.NewGuid(),
                        DataType = "address",
                        Category = "personal",
                        CategoryLabel = "Address"
                    }
                }
            }
        };

        _mockActiveAccess
            .Setup(s => s.GetActiveAccessAsync(_workerId, company, dataType))
            .ReturnsAsync(expected);

        // Act
        var result = await _controller.GetActiveAccess(_workerId, company, dataType);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var value = Assert.IsType<List<ActiveAccessDto>>(ok.Value);
        Assert.Single(value);
        Assert.Equal("TestCorp", value[0].CompanyName);
        Assert.Equal("address", value[0].WorkerInfo[0].DataType);

        _mockActiveAccess.Verify(
            s => s.GetActiveAccessAsync(_workerId, company, dataType),
            Times.Once);
    }
}