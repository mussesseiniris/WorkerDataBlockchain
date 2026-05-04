import type { WorkerDashboardResponse } from "@/lib/workerDashboardApi";

type WorkerDashboardViewProps = {
  data: WorkerDashboardResponse;
};

export default function WorkerDashboardView({
  data,
}: WorkerDashboardViewProps) {
  return (
    <main className="min-h-screen bg-slate-50 px-8 py-8">
      <div className="mx-auto max-w-6xl space-y-6">
        <header>
          <h1 className="text-2xl font-semibold text-slate-900">
            Worker Dashboard
          </h1>
        </header>

        <section className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
          <h2 className="mb-4 text-lg font-semibold text-slate-900">
            Personal Information
          </h2>

          <div className="grid gap-4 md:grid-cols-3">
            <div>
              <p className="text-sm text-slate-500">Name</p>
              <p className="mt-1 font-medium text-slate-900">
                {data.worker.name}
              </p>
            </div>

            <div>
              <p className="text-sm text-slate-500">Email</p>
              <p className="mt-1 font-medium text-slate-900">
                {data.worker.email}
              </p>
            </div>

            <div>
              <p className="text-sm text-slate-500">Status</p>
              <p className="mt-1 font-medium text-slate-900">
                {data.worker.verified ? "Verified" : "Not verified"}
              </p>
            </div>
          </div>
        </section>

        <section className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
          <h2 className="mb-4 text-lg font-semibold text-slate-900">
            Latest Requests
          </h2>

          {data.latestRequests.length === 0 ? (
            <p className="text-sm text-slate-500">No requests yet.</p>
          ) : (
            <div className="divide-y divide-slate-200">
              {data.latestRequests.map((request) => (
                <div
                  key={request.requestId}
                  className="flex items-start justify-between gap-4 py-4 first:pt-0 last:pb-0"
                >
                  <div>
                    <p className="font-medium text-slate-900">
                      {request.employerName}
                    </p>
                    <p className="mt-1 text-sm text-slate-600">
                      {request.reason}
                    </p>
                  </div>

                  <p className="shrink-0 text-sm text-slate-500">
                    {new Date(request.createdAt).toLocaleDateString()}
                  </p>
                </div>
              ))}
            </div>
          )}
        </section>

        <section className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
          <h2 className="mb-4 text-lg font-semibold text-slate-900">
            Blockchain Records
          </h2>

          {data.blockchainRecords.length === 0 ? (
            <p className="text-sm text-slate-500">
              No blockchain records available yet.
            </p>
          ) : (
            <div>{/* Future blockchain records */}</div>
          )}
        </section>
      </div>
    </main>
  );
}
