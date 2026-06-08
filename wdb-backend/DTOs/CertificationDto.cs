namespace wdb_backend.DTOs;

public class CertificationUploadRequest
{
    public required IFormFile File { get; set; }
}

public class CertificationStatusResponse
{
    public string? Status { get; set; }
    public string? FileName { get; set; }
    public DateTime? UploadedAt { get; set; }
}