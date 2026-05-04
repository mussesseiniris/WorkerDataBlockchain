using System.ComponentModel;
using Moq;
using wdb_backend;
using wdb_backend.Abstractions;
using wdb_backend.Models;
using wdb_backend.Services;

public class WorkerInfoServiceTests
{
    [Fact]
    public async Task GetAllAsync_infoExists_returnsWorkerInfos()
    {
        // Arrange - prepare data
        var mockRepo = new Mock<IWorkerInfoRepository>();
        var service = new WorkerInfoServiceImpl(mockRepo.Object);
        var workerId = Guid.NewGuid();
        var fakeWorkerInfo1 = new WorkerInfo { WorkerId = workerId, Desc = "address", Value = "havana rise" };
        var fakeWorkerInfo2 = new WorkerInfo { WorkerId = workerId, Desc = "phone", Value = "12345678" };
        List<WorkerInfo> workerInfos = new List<WorkerInfo>();
        workerInfos.Add(fakeWorkerInfo1);
        workerInfos.Add(fakeWorkerInfo2);
        mockRepo.Setup(r => r.GetAllAsyncList(workerId, default)).ReturnsAsync(workerInfos);

        // Act - call method 
        var result = await service.GetAllAsyncList(workerId, default);

        // Assert - check the result
        Assert.Equal(workerInfos, result);
    }

    [Fact]
    public async Task GetAllAsync_infoNotExists_throwsException()
    {
        // Arrange - prepare data
        var mockRepo = new Mock<IWorkerInfoRepository>();
        var service = new WorkerInfoServiceImpl(mockRepo.Object);
        var workerId = Guid.NewGuid();

        // Act &Assert - call method and check the result
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetAllAsyncList(workerId));
    }

}
