namespace wdb_backend.DTOs;

public class EmployerDashboardDto
{
    public EmployerCompanyInfoDto Company { get; set; } = new();
    public EmployerDashboardSummaryDto Summary { get; set; } = new();
    public List<EmployerRecentRequestDto> RecentRequests { get; set; } = new();
}

public class EmployerCompanyInfoDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Verified { get; set; }
}

public class EmployerDashboardSummaryDto
{
    public int PendingRequests { get; set; }
    public int AvailableRequests { get; set; }
    public int PartialRequests { get; set; }
}

public class EmployerRecentRequestDto
{
    public Guid RequestId { get; set; }
    public string WorkerName { get; set; } = string.Empty;
    public List<string> RequestedFields { get; set; } = new();
    public string Reason { get; set; } = string.Empty;

    // Pending / Available / Partial / Unavailable
    public string Status { get; set; } = string.Empty;

    public DateTime LastUpdatedAt { get; set; }
}
