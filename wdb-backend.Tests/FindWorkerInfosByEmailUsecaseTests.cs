using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Moq;
using wdb_backend.Abstractions;
using wdb_backend.Models;
using wdb_backend.Usecases;


public class FindWorkerInfosByEmailUsecaseTests
{
    [Fact]
    public async Task FindWorkerInfosByEmail_validWorker_ReturnsWorkerInfos()
    {
        // Arrange
        var mockWorkerService = new Mock<IWorkerService>();
        var mockWorkerInfoService = new Mock<IWorkerInfoService>();
        var findUsecase = new FindWorkerInfosByEmailUsecaseImpl(mockWorkerService.Object, mockWorkerInfoService.Object);
        var fakeWorker = new Worker { Id = Guid.NewGuid(), Email = "test@email", Name = "test" };
        var worker_info1 = new WorkerInfo { Value = "havana rise", Type = "text" };
        var worker_info2 = new WorkerInfo { Value = "123456", Type = "text" };
        var workerInfos = new List<WorkerInfo>();
        var fakeEmployerId = Guid.NewGuid();
        workerInfos.Add(worker_info1);
        workerInfos.Add(worker_info2);
        mockWorkerService.Setup(r => r.GetByEmailAsync(fakeWorker.Email)).ReturnsAsync(fakeWorker);
        mockWorkerInfoService.Setup(r => r.GetEffectiveWorkerInfo(fakeWorker.Id, fakeEmployerId, default)).ReturnsAsync(workerInfos);
        // Act
        var resultWorkerInfos = await findUsecase.FindWorkerInfosByEmail(fakeWorker.Email, fakeEmployerId, default);
        // Assert
        Assert.Equivalent(workerInfos, resultWorkerInfos);
    }

    [Fact]
    public async Task FindWorkerInfosByEmail_InvalidWorker_ReturnsEmpty()
    {
        // Arrange
        var mockWorkerService = new Mock<IWorkerService>();
        var mockWorkerInfoService = new Mock<IWorkerInfoService>();
        var findUsecase = new FindWorkerInfosByEmailUsecaseImpl(mockWorkerService.Object, mockWorkerInfoService.Object);
        var fakeWorker = new Worker { Id = Guid.NewGuid(), Email = "test@email", Name = "test" };
        var fakeEmployerId = Guid.NewGuid();

        // Act
        var resultWorkerInfos = await findUsecase.FindWorkerInfosByEmail(fakeWorker.Email, fakeEmployerId, default);
        // Assert
        Assert.Empty(resultWorkerInfos);
    }

}
