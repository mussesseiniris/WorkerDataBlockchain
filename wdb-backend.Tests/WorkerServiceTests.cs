using Moq;
using wdb_backend.Models;
using wdb_backend.Services;
using wdb_backend.Abstractions;


public class WorkerServiceTests
{

    [Fact]
    public async Task GetByEmailAsync_EmailExists_ReturnsWorker()
    {
        // Arrange - prepare data
        var mockRepo = new Mock<IWorkerRepository>();
        var service = new WorkerServiceImpl(mockRepo.Object);
        var fakeWorker = new Worker { Name = "test1", Email = "test1@email" };
        mockRepo.Setup(r => r.GetByEmailAsync("test1@email", default)).ReturnsAsync(fakeWorker);
        // Act - call method
        var result = await service.GetByEmailAsync("test1@email");
        // Assert - check the result
        Assert.Equal(fakeWorker.Email, result.Email);
    }
    
    [Fact]
    public async Task GetByEmailAsync_EmailNotExists_ThrowsException()
    {
        // Arrange - prepare data
        var mockRepo = new Mock<IWorkerRepository>();
        var service = new WorkerServiceImpl(mockRepo.Object);

        // Act - call method & Assert - check the result
        await Assert.ThrowsAsync<KeyNotFoundException>(()=>service.GetByEmailAsync("noExists@email"));
    }
  
}
