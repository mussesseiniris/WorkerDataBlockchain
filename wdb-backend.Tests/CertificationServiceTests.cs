using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using wdb_backend.Data;
using wdb_backend.Models;
using wdb_backend.Services;

namespace wdb_backend.Tests;

public class CertificationServiceTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static CertificationServiceImpl CreateService(AppDbContext context)
    {
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.ContentRootPath).Returns(Path.GetTempPath());
        return new CertificationServiceImpl(context, mockEnv.Object);
    }

    private static IFormFile CreateMockFile(string fileName = "cert.pdf")
    {
        var mockFile = new Mock<IFormFile>();
        var content = "fake pdf content";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(stream.Length);
        mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return mockFile.Object;
    }

    // ── UploadCertificationAsync ──────────────────────────────────────────

    [Fact]
    public async Task Upload_ShouldSetStatusToPending_WhenStatusIsNull()
    {
        // Arrange
        await using var context = CreateContext();
        var employerId = Guid.NewGuid();

        context.Employers.Add(new Employer
        {
            Id = employerId,
            Name = "Test Company",
            Email = "test@company.com",
            Password = "password",
            Verified = false,
            CertificationStatus = null
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var file = CreateMockFile("company_cert.pdf");

        // Act
        var result = await service.UploadCertificationAsync(employerId, file);

        // Assert
        Assert.Equal("Pending", result.Status);
        Assert.Equal("company_cert.pdf", result.FileName);
        Assert.NotNull(result.UploadedAt);

        var employer = await context.Employers.FindAsync(employerId);
        Assert.Equal("Pending", employer!.CertificationStatus);
    }

    [Fact]
    public async Task Upload_ShouldSetStatusToPending_WhenStatusIsRejected()
    {
        // Arrange
        await using var context = CreateContext();
        var employerId = Guid.NewGuid();

        context.Employers.Add(new Employer
        {
            Id = employerId,
            Name = "Test Company",
            Email = "test@company.com",
            Password = "password",
            Verified = false,
            CertificationStatus = "Rejected"
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var file = CreateMockFile("new_cert.pdf");

        // Act
        var result = await service.UploadCertificationAsync(employerId, file);

        // Assert
        Assert.Equal("Pending", result.Status);
        Assert.Equal("new_cert.pdf", result.FileName);
    }

    [Fact]
    public async Task Upload_ShouldThrow_WhenStatusIsPending()
    {
        // Arrange
        await using var context = CreateContext();
        var employerId = Guid.NewGuid();

        context.Employers.Add(new Employer
        {
            Id = employerId,
            Name = "Test Company",
            Email = "test@company.com",
            Password = "password",
            Verified = false,
            CertificationStatus = "Pending"
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var file = CreateMockFile();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UploadCertificationAsync(employerId, file));
    }

    [Fact]
    public async Task Upload_ShouldThrow_WhenStatusIsApproved()
    {
        // Arrange
        await using var context = CreateContext();
        var employerId = Guid.NewGuid();

        context.Employers.Add(new Employer
        {
            Id = employerId,
            Name = "Test Company",
            Email = "test@company.com",
            Password = "password",
            Verified = false,
            CertificationStatus = "Approved"
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var file = CreateMockFile();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UploadCertificationAsync(employerId, file));
    }

    [Fact]
    public async Task Upload_ShouldThrow_WhenEmployerNotFound()
    {
        // Arrange
        await using var context = CreateContext();
        var service = CreateService(context);
        var file = CreateMockFile();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.UploadCertificationAsync(Guid.NewGuid(), file));
    }

    // ── GetCertificationStatusAsync ───────────────────────────────────────

    [Fact]
    public async Task GetStatus_ShouldReturnNullStatus_WhenNeverUploaded()
    {
        // Arrange
        await using var context = CreateContext();
        var employerId = Guid.NewGuid();

        context.Employers.Add(new Employer
        {
            Id = employerId,
            Name = "Test Company",
            Email = "test@company.com",
            Password = "password",
            Verified = false,
            CertificationStatus = null
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var result = await service.GetCertificationStatusAsync(employerId);

        // Assert
        Assert.Null(result.Status);
        Assert.Null(result.FileName);
        Assert.Null(result.UploadedAt);
    }

    [Fact]
    public async Task GetStatus_ShouldReturnCorrectStatus_WhenPending()
    {
        // Arrange
        await using var context = CreateContext();
        var employerId = Guid.NewGuid();
        var uploadedAt = DateTime.UtcNow;

        context.Employers.Add(new Employer
        {
            Id = employerId,
            Name = "Test Company",
            Email = "test@company.com",
            Password = "password",
            Verified = false,
            CertificationStatus = "Pending",
            CertificationFileName = "cert.pdf",
            CertificationUploadedAt = uploadedAt
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var result = await service.GetCertificationStatusAsync(employerId);

        // Assert
        Assert.Equal("Pending", result.Status);
        Assert.Equal("cert.pdf", result.FileName);
        Assert.NotNull(result.UploadedAt);
    }

    [Fact]
    public async Task GetStatus_ShouldThrow_WhenEmployerNotFound()
    {
        // Arrange
        await using var context = CreateContext();
        var service = CreateService(context);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.GetCertificationStatusAsync(Guid.NewGuid()));
    }
}