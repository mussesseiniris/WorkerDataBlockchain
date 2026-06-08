using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Security;
using wdb_backend.Abstractions;
using wdb_backend.Data;
using wdb_backend.DTOs;
using wdb_backend.Models;
namespace wdb_backend.Services;


public class CertificationServiceImpl : ICertificationService
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly string _uploadPath;

    public CertificationServiceImpl(AppDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
        _uploadPath = Path.Combine(environment.ContentRootPath, "uploads", "certifications");
        Directory.CreateDirectory(_uploadPath);
    }

    public async Task<CertificationStatusResponse> UploadCertificationAsync(Guid employerId, IFormFile file, CancellationToken ct = default)
    {
        var employer = await _context.Employers.FirstOrDefaultAsync(e => e.Id == employerId, ct);
        if (employer == null)
        {
            throw new KeyNotFoundException($"Employer {employerId} not found");
        }

        var status = employer.CertificationStatus;
        if (status == "Pending")
        {
            throw new InvalidOperationException($"Certification for employer {employerId} is already pending review");
        }
        if (status == "Approved")
        {
            throw new InvalidOperationException($"Certification for employer {employerId} is already approved, no need to upload again");
        }

        var fileName = $"{employerId}_{file.FileName}";
        var filePath = Path.Combine(_uploadPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, ct);
        }

        employer.CertificationStatus = "Pending";
        employer.CertificationFilePath = filePath;
        employer.CertificationFileName = file.FileName;
        employer.CertificationUploadedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return new CertificationStatusResponse
        {
            Status = employer.CertificationStatus,
            FileName = employer.CertificationFileName,
            UploadedAt = employer.CertificationUploadedAt
        };
    }

    public async Task<CertificationStatusResponse> GetCertificationStatusAsync(Guid employerId, CancellationToken ct = default)
    {
        var employer = await _context.Employers.AsNoTracking().FirstOrDefaultAsync(e => e.Id == employerId, ct);
        if (employer == null)
        {
            throw new KeyNotFoundException($"Employer {employerId} not found");
        }

        return new CertificationStatusResponse
        {
            Status = employer.CertificationStatus,
            FileName = employer.CertificationFileName,
            UploadedAt = employer.CertificationUploadedAt
        };

    }
}
