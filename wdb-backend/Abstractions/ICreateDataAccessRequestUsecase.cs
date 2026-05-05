using Microsoft.AspNetCore.Mvc;
using wdb_backend.Models;

namespace wdb_backend.Abstractions;

public interface ICreateDataAccessRequestUsecase
{

    Task CreateDataAccessRequest(List<WorkerInfo> workerInfos, Guid employerId,
        Guid workerId, string reason);



}
