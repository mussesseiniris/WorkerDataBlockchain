using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using wdb_backend.Abstractions;
using wdb_backend.Common;
using wdb_backend.Controllers;
using wdb_backend.Models;

namespace wdb_backend.Tests;

public class WorkerControllerTests
{
    private readonly Mock<IPermissionService> _mockPermission;
    private readonly Mock<IRequestService> _mockRequest;
    private readonly Mock<IWorkerInfoService> _mockWorkerInfo;
    private readonly Mock<IEmployerService> _mockEmployer;
    private readonly WorkerController _controller;

    public WorkerControllerTests()
    {
        _mockPermission = new Mock<IPermissionService>();
        _mockRequest = new Mock<IRequestService>();
        _mockWorkerInfo = new Mock<IWorkerInfoService>();
        _mockEmployer = new Mock<IEmployerService>();

        _controller = new WorkerController(
            _mockPermission.Object,
            _mockRequest.Object,
            _mockWorkerInfo.Object,
            _mockEmployer.Object
        );
    }

    // --- GetPermissions ---

    [Fact]
    public async Task GetPermissions_ReturnsOk_WhenPermissionsExist()
    {
        var workerId = Guid.NewGuid();
        var permissions = new List<Permission> { new Permission { Status = PermissionStatus.Pending } };
        _mockPermission
            .Setup(s => s.GetAllByWorkerIdAsync(workerId, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        var result = await _controller.GetPermissions(workerId);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(permissions, ok.Value);
    }

    [Fact]
    public async Task GetPermissions_ReturnsNotFound_WhenNull()
    {
        var workerId = Guid.NewGuid();
        _mockPermission
            .Setup(s => s.GetAllByWorkerIdAsync(workerId, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<Permission>?)null);

        var result = await _controller.GetPermissions(workerId);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    // --- GetAllWorkerInfo ---

    [Fact]
    public async Task GetAllWorkerInfo_ReturnsOk_WhenInfoExists()
    {
        var workerId = Guid.NewGuid();
        var info = new List<WorkerInfo> { new WorkerInfo { Desc = "Job Title", Value = "Engineer" } };
        _mockWorkerInfo
            .Setup(s => s.GetAllAsync(workerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(info);

        var result = await _controller.GetAllWorkerInfo(workerId);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(info, ok.Value);
    }

    [Fact]
    public async Task GetAllWorkerInfo_ReturnsNotFound_WhenNull()
    {
        var workerId = Guid.NewGuid();
        _mockWorkerInfo
            .Setup(s => s.GetAllAsync(workerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<WorkerInfo>?)null);

        var result = await _controller.GetAllWorkerInfo(workerId);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    // --- GetRows ---

    [Fact]
    public async Task GetRows_ReturnsUnauthorized_WhenNoIdentityClaim()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var result = await _controller.GetRows();

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task GetRows_ReturnsOk_WhenValidUser()
    {
        var workerId = Guid.NewGuid();
        SetUserClaim(_controller, workerId);

        _mockRequest
            .Setup(s => s.GetAllByWorkerIdAsync(workerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Request>());
        _mockWorkerInfo
            .Setup(s => s.GetAllAsync(workerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WorkerInfo>());
        _mockPermission
            .Setup(s => s.GetAllByWorkerIdAsync(workerId, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Permission>());
        _mockEmployer
            .Setup(s => s.GetDistinctEmployers(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Employer>());

        var result = await _controller.GetRows();

        Assert.IsType<OkObjectResult>(result);
    }

    // --- GetActiveAccess ---

    [Fact]
    public async Task GetActiveAccess_ReturnsOk_WithEmptyList_WhenNoRequests()
    {
        var workerId = Guid.NewGuid();
        _mockRequest
            .Setup(s => s.GetAllByWorkerIdAsync(workerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Request>());
        _mockPermission
            .Setup(s => s.GetAllByWorkerIdAsync(workerId, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Permission>());
        _mockWorkerInfo
            .Setup(s => s.GetAllAsync(workerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WorkerInfo>());

        var result = await _controller.GetActiveAccess(workerId);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    // --- Helpers ---

    private static void SetUserClaim(WorkerController controller, Guid workerId)
    {
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, workerId.ToString()) };
        var identity = new ClaimsIdentity(claims, "test");
        var principal = new ClaimsPrincipal(identity);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }
}
