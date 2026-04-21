using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using wdb_backend.Models;
using wdb_backend.Services;
using wdb_backend.Abstractions;
using System.Security;
using wdb_backend.Common;
using Microsoft.VisualBasic;
using wdb_backend.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
namespace wdb_backend.Tests;

public class WorkerServiceTests
{
    [Fact]
    async public Task GetPermissionsTest()
    {
        // Arrange: I have no idea how to set the layers up
        var mockPermissionRepo = new Mock<IPermissionRepository>();
        var permissionService = new PermissionServiceImpl(mockPermissionRepo.Object);

        var mockRequestRepo = new Mock<IRequestRepository>();
        var requestService = new RequestServiceImpl(mockRequestRepo.Object);

        var workerId = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        var employerId = Guid.NewGuid();

        //permissions
        var fakePermissions = new List<Permission>
        {
            new Permission {Id = Guid.NewGuid(), WorkerId = workerId, Status = 0, RequestId = requestId},
            new Permission {Id = Guid.NewGuid(), WorkerId = workerId, Status = 0, RequestId = requestId},
            new Permission {Id = Guid.NewGuid(), WorkerId = workerId, Status = (PermissionStatus)1, RequestId = requestId}

        };

        //requests
        var fakeRequests = new List<Request>{
            new Request{Id = requestId, EmployerId = employerId, Reason = "Reason 1"},
            new Request{Id = Guid.NewGuid(), EmployerId = employerId, Reason ="Reason 2"},
        };
        
        mockPermissionRepo.Setup(r => r.GetAllByWorkerIdAsync(workerId, default)).ReturnsAsync(fakePermissions);
        mockRequestRepo.Setup(r => r.GetAllByWorkerIdAsync(workerId, default)).ReturnsAsync(fakeRequests);

        // Act: 
        //Get permission based on workerid and filter to pending status
        var permissionResult = await permissionService.GetAllByWorkerIdAsync(workerId, 0);
        
        //Get requets using request id from permission rows 
        var requestResult = await requestService.GetByRequestIdAsync(workerId, requestId);

        
        // Assert: the returned data is a list of permissions/request
        var returnedPermissions = Assert.IsType<List<Permission>>(permissionResult);
        var returnedRequests = Assert.IsType<Request>(requestResult);
        
        // Assert: the correct number of permission retrieved
        Assert.Equal(2, returnedPermissions.Count);
        
        // Assert: the permission status of rows is pending
        Assert.All(returnedPermissions, returnedPermission => Assert.True(returnedPermission.Status == 0));
        
        // Assert: requests have correct requestId
        Assert.True(returnedRequests.Id == requestId);

    }

     [Fact]
    async public Task ChangePermissionStatusTest()
    {
        //permission id gets passed from frontend to backend 
        //goes to controller layer, calls the service layer, calls the repo layer

        //Arrange:
        var mockPermissionRepo = new Mock<IPermissionRepository>();
        var permissionService = new PermissionServiceImpl(mockPermissionRepo.Object);
        
        var workerId = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();
        

        var fakePermission = new Permission {Id = permissionId, WorkerId = workerId, Status = 0, LastUpdatedAt = DateTime.UtcNow.AddSeconds(-1), RequestId = requestId};
        var fakePermissionUpdate = new Permission {Id = permissionId, WorkerId = workerId, Status = (PermissionStatus)1, LastUpdatedAt = DateTime.UtcNow, RequestId = requestId};
        var originalTimestamp = fakePermission.LastUpdatedAt;

        mockPermissionRepo.Setup(r => r.GetOneAsync(permissionId, default)).ReturnsAsync(fakePermission);
        mockPermissionRepo.Setup(r => r.UpdateAsync(permissionId, fakePermission, default)).ReturnsAsync(fakePermissionUpdate);

        //Act:
        var updateResult = await permissionService.UpdateAsync(permissionId, 1);


        //Assert:
        var returnedPermissionUpdate = Assert.IsType<Permission>(updateResult);

        //test permission status is approve/disapprove
        Assert.True(returnedPermissionUpdate.Status == (PermissionStatus)1);

        //test permission last_updated_at is updated
        Assert.True(returnedPermissionUpdate.LastUpdatedAt > originalTimestamp);

    }

    private static AppDbContext CreateDbContext(String dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(dbName)
        .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetAllWorkerById_ShouldReturnPermission()
    {
        //Arrange
        using var dbContext = CreateDbContext(nameof(GetAllWorkerById_ShouldReturnPermission));

        var workerId = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        var reason = "Because I want your data";

        dbContext.Permissions.AddRange( 
        new Permission
        {
            WorkerId = workerId,
            RequestId = requestId,
            Status = 0
        }, 
        new Permission
        {
            WorkerId = workerId,
            RequestId = requestId,
            Status = 0
        },
        new Permission
        {
            WorkerId = workerId,
            RequestId = requestId,
            Status = (PermissionStatus)1
        }
        );

        dbContext.Requests.AddRange(
        new Request
        {
          
          WorkerId = workerId,
          Reason = reason,
          Id = requestId 
        },
        new Request
        {
            WorkerId = workerId,
            Reason = "just because",
            Id = Guid.NewGuid()
        }
        );

        await dbContext.SaveChangesAsync();

        var permissionRepo = new PermissionRepoImpl(dbContext);
        var permissionService = new PermissionServiceImpl(permissionRepo);

        var requestRepo = new RequestRepoImpl(dbContext);
        var requestService = new RequestServiceImpl(requestRepo);

        //Act
        var resultPermission = await permissionService.GetAllByWorkerIdAsync(workerId, 0);
        var resultRequest = await requestService.GetByRequestIdAsync(workerId, requestId);

        //Assert
        Assert.All(resultPermission, returnedPermission => Assert.True(returnedPermission.Status == 0));
        Assert.Equal(2, resultPermission.Count);

        Assert.True(resultRequest.Id == requestId);
        Assert.Equal(reason,resultRequest.Reason);
    }

    [Fact]
    public async Task Update_ChangePermissionStatus()
    {
       //Arrange
        using var dbContext = CreateDbContext(nameof(GetAllWorkerById_ShouldReturnPermission));

        var workerId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();
        var originalTimestamp = DateTime.UtcNow.AddSeconds(-1);

        dbContext.Permissions.AddRange( 
        new Permission
        {   
            Id = permissionId,
            WorkerId = workerId,
            LastUpdatedAt = originalTimestamp,
            Status = 0
        }, 
        new Permission
        {   
            Id = Guid.NewGuid(),
            WorkerId = workerId,
            LastUpdatedAt = DateTime.UtcNow,
            Status = 0
        }
        );

        await dbContext.SaveChangesAsync();
        var permissionRepo = new PermissionRepoImpl(dbContext);
        var permissionService = new PermissionServiceImpl(permissionRepo);

       //Act
       var result = await permissionService.UpdateAsync(permissionId, 1);

       //Assert
       Assert.True(result.Status == (PermissionStatus)1);
       Assert.True(result.LastUpdatedAt > originalTimestamp);
    }



}