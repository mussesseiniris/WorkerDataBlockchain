using wdb_backend.DTOs;

namespace wdb_backend.Abstractions;

public interface ICertificationService


{
    Task<CertificationStatusResponse> UploadCertificationAsync(Guid employerId, IFormFile file, CancellationToken ct = default);
    Task<CertificationStatusResponse> GetCertificationStatusAsync(Guid employerId, CancellationToken ct = default);
}