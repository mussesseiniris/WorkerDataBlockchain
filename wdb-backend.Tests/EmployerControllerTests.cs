
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using wdb_backend.Abstractions;
using wdb_backend.Controllers;
using wdb_backend.Models;
using wdb_backend.DTOs;

public class EmployerControllerTests
{

    [Fact]
    public async Task GetWorkerInfosByEmail_ValidEmail_ReturnsWorkerInfos()
    {
        // Arrange
        var mockCreateDataUsecase = new Mock<ICreateDataAccessRequestUsecase>();
        var mockFindInfosUsecase = new Mock<IFindWorkerInfosByEmailUsecase>();
        var employerController = new EmployerController(mockCreateDataUsecase.Object, mockFindInfosUsecase.Object);
        var email = "test@email";
        var worker_info1 = new WorkerInfo { Desc = "address", Value = "havana rise" };
        var worker_info2 = new WorkerInfo { Desc = "phone", Value = "123456" };
        var workerInfos = new List<WorkerInfo>();
        workerInfos.Add(worker_info1);
        workerInfos.Add(worker_info2);
        mockFindInfosUsecase.Setup(r => r.FindWorkerInfosByEmail(email)).ReturnsAsync(workerInfos);
        // Act
        var okResult = await employerController.GetWorkerInfosByEmail(email);

        // Assert
        var result = Assert.IsType<OkObjectResult>(okResult.Result);
        Assert.Equal(workerInfos, result.Value);
    }

    [Fact]
    public async Task GetWorkerInfosByEmail_InValidEmail_Returns404()
    {
        // Arrange
        var mockCreateDataUsecase = new Mock<ICreateDataAccessRequestUsecase>();
        var mockFindInfosUsecase = new Mock<IFindWorkerInfosByEmailUsecase>();
        var employerController = new EmployerController(mockCreateDataUsecase.Object, mockFindInfosUsecase.Object);
        var email = "test@email";
        var workerInfos = new List<WorkerInfo>();
        mockFindInfosUsecase.Setup(r => r.FindWorkerInfosByEmail(email)).ReturnsAsync(workerInfos);

        // Act
        var Result = await employerController.GetWorkerInfosByEmail(email);

        // Assert
        Assert.IsType<NotFoundResult>(Result.Result);
    }



    [Fact]
    public async Task CreateDataAccessRequest_ValidInput_ReturnsOk()
    {
        // Arrange
        var mockCreateDataUsecase = new Mock<ICreateDataAccessRequestUsecase>();
        var mockFindInfosUsecase = new Mock<IFindWorkerInfosByEmailUsecase>();
        var employerController = new EmployerController(mockCreateDataUsecase.Object, mockFindInfosUsecase.Object);
        var email = "test@email";
        var infoDesc = new List<string>();
        infoDesc.Add("Address");
        infoDesc.Add("Phone");
        var fakeRequest = new Request { EmployerId = Guid.NewGuid(), WorkerId = Guid.NewGuid(), Reason = "check basic info" };
        var CreateRequestDTO = new CreateRequestUsecaseDTO { Email = email, InfoDesc = infoDesc, Reason = fakeRequest.Reason };
        var worker_info1 = new WorkerInfo { Desc = "address", Value = "havana rise", WorkerId = fakeRequest.WorkerId };
        var worker_info2 = new WorkerInfo { Desc = "phone", Value = "123456", WorkerId = fakeRequest.WorkerId };
        var workerInfos = new List<WorkerInfo>();
        workerInfos.Add(worker_info1);
        workerInfos.Add(worker_info2);
        mockFindInfosUsecase.Setup(r => r.FindWorkerInfosByEmail(email, default)).ReturnsAsync(workerInfos);
        mockCreateDataUsecase.Setup(r => r.CreateDataAccessRequest(workerInfos, fakeRequest.EmployerId, fakeRequest.WorkerId, fakeRequest.Reason)).Returns(Task.CompletedTask);

        // Act
        var okResult = await employerController.CreateRequest(email, infoDesc, fakeRequest.Reason, fakeRequest.EmployerId);

        // Assert
        Assert.IsType<OkResult>(okResult);
        mockCreateDataUsecase.Verify(r => r.CreateDataAccessRequest(workerInfos, fakeRequest.EmployerId, fakeRequest.WorkerId, fakeRequest.Reason), Times.Once);
    }

    [Fact]
    public async Task CreateDataAccessRequest_EmptyWorkerInfos_Returns404()
    {
        // Arrange
        var mockCreateDataUsecase = new Mock<ICreateDataAccessRequestUsecase>();
        var mockFindInfosUsecase = new Mock<IFindWorkerInfosByEmailUsecase>();
        var employerController = new EmployerController(mockCreateDataUsecase.Object, mockFindInfosUsecase.Object);
        var email = "test@email";
        var infoDesc = new List<string>();
        infoDesc.Add("Address");
        infoDesc.Add("Phone");
        var fakeRequest = new Request { EmployerId = Guid.NewGuid(), WorkerId = Guid.NewGuid(), Reason = "check basic info" };
        var CreateRequestDTO = new CreateRequestUsecaseDTO { Email = email, InfoDesc = infoDesc, Reason = fakeRequest.Reason };
        var workerInfos = new List<WorkerInfo>();
        mockFindInfosUsecase.Setup(r => r.FindWorkerInfosByEmail(email, default)).ReturnsAsync(workerInfos);
        mockCreateDataUsecase.Setup(r => r.CreateDataAccessRequest(workerInfos, fakeRequest.EmployerId, fakeRequest.WorkerId, fakeRequest.Reason)).Returns(Task.CompletedTask);

        // Act
        var Result = await employerController.CreateRequest(email, infoDesc, fakeRequest.Reason, fakeRequest.EmployerId);

        // Assert
        Assert.IsType<NotFoundResult>(Result);
    }
}
