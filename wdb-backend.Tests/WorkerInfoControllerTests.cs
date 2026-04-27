using Microsoft.AspNetCore.Mvc;
using Moq;
using wdb_backend.Abstractions;
using wdb_backend.Controllers;
using wdb_backend.Models;

public class WorkerInfoControllerTests
{
    private readonly Mock<IWorkerInfoRepository> _mockRepo;
    private readonly WorkerInfoController _controller;

    public WorkerInfoControllerTests()
    {
        _mockRepo = new Mock<IWorkerInfoRepository>();
        _controller = new WorkerInfoController(_mockRepo.Object);
    }

    [Fact]
    public async Task GetAllWorkerInfo_ReturnsOkResult_WithWorkerInfo()
    {
        // Arrange
        var workerId = Guid.NewGuid();
        var workerInfoSet = new HashSet<WorkerInfo>
            {
                new WorkerInfo { Id = Guid.NewGuid(), Desc = "Test Desc", Value = "Test Value" }
            };
        _mockRepo.Setup(repo => repo.GetAllAsync(workerId)).ReturnsAsync(workerInfoSet);

        // Act
        var result = await _controller.GetAllWorkerInfo(workerId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<HashSet<WorkerInfo>>(okResult.Value);
        Assert.Single(returnValue);
    }

    [Fact]
    public async Task AddWorkerInfo_ReturnsOkResult()
    {
        // Arrange
        var workerId = Guid.NewGuid();
        var workerInfo = new WorkerInfo { Id = Guid.NewGuid(), Desc = "Test Desc", Value = "Test Value" };

        // Act
        var result = await _controller.AddWorkerInfo(workerId, workerInfo);

        // Assert
        Assert.IsType<OkResult>(result);
        _mockRepo.Verify(repo => repo.AddOneAsync(workerId, workerInfo), Times.Once);
    }


    [Fact]
    public async Task UpdateWorkerInfo_ReturnsOkResult_WithUpdatedInfo()
    {
        // Arragnge
        var workerId = Guid.NewGuid();
        var workerInfo = new WorkerInfo { Id = Guid.NewGuid(), Desc = "Test", Value = "Test Value" };


        _mockRepo.Setup(repo => repo.UpdateAsync(workerId, workerInfo)).ReturnsAsync(workerInfo);

        // Act
        var result = await _controller.UpdateWorkerInfo(workerId, workerInfo);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<WorkerInfo>(okResult.Value);
        Assert.Equal(workerInfo.Id, returnValue.Id);
    }
}