namespace wdb_backend.Models;

public class BlockchainKeyPair
{
    public string PrivateKey { get; set; } = string.Empty;
    public string BlockchainAddress { get; set; } = string.Empty;
}

public class BlockchainTransactionResponse
{
    public string EmployerAddress { get; set; } = string.Empty;
    public string WorkerAddress { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Action { get; set; } = string.Empty;
    public string TxHash { get; set; } = string.Empty;
}

public enum BlockchainAction
{
    PermissionRequested = 0,
    PermissionApproved = 1,
    PermissionRejected = 2,
    DataViewed = 3,
    PermissionRevoked = 4
}