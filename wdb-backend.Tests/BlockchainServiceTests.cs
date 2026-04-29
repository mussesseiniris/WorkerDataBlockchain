using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using wdb_backend.Abstractions;
using wdb_backend.Models;
using wdb_backend.Services;
using Xunit;

namespace wdb_backend.Tests;

/// <summary>
/// Unit tests for BlockchainService.
/// GenerateKeyPair uses real BlockchainService — pure math, no node needed.
/// LogTransactionAsync uses mocked IBlockchainService — no Hardhat node needed.
/// </summary>
public class BlockchainServiceTests
{
    // ─────────────────────────────────────────────────────
    // Dummy test data
    // ─────────────────────────────────────────────────────
    private const string EmployerAddress = "0xf39Fd6e51aad88F6F4ce6aB8827279cffFb92266";
    private const string WorkerAddress = "0x70997970C51812dc3A010C7d01b50e0d17dc79C8";
    private const string PrivateKey = "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80";
    private const string FakeTxHash = "0x111abc222def333ghi444jkl555mno666pqr777stu888vwx999yz0000000001";

    // ─────────────────────────────────────────────────────
    // Creates real BlockchainService with fake config
    // ─────────────────────────────────────────────────────
    private BlockchainService CreateRealService()
    {
        var inMemoryConfig = new Dictionary<string, string?>
        {
            { "Blockchain:RpcUrl",          "http://127.0.0.1:8545"   },
            { "Blockchain:ContractAddress", "0xCf7Ed3..."              },
            { "Blockchain:AbiPath",         "fake/path/does/not/exist" }
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfig)
            .Build();

        var logger = new Mock<ILogger<BlockchainService>>().Object;

        return new BlockchainService(config, logger);
    }

    // ─────────────────────────────────────────────────────
    // Creates mock IBlockchainService
    // ─────────────────────────────────────────────────────
    private Mock<IBlockchainService> CreateMockService()
    {
        var mock = new Mock<IBlockchainService>();

        mock.Setup(s => s.LogTransactionAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<BlockchainAction>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(FakeTxHash);

        return mock;
    }

    // ─────────────────────────────────────────────────────
    // GenerateKeyPair Tests
    // Pure math — no Hardhat node needed
    // ─────────────────────────────────────────────────────
    public class GenerateKeyPairTests : BlockchainServiceTests
    {
        [Fact]
        public void GenerateKeyPair_ShouldReturnNonEmptyPrivateKey()
        {
            // Arrange
            var service = CreateRealService();

            // Act
            var keyPair = service.GenerateKeyPair();

            // Assert
            Assert.NotEmpty(keyPair.PrivateKey);
        }

        [Fact]
        public void GenerateKeyPair_ShouldReturnNonEmptyBlockchainAddress()
        {
            // Arrange
            var service = CreateRealService();

            // Act
            var keyPair = service.GenerateKeyPair();

            // Assert
            Assert.NotEmpty(keyPair.BlockchainAddress);
        }

        [Fact]
        public void GenerateKeyPair_AddressShouldStartWith0x()
        {
            // Arrange
            var service = CreateRealService();

            // Act
            var keyPair = service.GenerateKeyPair();

            // Assert
            Assert.StartsWith("0x", keyPair.BlockchainAddress);
        }

        [Fact]
        public void GenerateKeyPair_EachCallShouldReturnUniqueKeyPair()
        {
            // Arrange
            var service = CreateRealService();

            // Act
            var keyPair1 = service.GenerateKeyPair();
            var keyPair2 = service.GenerateKeyPair();

            // Assert — two generated keys should never be the same
            Assert.NotEqual(keyPair1.PrivateKey, keyPair2.PrivateKey);
            Assert.NotEqual(keyPair1.BlockchainAddress, keyPair2.BlockchainAddress);
        }

        [Fact]
        public void GenerateKeyPair_ShouldReturnBlockchainKeyPairType()
        {
            // Arrange
            var service = CreateRealService();

            // Act
            var keyPair = service.GenerateKeyPair();

            // Assert
            Assert.IsType<BlockchainKeyPair>(keyPair);
        }
    }

    // ─────────────────────────────────────────────────────
    // Employer Scenario Tests
    // ─────────────────────────────────────────────────────
    public class EmployerScenarioTests : BlockchainServiceTests
    {
        [Fact]
        public async Task LogTransactionAsync_WhenEmployerSendsRequest_ShouldReturnTxHash()
        {
            // Arrange
            var mockService = CreateMockService();

            // Act — employer requests permission from worker
            var txHash = await mockService.Object.LogTransactionAsync(
                privateKey: PrivateKey,
                employerAddress: EmployerAddress,
                workerAddress: WorkerAddress,
                action: BlockchainAction.PermissionRequested
            );

            // Assert
            Assert.NotEmpty(txHash);
            Assert.StartsWith("0x", txHash);

            // verify called once with correct action
            mockService.Verify(s => s.LogTransactionAsync(
                PrivateKey,
                EmployerAddress,
                WorkerAddress,
                BlockchainAction.PermissionRequested,
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Fact]
        public async Task LogTransactionAsync_WhenEmployerViewsData_ShouldReturnTxHash()
        {
            // Arrange
            var mockService = CreateMockService();

            // Act — employer views worker data
            var txHash = await mockService.Object.LogTransactionAsync(
                privateKey: PrivateKey,
                employerAddress: EmployerAddress,
                workerAddress: WorkerAddress,
                action: BlockchainAction.DataViewed
            );

            // Assert
            Assert.NotEmpty(txHash);
            Assert.StartsWith("0x", txHash);

            // verify called once with correct action
            mockService.Verify(s => s.LogTransactionAsync(
                PrivateKey,
                EmployerAddress,
                WorkerAddress,
                BlockchainAction.DataViewed,
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }
    }

    // ─────────────────────────────────────────────────────
    // Worker Scenario Tests
    // ─────────────────────────────────────────────────────
    public class WorkerScenarioTests : BlockchainServiceTests
    {
        [Fact]
        public async Task LogTransactionAsync_WhenWorkerApprovesRequest_ShouldReturnTxHash()
        {
            // Arrange
            var mockService = CreateMockService();

            // Act — worker approves employer permission request
            var txHash = await mockService.Object.LogTransactionAsync(
                privateKey: PrivateKey,
                employerAddress: EmployerAddress,
                workerAddress: WorkerAddress,
                action: BlockchainAction.PermissionApproved
            );

            // Assert
            Assert.NotEmpty(txHash);
            Assert.StartsWith("0x", txHash);

            // verify called once with correct action
            mockService.Verify(s => s.LogTransactionAsync(
                PrivateKey,
                EmployerAddress,
                WorkerAddress,
                BlockchainAction.PermissionApproved,
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Fact]
        public async Task LogTransactionAsync_WhenWorkerRevokesAccess_ShouldReturnTxHash()
        {
            // Arrange
            var mockService = CreateMockService();

            // Act — worker revokes employer access
            var txHash = await mockService.Object.LogTransactionAsync(
                privateKey: PrivateKey,
                employerAddress: EmployerAddress,
                workerAddress: WorkerAddress,
                action: BlockchainAction.PermissionRevoked
            );

            // Assert
            Assert.NotEmpty(txHash);
            Assert.StartsWith("0x", txHash);

            // verify called once with correct action
            mockService.Verify(s => s.LogTransactionAsync(
                PrivateKey,
                EmployerAddress,
                WorkerAddress,
                BlockchainAction.PermissionRevoked,
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }
    }
}