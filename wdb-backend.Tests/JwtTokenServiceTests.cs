using System.IdentityModel.Tokens.Jwt;
  using Microsoft.Extensions.Options;
  using Moq;
  using wdb_backend.Abstractions;
  using wdb_backend.Services;

  namespace wdb_backend.Tests;

  public class JwtTokenServiceTests
  {
      private readonly JwtTokenServiceImpl _jwtService;

      public JwtTokenServiceTests()
      {
          var options = Options.Create(new JwtOptions
          {
              Issuer = "TestIssuer",
              Audience = "TestAudience",
              Key = "ThisIsATestSecretKeyThatIsAtLeast32Chars!",
              AccessTokenMinutes = 60
          });
          _jwtService = new JwtTokenServiceImpl(options);
      }

      [Fact]
      public void GenerateAccessToken_ReturnsNonEmptyString()
      {
          var mockUser = new Mock<IUser>();
          mockUser.Setup(u => u.Id).Returns(Guid.NewGuid());
          mockUser.Setup(u => u.Email).Returns("test@example.com");
          mockUser.Setup(u => u.Name).Returns("TestUser");

          var token = _jwtService.GenerateAccessToken(mockUser.Object);

          Assert.False(string.IsNullOrEmpty(token));
      }

      [Fact]
      public void GenerateAccessToken_ContainsCorrectClaims()
      {
          var userId = Guid.NewGuid();
          var mockUser = new Mock<IUser>();
          mockUser.Setup(u => u.Id).Returns(userId);
          mockUser.Setup(u => u.Email).Returns("test@example.com");
          mockUser.Setup(u => u.Name).Returns("TestUser");

          var token = _jwtService.GenerateAccessToken(mockUser.Object);

          var handler = new JwtSecurityTokenHandler();
          var jwt = handler.ReadJwtToken(token);

          Assert.Equal(userId.ToString(), jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
          Assert.Equal("test@example.com", jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
          Assert.Equal("TestUser", jwt.Claims.First(c => c.Type == "username").Value);
      }

      [Fact]
      public void GenerateAccessToken_HasCorrectIssuerAndAudience()
      {
          var mockUser = new Mock<IUser>();
          mockUser.Setup(u => u.Id).Returns(Guid.NewGuid());
          mockUser.Setup(u => u.Email).Returns("test@example.com");
          mockUser.Setup(u => u.Name).Returns("TestUser");

          var token = _jwtService.GenerateAccessToken(mockUser.Object);

          var handler = new JwtSecurityTokenHandler();
          var jwt = handler.ReadJwtToken(token);

          Assert.Equal("TestIssuer", jwt.Issuer);
          Assert.Contains("TestAudience", jwt.Audiences);
      }

      [Fact]
      public void GenerateAccessToken_ExpiresInFuture()
      {
          var mockUser = new Mock<IUser>();
          mockUser.Setup(u => u.Id).Returns(Guid.NewGuid());
          mockUser.Setup(u => u.Email).Returns("test@example.com");
          mockUser.Setup(u => u.Name).Returns("TestUser");

          var token = _jwtService.GenerateAccessToken(mockUser.Object);

          var handler = new JwtSecurityTokenHandler();
          var jwt = handler.ReadJwtToken(token);

          Assert.True(jwt.ValidTo > DateTime.UtcNow);
      }
  }
