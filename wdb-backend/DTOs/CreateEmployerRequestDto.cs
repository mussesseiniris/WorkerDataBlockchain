namespace wdb_backend.DTOs;

/// <summary>
/// Payload submitted by the employer to create a new data access request.
/// PresetFieldIds / CustomWorkerInfoIds / CustomRequest may be present in any
/// combination, but at least one must be non-empty.
/// The employer does not set the expiry date. The worker sets it during review.
/// </summary>
public class CreateEmployerRequestDto
{
    public required string WorkerEmail { get; set; }

    public required string Reason { get; set; }

    public List<Guid> PresetFieldIds { get; set; } = new();

    public List<Guid> CustomWorkerInfoIds { get; set; } = new();

    public string? CustomRequest { get; set; }
}

public class CreateEmployerRequestResultDto
{
    public Guid RequestId { get; set; }
}
