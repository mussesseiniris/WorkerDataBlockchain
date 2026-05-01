using System.Security.Cryptography.X509Certificates;
using Moq;
using wdb_backend.Abstractions;
using wdb_backend.Models;
using wdb_backend.Services;

public class RequestServiceTests
{
    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsRequest()
    {
        //Arange
        var mockRepo = new Mock<IRequestRepository>();
        var service = new RequestServiceImpl(mockRepo.Object);
        var employerId = Guid.NewGuid();
        var workerId = Guid.NewGuid();
        var reason = "check the age";
        var fakeRequest = new Request { Id = Guid.NewGuid(), EmployerId = employerId, WorkerId = workerId, Reason = reason };
        mockRepo.Setup(r => r.AddAsync(employerId, workerId, reason, default)).ReturnsAsync(fakeRequest);

        //Act
        var result = await service.CreateAsync(employerId, workerId, reason);

        //Assert
        Assert.Equal(fakeRequest.Reason, result.Reason);

    }

    [Fact]
    public async Task CreateAsync_RepoReturnsNull_ThrowsKeyNotFoundException()
    {
        //Arange
        var mockRepo = new Mock<IRequestRepository>();
        var service = new RequestServiceImpl(mockRepo.Object);
        var reason = "check the age";

        // Act & Assert 
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CreateAsync(Guid.NewGuid(), Guid.NewGuid(), reason));
    }

    [Fact]
public async Task GetAllByEmployerIdAsync_validEmployer_ReturnsAllRequests()
{
    // Arrange
    var mockRepo = new Mock<IRequestRepository>();
    var requestService = new RequestServiceImpl(mockRepo.Object);
    var employerId = Guid.NewGuid();
    var requests = new List<Request>();
    var workerId = Guid.NewGuid();
    var request1= new Request {Id=Guid.NewGuid(),Reason="check basic details",EmployerId=employerId,WorkerId=workerId};
    requests.Add(request1);
    mockRepo.Setup(r=>r.GetAllByEmployerIdAsync(employerId)).ReturnsAsync(requests);
    // Act
    var resultRequests = await requestService.GetAllByEmployerIdAsync(employerId);

    // Assert
    Assert.NotNull(resultRequests);
    Assert.Single(resultRequests);
    Assert.Equal(request1.Id,resultRequests[0].Id);
}

}
