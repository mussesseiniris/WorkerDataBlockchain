using Moq;
using wdb_backend.Abstractions;
using wdb_backend.Models;
using wdb_backend.Usecases;

public class CreateDataAccessRequestUsecaseTests
{

    [Fact]
    public async Task CreateDataAccessRequest_validRequestAndPermission_ServicesAreCalled()
    {
        //Arrange
        var mockPermService = new Mock<IPermissionService>();
        var mockRequService = new Mock<IRequestService>();
        var usecase = new CreateDataAccessRequestUsecaseImpl(mockRequService.Object, mockPermService.Object);

        var employer = new Employer { Id = Guid.NewGuid(), Name = "iris", Email = "test@email.com" };
        var worker = new Worker { Id = Guid.NewGuid(), Name = "Alice", Email = "workerTest@email.com" };
        var WorkerInfo1 = new WorkerInfo { WorkerId = worker.Id, Desc = "address", Value = "havanarise" };
        var WorkerInfo2 = new WorkerInfo { WorkerId = worker.Id, Desc = "phone", Value = "123456" };
        var workerinfos = new List<WorkerInfo>();
        var reason = "Check health";
        workerinfos.Add(WorkerInfo1);
        workerinfos.Add(WorkerInfo2);
        var fakeRequest = new Request { EmployerId = employer.Id, WorkerId = worker.Id, Reason = reason };
        mockRequService.Setup(m => m.CreateAsync(employer.Id, worker.Id, reason, default)).ReturnsAsync(fakeRequest);
        mockPermService.Setup(m => m.CreateAllByRequestAsync(fakeRequest, workerinfos, default)).Returns(Task.CompletedTask);


        //Act
        await usecase.CreateDataAccessRequest(workerinfos, employer.Id, worker.Id, reason);

        //Assert
        mockRequService.Verify(m => m.CreateAsync(employer.Id, worker.Id, reason, default), Times.Once);
        mockPermService.Verify(m => m.CreateAllByRequestAsync(fakeRequest, workerinfos, default), Times.Once);
    }


}