using wdb_backend.Abstractions;
using wdb_backend.Models;

public class FindWorkerInfosByEmailUsecaseImpl : IFindWorkerInfosByEmailUsecase
{
    private readonly IWorkerService _workerService;
    private readonly IWorkerInfoService _workerInfoService;

    public FindWorkerInfosByEmailUsecaseImpl(IWorkerService workerService, IWorkerInfoService workerInfoService)
    {
        _workerService = workerService;
        _workerInfoService = workerInfoService;
    }

    public async Task<List<WorkerInfo>> FindWorkerInfosByEmail(string email, CancellationToken cancellationToken)
    {
        var worker = await _workerService.GetByEmailAsync(email, cancellationToken);
        if (worker == null)
        {
            return new List<WorkerInfo>();
        }
        else{
        var workerinfos = await _workerInfoService.GetAllAsync(worker.Id, cancellationToken);
        return workerinfos;}

    }
}
