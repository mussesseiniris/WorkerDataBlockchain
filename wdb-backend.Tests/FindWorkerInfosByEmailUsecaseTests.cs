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
        var worker_info1 = new WorkerInfo { Desc = "address", Value = "havana rise" };
        var worker_info2 = new WorkerInfo { Desc = "phone", Value = "123456" };
        var workerInfos = new List<WorkerInfo>();
        workerInfos.Add(worker_info1);
        workerInfos.Add(worker_info2);
        mockWorkerService.Setup(r => r.GetByEmailAsync(fakeWorker.Email)).ReturnsAsync(fakeWorker);
        mockWorkerInfoService.Setup(r => r.GetAllAsync(fakeWorker.Id)).ReturnsAsync(workerInfos);
        // Act
        var resultWorkerInfos = await findUsecase.FindWorkerInfosByEmail(fakeWorker.Email, default);
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
        // var worker_info1 = new WorkerInfo { Desc = "address", Value = "havana rise" };
        // var worker_info2 = new WorkerInfo { Desc = "phone", Value = "123456" };
        // var workerInfos = new List<WorkerInfo>();
        // workerInfos.Add(worker_info1);
        // workerInfos.Add(worker_info2);
        // mockWorkerService.Setup(r => r.GetByEmailAsync(fakeWorker.Email)).ReturnsAsync(fakeWorker);
        // mockWorkerInfoService.Setup(r => r.GetAllAsync(fakeWorker.Id)).ReturnsAsync(workerInfos);
        // Act
        var resultWorkerInfos = await findUsecase.FindWorkerInfosByEmail(fakeWorker.Email, default);
        // Assert
        Assert.Empty(resultWorkerInfos);
    }

}