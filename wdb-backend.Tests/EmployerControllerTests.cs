
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using wdb_backend.Abstractions;
using wdb_backend.Controllers;
using wdb_backend.Models;
using wdb_backend.DTOs;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Org.BouncyCastle.Asn1.Misc;
using System.ComponentModel;

public class EmployerControllerTests
{

    [Fact]
    public async Task GetWorkerInfosByEmail_ValidEmail_ReturnsWorkerInfoDto()
    {
        // Arrange
        var mockCreateDataUsecase = new Mock<ICreateDataAccessRequestUsecase>();
        var mockFindInfosUsecase = new Mock<IFindWorkerInfosByEmailUsecase>();
        var mockWorkerService = new Mock<IWorkerService>();
        var employerController = new EmployerController(mockCreateDataUsecase.Object, mockFindInfosUsecase.Object, mockWorkerService.Object);
        var email = "test@email";
        var worker_info1 = new WorkerInfo { Desc = "address", Value = "havana", Id = Guid.NewGuid() };
        var worker_info2 = new WorkerInfo { Desc = "phone", Value = "123456", Id = Guid.NewGuid() };
        var workerInfos = new List<WorkerInfo>();
        workerInfos.Add(worker_info1);
        workerInfos.Add(worker_info2);
        mockFindInfosUsecase.Setup(r => r.FindWorkerInfosByEmail(email)).ReturnsAsync(workerInfos);

        var expectedWorkerInfoDtos = new List<WorkerInfoDto>
        {
            new WorkerInfoDto{Desc="address",Id=worker_info1.Id},
            new WorkerInfoDto{Desc="phone",Id=worker_info2.Id}

        };
        // Act
        var okResult = await employerController.GetWorkerInfosByEmail(email);

        // Assert
        var result = Assert.IsType<OkObjectResult>(okResult.Result);
        var resultList = Assert.IsType<List<WorkerInfoDto>>(result.Value);
        Assert.Equal(2, resultList.Count);
        Assert.Equal(worker_info1.Id, resultList[0].Id);
        Assert.Equal("address", resultList[0].Desc);
        Assert.Equal(worker_info2.Id, resultList[1].Id);
        Assert.Equal("phone", resultList[1].Desc);
    }

    [Fact]
    public async Task GetWorkerInfosByEmail_InValidEmail_ReturnsEmptyList()
    {
        // Arrange
        var mockCreateDataUsecase = new Mock<ICreateDataAccessRequestUsecase>();
        var mockFindInfosUsecase = new Mock<IFindWorkerInfosByEmailUsecase>();
        var mockWorkerService = new Mock<IWorkerService>();
        var employerController = new EmployerController(mockCreateDataUsecase.Object, mockFindInfosUsecase.Object, mockWorkerService.Object);
        var email = "test@email";
        var workerInfos = new List<WorkerInfo>();
        mockFindInfosUsecase.Setup(r => r.FindWorkerInfosByEmail(email)).ReturnsAsync(workerInfos);

        // Act
        var Result = await employerController.GetWorkerInfosByEmail(email);

        // Assert
        var result = Assert.IsType<OkObjectResult>(Result.Result);
        var resultList = Assert.IsType<List<WorkerInfoDto>>(result.Value);
        Assert.Empty(resultList);
    }



    [Fact]
    public async Task CreateDataAccessRequest_ValidInput_ReturnsOk()
    {
        // Arrange
        var mockCreateDataUsecase = new Mock<ICreateDataAccessRequestUsecase>();
        var mockFindInfosUsecase = new Mock<IFindWorkerInfosByEmailUsecase>();
        var mockWorkerService = new Mock<IWorkerService>();
        var employerController = new EmployerController(mockCreateDataUsecase.Object, mockFindInfosUsecase.Object, mockWorkerService.Object);
        var employerId = Guid.NewGuid();
        var claims = new List<Claim>
{
    new Claim(JwtRegisteredClaimNames.Sub, employerId.ToString())
};
        employerController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims))
            }
        };

        var email = "test@email";
        var fakeRequest = new Request { EmployerId = employerId, WorkerId = Guid.NewGuid(), Reason = "check basic info" };
        var worker_info1 = new WorkerInfo { Desc = "address", Value = "havana rise", WorkerId = fakeRequest.WorkerId };
        var worker_info2 = new WorkerInfo { Desc = "phone", Value = "123456", WorkerId = fakeRequest.WorkerId };
        var workerInfos = new List<WorkerInfo>();
        workerInfos.Add(worker_info1);
        workerInfos.Add(worker_info2);
        var infoDesc = new List<string> { worker_info1.Id.ToString(), worker_info2.Id.ToString() };
        var CreateRequestDTO = new CreateRequestUsecaseDTO { Email = email, InfoDesc = infoDesc, Reason = fakeRequest.Reason };
        mockFindInfosUsecase.Setup(r => r.FindWorkerInfosByEmail(email)).ReturnsAsync(workerInfos);
        mockCreateDataUsecase.Setup(r => r.CreateDataAccessRequest(workerInfos, fakeRequest.EmployerId, fakeRequest.WorkerId, fakeRequest.Reason)).Returns(Task.CompletedTask);

        var fakeRequestDTO = new CreateRequestUsecaseDTO { Email = email, InfoDesc = infoDesc, Reason = fakeRequest.Reason };

        // Act
        var okResult = await employerController.CreateRequest(fakeRequestDTO);

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
        var mockWorkerService = new Mock<IWorkerService>();
        var employerController = new EmployerController(mockCreateDataUsecase.Object, mockFindInfosUsecase.Object, mockWorkerService.Object);

        var employerId = Guid.NewGuid();
        var claims = new List<Claim>
{
    new Claim(JwtRegisteredClaimNames.Sub, employerId.ToString())
};
        employerController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims))
            }
        };

        var email = "test@email";
        var infoDesc = new List<string>();
        var workerInfos = new List<WorkerInfo>();
        mockFindInfosUsecase.Setup(r => r.FindWorkerInfosByEmail(email, default)).ReturnsAsync(workerInfos);
        var reason = "check the basic info";
        var fakeRequestDTO = new CreateRequestUsecaseDTO { Email = email, InfoDesc = infoDesc, Reason = reason };

        // Act
        var Result = await employerController.CreateRequest(fakeRequestDTO);

        // Assert
        Assert.IsType<NotFoundResult>(Result);
    }
}
