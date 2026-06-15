using Moq;
using wdb_backend.Abstractions;
using wdb_backend.Models;
using wdb_backend.Services;

public class WorkerInfoServiceTests
{
    private readonly Mock<IWorkerInfoRepository> _mockRepo;
    private readonly WorkerInfoServiceImpl _service;
    private readonly Guid _workerId = Guid.NewGuid();

    public WorkerInfoServiceTests()
    {
        _mockRepo = new Mock<IWorkerInfoRepository>();
        _service = new WorkerInfoServiceImpl(_mockRepo.Object);
    }

    // ── GetAllAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_WhenInfoExists_ReturnsWorkerInfos()
    {
        // Arrange
        var fakeInfos = new List<WorkerInfo>
        {
            new WorkerInfo { Id = Guid.NewGuid(), Type = "text", Value = "Wellington" },
            new WorkerInfo { Id = Guid.NewGuid(), Type = "text", Value = "021123456" }
        };
        _mockRepo.Setup(r => r.GetAllAsync(_workerId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(fakeInfos);

        // Act
        var result = await _service.GetAllAsync(_workerId);

        // Assert
        Assert.Equal(fakeInfos, result);
    }

    [Fact]
    public async Task GetAllAsync_WhenRepoReturnsNull_ThrowsKeyNotFoundException()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetAllAsync(_workerId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((List<WorkerInfo>)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.GetAllAsync(_workerId));
    }

    // ── GetAllWithPresetsAsync ────────────────────────────────────────────

    [Fact]
    public async Task GetAllWithPresetsAsync_ReturnsListFromRepo()
    {
        // Arrange
        var fakeInfos = new List<WorkerInfo>
        {
            new WorkerInfo { Id = Guid.NewGuid(), FieldId = Guid.NewGuid(), Type = "text" }
        };
        _mockRepo.Setup(r => r.GetAllWithPresetsAsync(_workerId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(fakeInfos);

        // Act
        var result = await _service.GetAllWithPresetsAsync(_workerId);

        // Assert
        Assert.Equal(fakeInfos, result);
    }

    // ── CreateAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ReturnsWorkerInfo_AndCallsRepo()
    {
        // Arrange
        var workerInfo = new WorkerInfo
        {
            Id = Guid.NewGuid(),
            CustomLabel = "My Field",
            Type = "text",
            Value = "Some value"
        };
        _mockRepo.Setup(r => r.AddOneAsync(_workerId, workerInfo, It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateAsync(_workerId, workerInfo);

        // Assert
        Assert.Equal(workerInfo, result);
        _mockRepo.Verify(r => r.AddOneAsync(_workerId, workerInfo, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── UpdateAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ReturnsUpdatedWorkerInfo()
    {
        // Arrange
        var workerInfo = new WorkerInfo
        {
            Id = Guid.NewGuid(),
            FieldId = Guid.NewGuid(),
            Type = "text",
            Value = "Updated value"
        };
        _mockRepo.Setup(r => r.UpdateAsync(_workerId, workerInfo, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(workerInfo);

        // Act
        var result = await _service.UpdateAsync(_workerId, workerInfo);

        // Assert
        Assert.Equal(workerInfo, result);
    }

    // ── GetEffectiveWorkerInfo ────────────────────────────────────────────

    [Fact]
    public async Task GetEffectiveWorkerInfo_ReturnsListFromRepo()
    {
        // Arrange
        var employerId = Guid.NewGuid();
        var fakeInfos = new List<WorkerInfo>
        {
            new WorkerInfo { Id = Guid.NewGuid(), Type = "text", Value = "approved data" }
        };
        _mockRepo.Setup(r => r.GetEffectiveWorkerInfo(_workerId, employerId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(fakeInfos);

        // Act
        var result = await _service.GetEffectiveWorkerInfo(_workerId, employerId);

        // Assert
        Assert.Equal(fakeInfos, result);
    }
}

