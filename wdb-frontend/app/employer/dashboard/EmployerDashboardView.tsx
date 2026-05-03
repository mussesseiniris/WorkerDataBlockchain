import type { EmployerDashboardData } from '@/lib/employerDashboardApi';

type EmployerDashboardViewProps = {
  data: EmployerDashboardData;
};

function getStatusClassName(status: string) {
  switch (status) {
    case 'Pending':
      return 'border-amber-200 bg-amber-50 text-amber-700';
    case 'Approved':
      return 'border-emerald-200 bg-emerald-50 text-emerald-700';
    case 'Rejected':
      return 'border-red-200 bg-red-50 text-red-700';
    case 'Expired':
      return 'border-slate-300 bg-slate-100 text-slate-600';
    case 'Partial':
      return 'border-blue-200 bg-blue-50 text-blue-700';
    default:
      return 'border-slate-200 bg-slate-50 text-slate-600';
  }
}

export default function EmployerDashboardView({
  data,
}: EmployerDashboardViewProps) {
  return (
    <main className="min-h-screen bg-slate-50 px-8 py-8">
      <div className="mx-auto max-w-6xl space-y-6">
        <header>
          <h1 className="text-2xl font-semibold text-slate-900">
            Employer Dashboard
          </h1>
        </header>

        <section className="grid gap-4 md:grid-cols-3">
          <div className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
            <p className="text-sm text-slate-500">Pending Requests</p>
            <p className="mt-2 text-3xl font-semibold text-slate-900">
              {data.summary.pendingRequests}
            </p>
            <p className="mt-1 text-sm text-slate-500">
              Waiting for worker response
            </p>
          </div>

          <div className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
            <p className="text-sm text-slate-500">Active Approved Access</p>
            <p className="mt-2 text-3xl font-semibold text-slate-900">
              {data.summary.activeApprovedAccess}
            </p>
            <p className="mt-1 text-sm text-slate-500">
              Current worker-approved access
            </p>
          </div>

          <div className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
            <p className="text-sm text-slate-500">Expiring Soon</p>
            <p className="mt-2 text-3xl font-semibold text-slate-900">
              {data.summary.expiringSoon}
            </p>
            <p className="mt-1 text-sm text-slate-500">
              Access expiring in the next 30 days
            </p>
          </div>
        </section>

        <section className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
          <h2 className="mb-4 text-lg font-semibold text-slate-900">
            Recent Access Requests
          </h2>

          {data.recentRequests.length === 0 ? (
            <p className="text-sm text-slate-500">
              No recent access requests.
            </p>
          ) : (
            <div className="divide-y divide-slate-200">
              {data.recentRequests.map((request) => (
                <div
                  key={request.requestId}
                  className="grid gap-4 py-4 first:pt-0 last:pb-0 md:grid-cols-[1.2fr_1.6fr_1.6fr_auto_auto]"
                >
                  <div>
                    <p className="text-sm text-slate-500">Worker</p>
                    <p className="mt-1 font-medium text-slate-900">
                      {request.workerName}
                    </p>
                  </div>

                  <div>
                    <p className="text-sm text-slate-500">Requested Fields</p>
                    <p className="mt-1 text-sm text-slate-700">
                      {request.requestedFields.join(', ')}
                    </p>
                  </div>

                  <div>
                    <p className="text-sm text-slate-500">Reason</p>
                    <p className="mt-1 text-sm text-slate-700">
                      {request.reason}
                    </p>
                  </div>

                  <div>
                    <p className="text-sm text-slate-500">Status</p>
                    <span
                      className={`mt-1 inline-flex rounded-full border px-3 py-1 text-xs font-medium ${getStatusClassName(
                        request.status
                      )}`}
                    >
                      {request.status}
                    </span>
                  </div>

                  <div className="md:text-right">
                    <p className="text-sm text-slate-500">Last Updated</p>
                    <p className="mt-1 text-sm text-slate-600">
                      {new Date(request.lastUpdatedAt).toLocaleDateString()}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          )}
        </section>
      </div>
    </main>
  );
}
