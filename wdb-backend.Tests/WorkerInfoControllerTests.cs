using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Moq;
using wdb_backend.Abstractions;
using wdb_backend.Controllers;
using wdb_backend.DTOs;
using wdb_backend.Models;

public class WorkerInfoControllerTests
{
    private readonly Mock<IWorkerInfoService> _mockService;
    private readonly Mock<ISupabaseStorageService> _mockStorage;
    private readonly WorkerInfoController _controller;
    private readonly Guid _workerId = Guid.NewGuid();

    public WorkerInfoControllerTests()
    {
        _mockService = new Mock<IWorkerInfoService>();
        _mockStorage = new Mock<ISupabaseStorageService>();
        _controller = new WorkerInfoController(_mockService.Object, _mockStorage.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, _workerId.ToString()),
        }, "mock"));

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
    }

    // ── GetProfile ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetProfile_ReturnsOk_WithGroupedFields()
    {
        // Arrange
        var fields = new List<WorkerInfo>
        {
            new WorkerInfo
            {
                Id = Guid.NewGuid(),
                FieldId = Guid.NewGuid(),
                Value = "John",
                Type = "text",
                Field = new Field
                {
                    FieldName = "Full Name",
                    AllowedType = "text",
                    Label = "Full Name",
                    Category = new Category { CategoryName = "PersonalInfo" }
                }
            }
        };
        _mockService.Setup(s => s.GetAllWithPresetsAsync(_workerId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(fields);

        // Act
        var result = await _controller.GetProfile(CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var categories = Assert.IsType<List<WorkerProfileCategoryDto>>(ok.Value);
        Assert.Single(categories);
        Assert.Equal("PersonalInfo", categories[0].Category);
    }

    // ── UpdatePreset ──────────────────────────────────────────────────────

    [Fact]
    public async Task UpdatePreset_ReturnsOk_WhenValid()
    {
        // Arrange
        var fieldId = Guid.NewGuid();
        var updated = new WorkerInfo
        {
            Id = Guid.NewGuid(),
            FieldId = fieldId,
            Value = "Updated",
            Type = "text"
        };
        _mockService.Setup(s => s.UpdateAsync(_workerId, It.IsAny<WorkerInfo>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(updated);

        var request = new UpdatePresetFieldRequest { FieldId = fieldId, Value = "Updated" };

        // Act
        var result = await _controller.UpdatePreset(request, CancellationToken.None);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task UpdatePreset_ReturnsBadRequest_WhenFieldIdIsEmpty()
    {
        // Arrange
        var request = new UpdatePresetFieldRequest { FieldId = Guid.Empty, Value = "test" };

        // Act
        var result = await _controller.UpdatePreset(request, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdatePreset_ReturnsNotFound_WhenFieldNotFound()
    {
        // Arrange
        var request = new UpdatePresetFieldRequest { FieldId = Guid.NewGuid(), Value = "test" };
        _mockService.Setup(s => s.UpdateAsync(_workerId, It.IsAny<WorkerInfo>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new KeyNotFoundException("FIELD_NOT_FOUND"));

        // Act
        var result = await _controller.UpdatePreset(request, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ── CreateCustom ──────────────────────────────────────────────────────

    [Fact]
    public async Task CreateCustom_ReturnsOk_WhenValid()
    {
        // Arrange
        var created = new WorkerInfo
        {
            Id = Guid.NewGuid(),
            CustomLabel = "My Field",
            Type = "text",
            Value = "Some value"
        };
        _mockService.Setup(s => s.CreateAsync(_workerId, It.IsAny<WorkerInfo>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(created);

        var request = new CreateCustomFieldRequest { Label = "My Field", Type = "text", Value = "Some value" };

        // Act
        var result = await _controller.CreateCustom(request, CancellationToken.None);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task CreateCustom_ReturnsBadRequest_WhenTypeIsInvalid()
    {
        // Arrange
        var request = new CreateCustomFieldRequest { Label = "My Field", Type = "invalid", Value = "val" };

        // Act
        var result = await _controller.CreateCustom(request, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateCustom_ReturnsConflict_WhenLabelExists()
    {
        // Arrange
        _mockService.Setup(s => s.CreateAsync(_workerId, It.IsAny<WorkerInfo>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new InvalidOperationException("CUSTOM_LABEL_EXISTS"));

        var request = new CreateCustomFieldRequest { Label = "Existing", Type = "text", Value = "val" };

        // Act
        var result = await _controller.CreateCustom(request, CancellationToken.None);

        // Assert
        Assert.IsType<ConflictObjectResult>(result);
    }

    // ── DeleteCustom ──────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteCustom_ReturnsNoContent_WhenSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.DeleteAsync(_workerId, id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new WorkerInfo { Type = "text" });

        // Act
        var result = await _controller.DeleteCustom(id, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteCustom_ReturnsConflict_WhenActivePermissionExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.DeleteAsync(_workerId, id, It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new InvalidOperationException("ACTIVE_PERMISSION_EXISTS"));

        // Act
        var result = await _controller.DeleteCustom(id, CancellationToken.None);

        // Assert
        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task DeleteCustom_ReturnsBadRequest_WhenPresetField()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.DeleteAsync(_workerId, id, It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new InvalidOperationException("PRESET_FIELD_CANNOT_BE_DELETED"));

        // Act
        var result = await _controller.DeleteCustom(id, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ── GetAll (legacy) ───────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsOk_WithHashSet()
    {
        // Arrange
        var data = new HashSet<WorkerInfo>
        {
            new WorkerInfo { Id = Guid.NewGuid(), Type = "text", Value = "test" }
        };
        _mockService.Setup(s => s.GetAllAsyncHash(_workerId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(data);

        // Act
        var result = await _controller.GetAll(CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<HashSet<WorkerInfo>>(ok.Value);
    }
}
