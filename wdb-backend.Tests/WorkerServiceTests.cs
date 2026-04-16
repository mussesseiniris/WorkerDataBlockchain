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
            new Request{Id = requestId, EmployerId = employerId, Reason = "Reason 1"}
        };
       
        

        
        mockPermissionRepo.Setup(r => r.GetAllByWorkerIdAsync(workerId, default)).ReturnsAsync(fakePermissions);
        mockRequestRepo.Setup(r => r.GetAllByWorkerIdAsync(workerId, default)).ReturnsAsync(fakeRequests);

        // Act: 
        //Get permission based on workerid and filter to pending status
        var permissionResult = await permissionService.GetAllByWorkerIdAsync(workerId, 0);
        
        //Get requets using request id from permission rows 
        var requestResult = await requestService.GetAllByRequestIdAsync(workerId, requestId);

        
        // Assert: the returned data is a list of permissions/request
        var returnedPermissions = Assert.IsType<List<Permission>>(permissionResult);
        var returnedRequests = Assert.IsType<List<Request>>(requestResult);
        
        // Assert: the correct number of permission retrieved
        Assert.Equal(2, returnedPermissions.Count);
        
        // Assert: the permission status of rows is pending
        Assert.All(returnedPermissions, returnedPermission => Assert.True(returnedPermission.Status == 0));
        
        // Assert: requests have correct requestId
        Assert.All(returnedRequests, returnedRequest => Assert.True(returnedRequest.Id == requestId));
       
       // Assert: correct number of requests retrieved
       Assert.Equal(2, returnedRequests.Count);

    }
}