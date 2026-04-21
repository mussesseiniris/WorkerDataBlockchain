using Microsoft.EntityFrameworkCore;
using wdb_backend.Data;
using wdb_backend.Models;
using wdb_backend.Services;

public class PermissionRepoImplTests
{

    [Fact]
    public async Task AddAllByRequestAsync_valid_()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(databaseName: "TestDb")
        .Options;

        using var context = new AppDbContext(options);
        var repo = new PermissionRepoImpl(context);
        var worker_info1 = new WorkerInfo { Desc = "address", Value = "havana rise" };
        var worker_info2 = new WorkerInfo { Desc = "phone", Value = "123456" };
        var workerInfos = new List<WorkerInfo>();
        workerInfos.Add(worker_info1);
        workerInfos.Add(worker_info2);
        var request = new Request { WorkerId = Guid.NewGuid(), EmployerId = Guid.NewGuid(), Reason = "check the age" };

        // Act
        await repo.AddAllByRequestAsync(request, workerInfos, default);
        // Assert
        Assert.Equal(2, context.Permissions.Count());
    }
}
