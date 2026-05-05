
using Moq;
using wdb_backend.Abstractions;
using wdb_backend.Models;
using wdb_backend.Services;
using Xunit.Sdk;

public class PermissionServiceTests
{
    [Fact]
    public async Task CreateAllByRequestAsync_ValidPermission_RepoAddMethodIsCalled()
    {
        // arrange
        var mockRepo = new Mock<IPermissionRepository>();
        var service = new PermissionServiceImpl(mockRepo.Object);
        var worker_info1 = new WorkerInfo { Desc = "address", Value = "havana rise" };
        var worker_info2 = new WorkerInfo { Desc = "phone", Value = "123456" };
        var workerInfos = new List<WorkerInfo>();
        workerInfos.Add(worker_info1);
        workerInfos.Add(worker_info2);
        var request = new Request { WorkerId = Guid.NewGuid(), EmployerId = Guid.NewGuid(), Reason = "check the age" };
        mockRepo.Setup(r => r.AddAllByRequestAsync(request, workerInfos, default)).Returns(Task.CompletedTask);

        // act
        await service.CreateAllByRequestAsync(request, workerInfos);

        // Assert 
        mockRepo.Verify(r => r.AddAllByRequestAsync(request,workerInfos,default),Times.Once);
    }

}