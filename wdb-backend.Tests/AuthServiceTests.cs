using Moq;
using wdb_backend.Abstractions;
using wdb_backend.DTOs;
using wdb_backend.Models;
using wdb_backend.Services;

namespace wdb_backend.Tests;

public class AuthServiceTests
{
    // create mocks for dependencies
    private readonly Mock<IUserRepository<Worker>> _mockRepo;
    private readonly Mock<IPasswordHasher> _mockHasher;
    private readonly Mock<IJwtTokenService> _mockJwt;
    private readonly AuthService<Worker> _authService;

    // initialize instances
    public AuthServiceTests()
    {
        _mockRepo = new Mock<IUserRepository<Worker>>();
        _mockHasher = new Mock<IPasswordHasher>();
        _mockJwt = new Mock<IJwtTokenService>();
        _authService = new AuthService<Worker>(_mockRepo.Object, _mockHasher.Object, _mockJwt.Object);
    }

    // RegisterAsync Tests

    // Test: register should succeed with valid input
    [Fact]
    public async Task RegisterAsync_ValidInput_ReturnsSuccess()
    {
        // Arrange - set up what the mocks should return
        _mockRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _mockHasher.Setup(h => h.HashPassword(It.IsAny<string>())).Returns("hashed_password");

        // create the request
        var request = new RegisterRequest("test@example.com", "TestUser", "Password123");

        // Act - call the method
        var (success, message) = await _authService.RegisterAsync(request);

        // Assert - verify the result
        Assert.True(success);
        Assert.Equal(string.Empty, message);

        // Verify AddAsync was called once
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Worker>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // Test: register should fail if email already exists
    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ReturnsFail()
    {
        // Arrange — email already exists
        _mockRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var request = new RegisterRequest("test@example.com", "TestUser", "Password123");

        // Act
        var (success, message) = await _authService.RegisterAsync(request);

        // Assert
        Assert.False(success);
        Assert.Equal("Email already exists.", message);

        // Verify AddAsync was never called
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Worker>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_EmptyEmail_ReturnsFail()
    {
        var request = new RegisterRequest("", "TestUser", "Password123");

        var (success, message) = await _authService.RegisterAsync(request);

        Assert.False(success);
        Assert.Equal("Invalid Input.", message);
    }

    [Fact]
    public async Task RegisterAsync_EmptyUserName_ReturnsFail()
    {
        var request = new RegisterRequest("test@example.com", "", "Password123");

        var (success, message) = await _authService.RegisterAsync(request);

        Assert.False(success);
        Assert.Equal("Invalid Input.", message);
    }

    [Fact]
    public async Task RegisterAsync_EmptyPassword_ReturnsFail()
    {
        var request = new RegisterRequest("test@example.com", "TestUser", "");

        var (success, message) = await _authService.RegisterAsync(request);

        Assert.False(success);
        Assert.Equal("Invalid Input.", message);
    }

    [Fact]
    public async Task RegisterAsync_HashesPasswordBeforeSaving()
    {
        _mockRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _mockHasher.Setup(h => h.HashPassword("Password123")).Returns("hashed_password");

        var request = new RegisterRequest("test@example.com", "TestUser", "Password123");

        await _authService.RegisterAsync(request);

        _mockRepo.Verify(r => r.AddAsync(
            It.Is<Worker>(w => w.Password == "hashed_password"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // LoginAsync Tests

      [Fact]
      public async Task LoginAsync_ValidCredentials_ReturnsSuccess()
      {
          var worker = new Worker { Name = "TestUser", Email = "test@example.com", Password = "hashed_password" };

          _mockRepo.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
                   .ReturnsAsync(worker);
          _mockHasher.Setup(h => h.VerifyPassword("Password123", "hashed_password"))
                     .Returns(true);
          _mockJwt.Setup(j => j.GenerateAccessToken(worker))
                  .Returns("fake_jwt_token");

          var request = new LoginRequest("test@example.com", "Password123");

          var (success, message, result) = await _authService.LoginAsync(request);

          Assert.True(success);
          Assert.Equal("Login Successful.", message);
          Assert.NotNull(result);
          Assert.Equal("fake_jwt_token", result.AccessToken);
          Assert.Equal("TestUser", result.UserName);
          Assert.Equal("test@example.com", result.Email);
      }

      [Fact]
      public async Task LoginAsync_UserNotFound_ReturnsFail()
      {
          _mockRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync((Worker?)null);

          var request = new LoginRequest("unknown@example.com", "Password123");

          var (success, message, result) = await _authService.LoginAsync(request);

          Assert.False(success);
          Assert.Equal("User not found.", message);
          Assert.Null(result);
      }

      [Fact]
      public async Task LoginAsync_WrongPassword_ReturnsFail()
      {
          var worker = new Worker { Name = "TestUser", Email = "test@example.com", Password = "hashed_password" };

          _mockRepo.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
                   .ReturnsAsync(worker);
          _mockHasher.Setup(h => h.VerifyPassword("wrongpassword", "hashed_password"))
                     .Returns(false);

          var request = new LoginRequest("test@example.com", "wrongpassword");

          var (success, message, result) = await _authService.LoginAsync(request);

          Assert.False(success);
          Assert.Equal("Incorrect password.", message);
          Assert.Null(result);
      }

      [Fact]
      public async Task LoginAsync_EmptyEmail_ReturnsFail()
      {
          var request = new LoginRequest("", "Password123");

          var (success, message, result) = await _authService.LoginAsync(request);

          Assert.False(success);
          Assert.Equal("Invalid email or password.", message);
          Assert.Null(result);
      }

      [Fact]
      public async Task LoginAsync_EmptyPassword_ReturnsFail()
      {
          var request = new LoginRequest("test@example.com", "");

          var (success, message, result) = await _authService.LoginAsync(request);

          Assert.False(success);
          Assert.Equal("Invalid email or password.", message);
          Assert.Null(result);
      }
}
