using System.Security.Cryptography.X509Certificates;
using wdb_backend.Models;

public interface IFindWorkerInfosByEmailUsecase
{

    Task<List<WorkerInfo>> FindWorkerInfosByEmail(string email, CancellationToken cancellationToken = default);

}
