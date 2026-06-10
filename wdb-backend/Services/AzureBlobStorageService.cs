using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using wdb_backend.Abstractions;

namespace wdb_backend.Services;

/// <summary>
/// Azure Blob Storage implementation of ISupabaseStorageService.
/// Uses account key auth and generates SAS URLs for downloads.
/// </summary>
public class AzureBlobStorageService : ISupabaseStorageService
{
    private readonly BlobContainerClient _container;
    private readonly StorageSharedKeyCredential _credential;
    private readonly string _accountName;
    private readonly string _containerName;

    public AzureBlobStorageService(IConfiguration config)
    {
        _accountName = config["AzureBlob:AccountName"]
            ?? throw new InvalidOperationException("AzureBlob:AccountName not configured");
        var accountKey = config["AzureBlob:AccountKey"]
            ?? throw new InvalidOperationException("AzureBlob:AccountKey not configured");
        _containerName = config["AzureBlob:ContainerName"] ?? "documents";

        _credential = new StorageSharedKeyCredential(_accountName, accountKey);
        var serviceUri = new Uri($"https://{_accountName}.blob.core.windows.net");
        var serviceClient = new BlobServiceClient(serviceUri, _credential);
        _container = serviceClient.GetBlobContainerClient(_containerName);
    }

    public async Task<SignedUrlResult> CreateSignedUrlAsync(
        string objectPath,
        int expiresInSeconds = 900,
        CancellationToken cancellationToken = default)
    {
        var blobClient = _container.GetBlobClient(objectPath);
        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(expiresInSeconds);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerName,
            BlobName = objectPath,
            Resource = "b",
            ExpiresOn = expiresAt,
            Protocol = SasProtocol.Https
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var sasToken = sasBuilder.ToSasQueryParameters(_credential).ToString();
        var url = $"{blobClient.Uri}?{sasToken}";

        await Task.CompletedTask;
        return new SignedUrlResult
        {
            Url = url,
            ExpiresAt = expiresAt.UtcDateTime
        };
    }

    public async Task<string> UploadAsync(
        Stream content,
        string objectPath,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var blobClient = _container.GetBlobClient(objectPath);
        var headers = new BlobHttpHeaders { ContentType = contentType };

        await blobClient.UploadAsync(content, new BlobUploadOptions
        {
            HttpHeaders = headers
        }, cancellationToken);

        return objectPath;
    }
}
