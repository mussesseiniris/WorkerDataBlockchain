using Moq;
using wdb_backend.Abstractions;
using wdb_backend.Models;
using wdb_backend.Usecases;

namespace wdb_backend.Tests;

public class AddFlexibleWorkerInfoUsecaseTests
{
    private readonly Mock<IWorkerService> _mockWorkerService;
    private readonly Mock<IWorkerInfoService> _mockWorkerInfoService;
    private readonly Mock<IRequestService> _mockRequestService;
    private readonly AddFlexibleWorkerInfoUsecaseImpl _usecase;

    public AddFlexibleWorkerInfoUsecaseTests()
    {
        _mockWorkerService = new Mock<IWorkerService>();
        _mockWorkerInfoService = new Mock<IWorkerInfoService>();
        _mockRequestService = new Mock<IRequestService>();

        _usecase = new AddFlexibleWorkerInfoUsecaseImpl(
            _mockWorkerService.Object,
            _mockWorkerInfoService.Object,
            _mockRequestService.Object
        );
    }

    // ── normal flow ─────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_ValidInput_CreatesWorkerInfoAndRequest()
    {
        // Arrange
        var workerId = Guid.NewGuid();
        var employerId = Guid.NewGuid();
        var workerEmail = "worker@test.com";
        var desc = "LinkedIn Profile";
        var reason = "Background check";

        var worker = new Worker { Id = workerId, Email = workerEmail };

        _mockWorkerService
            .Setup(s => s.GetByEmailAsync(workerEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(worker);

        _mockWorkerInfoService
            .Setup(s => s.CreateAsync(workerId, It.IsAny<WorkerInfo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WorkerInfo { WorkerId = workerId, CustomLabel = desc, Type = "text", Value = null });

        _mockRequestService
            .Setup(s => s.CreateAsync(employerId, workerId, reason, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Request { EmployerId = employerId, WorkerId = workerId, Reason = reason });

        // Act
        await _usecase.ExecuteAsync(workerEmail, "PersonalInformation", desc, reason, employerId);

        // Assert
        _mockWorkerInfoService.Verify(
            s => s.CreateAsync(workerId, It.Is<WorkerInfo>(w =>
                w.CustomLabel == desc &&
                w.Value == null &&
                w.Type == "text" &&
                w.WorkerId == workerId
            ), It.IsAny<CancellationToken>()),
            Times.Once
        );

        _mockRequestService.Verify(
            s => s.CreateAsync(employerId, workerId, reason, null, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    // ── Worker not found ─────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WorkerNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var workerEmail = "notfound@test.com";

        _mockWorkerService
            .Setup(s => s.GetByEmailAsync(workerEmail, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException());

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _usecase.ExecuteAsync(workerEmail, "PersonalInformation", "desc", "reason", Guid.NewGuid())
        );

        // Verify that no WorkerInfo or Request was created
        _mockWorkerInfoService.Verify(
            s => s.CreateAsync(It.IsAny<Guid>(), It.IsAny<WorkerInfo>(), It.IsAny<CancellationToken>()),
            Times.Never
        );

        _mockRequestService.Verify(
            s => s.CreateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    // ── Worker returns null ──────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WorkerServiceReturnsNull_ThrowsException()
    {
        // Arrange
        _mockWorkerService
            .Setup(s => s.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Worker?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(() =>
            _usecase.ExecuteAsync("null@test.com", "PersonalInformation", "desc", "reason", Guid.NewGuid())
        );
    }
}
