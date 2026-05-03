namespace wdb_backend.DTOs;

public class EmployerDashboardDto
{
    public EmployerDashboardSummaryDto Summary { get; set; } = new();
    public List<EmployerRecentRequestDto> RecentRequests { get; set; } = new();
}

public class EmployerDashboardSummaryDto
{
    public int PendingRequests { get; set; }
    public int ActiveApprovedAccess { get; set; }
    public int ExpiringSoon { get; set; }
}

public class EmployerRecentRequestDto
{
    public Guid RequestId { get; set; }
    public string WorkerName { get; set; } = string.Empty;
    public List<string> RequestedFields { get; set; } = new();
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime LastUpdatedAt { get; set; }
}
