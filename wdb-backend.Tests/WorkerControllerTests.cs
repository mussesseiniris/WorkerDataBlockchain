using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wdb_backend.Controllers;
using wdb_backend.Data;
using wdb_backend.Models;
namespace wdb_backend.Tests;

/// <summary>
/// Unit tests for WorkerController.
/// Tests API endpoints for creating and managing worker records.
/// </summary>

public class WorkerControllerTests
{
    [Fact]
    async public Task WorkerControllerAddTest()
    {
        // Arrange: Set up in-memory database, controller and test data
        var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(databaseName:"TestDatabase")
        .Options;

        var context = new AppDbContext(options);
        var controller = new WorkerController(context);
        var worker = new Worker{Name="Iris",Email="test@email"};
        
        // Act: Call the AddWorker method
        var result = await controller.AddWorker(worker);
        
        // Assert: Verify the response is 200 OK with correct worker data
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedWorker = Assert.IsType<Worker>(okResult.Value);
        Assert.Equal("Iris",returnedWorker.Name);
        Assert.Equal("test@email",returnedWorker.Email);

    }

    async public Task WorkerControllerGetRequestsTest()
    {
        // Arrange: Set up in-memory database, controller and test data
        var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(databaseName:"TestDatabase")
        .Options;

        var context = new AppDbContext(options);
        var controller = new WorkerController(context);
        var worker = new Worker{Name="Iris",Email="test@email"};

        // Act: Call the GetRequests method
        var result = await controller.GetRequests(worker);

        // Assert: Verify the response is 200 OK with correct worker data
        //Test the correct number of requests retrieved
        //Test the permission status of rows is request
        //Test a row of request has correct workerid, employerid, expiry date, request reason, infoid
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedWorker = Assert.IsType<Worker>(okResult.Value);
        Assert.Equal("Iris",returnedWorker.Name);
        Assert.Equal("test@email",returnedWorker.Email);

    }
}
