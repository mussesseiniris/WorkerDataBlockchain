namespace wdb_backend.DTOs;

/// <summary>
/// A pending request shown on the worker data access review page.
/// </summary>
public class WorkerActiveRequestDto
{
    public Guid RequestId { get; set; }
    public Guid EmployerId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Null while the request is pending.
    /// The worker sets this value when approving access.
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<WorkerRequestReviewItemDto> Items { get; set; } = new();

    /// <summary>
    /// Optional custom request made by the employer for a new field that does not exist yet.
    /// </summary>
    public WorkerCustomRequestDto? CustomRequest { get; set; }
}

/// <summary>
/// A single permission item inside a pending request.
/// </summary>
public class WorkerRequestReviewItemDto
{
    public Guid PermissionId { get; set; }

    /// <summary>
    /// Preset field id. Null for custom fields.
    /// </summary>
    public Guid? FieldId { get; set; }

    /// <summary>
    /// worker_info.id. May be null before approving a preset field.
    /// </summary>
    public Guid? InfoId { get; set; }

    public string Label { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Value { get; set; }

    public int Status { get; set; }
    public bool HasValue { get; set; }
    public bool CanApprove { get; set; }
    public string? CannotApproveReason { get; set; }
}

public class WorkerCustomRequestDto
{
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "pending";
}

/// <summary>
/// POST /api/worker/data-access/requests/{requestId}/review
/// </summary>
public class SubmitWorkerRequestReviewRequest
{
    /// <summary>
    /// Required if the worker approves at least one permission item or approves a custom request.
    /// Not required if the worker rejects everything.
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    public List<SubmitWorkerRequestReviewItem> Items { get; set; } = new();

    /// <summary>
    /// Optional decision for request.custom_request.
    /// </summary>
    public SubmitWorkerCustomRequestDecision? CustomRequestDecision { get; set; }
}

public class SubmitWorkerRequestReviewItem
{
    public Guid PermissionId { get; set; }

    /// <summary>
    /// "approved" or "rejected".
    /// </summary>
    public string Decision { get; set; } = string.Empty;
}

public class SubmitWorkerCustomRequestDecision
{
    /// <summary>
    /// "approved" or "rejected".
    /// </summary>
    public string Decision { get; set; } = string.Empty;

    /// <summary>
    /// Required when decision = approved.
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// Required when decision = approved. Must be "text" or "file".
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Required when decision = approved.
    /// </summary>
    public string? Value { get; set; }
}
