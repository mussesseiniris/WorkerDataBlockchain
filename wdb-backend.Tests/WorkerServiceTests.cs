using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using wdb_backend.Models;
using wdb_backend.Services;
using wdb_backend.Abstractions;
using System.Security;
using wdb_backend.Common;
namespace wdb_backend.Tests;

public class WorkerServiceTests
{
    [Fact]
    async public Task GetPermissionsTest()
    {
        // Arrange: I have no idea how to set the layers up
        var mockPermissionRepo = new Mock<IPermissionRepository>();
        var permissionService = new PermissionServiceImpl(mockPermissionRepo.Object);

        // var requestService = new RequestServiceImpl();
        //add worker
        var workerId = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        //permissions
        var fakePermissions = new List<Permission>
        {
            new Permission {Id = Guid.NewGuid(), WorkerId = workerId, Status = 0, RequestId = requestId},
            new Permission {Id = Guid.NewGuid(), WorkerId = workerId, Status = 0, RequestId = requestId},
            new Permission {Id = Guid.NewGuid(), WorkerId = workerId, Status = (PermissionStatus)1, RequestId = requestId}

        };
        //requests

        
        mockPermissionRepo.Setup(r => r.GetAllByWorkerIdAsync(workerId, default)).ReturnsAsync(fakePermissions);

        // Act: 
        //Get permission based on workerid and filter to pending status
        var permissionResult = await permissionService.GetAllByWorkerIdAsync(workerId, 0);
        
        //Get requets using request id from permission rows 
        // var requestResult = await requestService.GetAllByWorkerIdAsync(workerId);

        
        // Assert: the returned data is a list of permissions
        var returnedPermissions = Assert.IsType<List<Permission>>(permissionResult);
        // var returnedRequests = Assert.IsType<List<Request>>(requestResult);
        
        // Assert: the correct number of permission retrieved
        Assert.Equal(2, returnedPermissions.Count);
        
        // Assert: the permission status of rows is pending
        Assert.All(returnedPermissions, returnedPermission => Assert.True(returnedPermission.Status == 0));
        
        // Assert: a row of permission has correct workerid, employerid, expiry date, request reason, infoid


    }
}