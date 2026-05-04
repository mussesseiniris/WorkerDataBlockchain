using Microsoft.AspNetCore.Mvc;
using Moq;
using wdb_backend.Abstractions;
using wdb_backend.Controllers;
using wdb_backend.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

public class WorkerInfoControllerTests
{
    // 1. mock interfaces that the controller depends on, and create an instance of the controller with those mocks
    private readonly Mock<IWorkerInfoService> _mockService;
    private readonly WorkerInfoController _controller;

    public WorkerInfoControllerTests()
    {
        _mockService = new Mock<IWorkerInfoService>();
        _controller = new WorkerInfoController(_mockService.Object);

        // 2. mock the User property of the controller to simulate an authenticated user with a workerId claim
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
        }, "mock"));

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
    }

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithWorkerInfo()
    {
        // Arrange
        var workerInfoSet = new HashSet<WorkerInfo>
        {
            new WorkerInfo { Id = Guid.NewGuid(), Desc = "Test Desc", Value = "Test Value" }
        };
        // 3. setup the mock service to return a predefined set of worker info when GetAllAsync is called
        _mockService.Setup(s => s.GetAllAsync(It.IsAny<Guid>())).ReturnsAsync(workerInfoSet);

        // Act - call the GetAll method of the controller
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result); // 注意：Controller 返回的是 IActionResult
        var returnValue = Assert.IsType<HashSet<WorkerInfo>>(okResult.Value);
        Assert.Single(returnValue);
    }

    [Fact]
    public async Task Create_ReturnsOkResult()
    {
        // Arrange
        var workerInfo = new WorkerInfo { Id = Guid.NewGuid(), Desc = "Test Desc", Value = "Test Value" };
        _mockService.Setup(s => s.CreateAsync(It.IsAny<Guid>(), It.IsAny<WorkerInfo>())).ReturnsAsync(workerInfo);

        // Act - call the Create method of the controller with a sample worker info
        var result = await _controller.Create(workerInfo);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        _mockService.Verify(s => s.CreateAsync(It.IsAny<Guid>(), It.IsAny<WorkerInfo>()), Times.Once);
    }

    [Fact]
    public async Task Update_ReturnsOkResult_WithUpdatedInfo()
    {
        // Arrange
        var workerInfo = new WorkerInfo { Id = Guid.NewGuid(), Desc = "Test", Value = "Test Value" };
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<Guid>(), It.IsAny<WorkerInfo>())).ReturnsAsync(workerInfo);

        // Act - call the Update method of the controller with a sample worker info
        var result = await _controller.Update(workerInfo);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<WorkerInfo>(okResult.Value);
        Assert.Equal(workerInfo.Id, returnValue.Id);
    }
}