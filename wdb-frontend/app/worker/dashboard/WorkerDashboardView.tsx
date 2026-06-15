import Link from 'next/link';
import type { WorkerDashboardResponse } from '@/lib/workerDashboardApi';

type WorkerDashboardViewProps = {
  data: WorkerDashboardResponse;
};

function formatDateTime(dateString: string) {
  if (!dateString) return '-';

  return new Date(dateString).toLocaleString('en-NZ', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

function formatDate(dateString?: string | null) {
  if (!dateString) return '-';

  return new Date(dateString).toLocaleDateString('en-NZ', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });
}

function getStatusLabel(status: number) {
  switch (status) {
    case 0:
      return 'Pending';
    case 3:
      return 'Revoked';
    case 1:
    case 2:
    case 4:
      return 'Reviewed';
    default:
      return 'Unknown';
  }
}

function getStatusBadgeClass(status: number) {
  switch (status) {
    case 0:
      return 'bg-orange-100 text-orange-700';
    case 3:
      return 'bg-slate-100 text-slate-700';
    case 1:
    case 2:
    case 4:
      return 'bg-blue-100 text-blue-700';
    default:
      return 'bg-slate-100 text-slate-600';
  }
}

function shortenTxHash(txHash: string) {
  if (!txHash) return 'Not available';
  return `${txHash.slice(0, 10)}...${txHash.slice(-6)}`;
}

export default function WorkerDashboardView({
  data,
}: WorkerDashboardViewProps) {
  const pendingCount = data.summary.pendingReviews;
  const reviewedCount = data.summary.reviewedRequests;
  const totalRequests = data.summary.totalRequests;

  return (
    <main className="min-h-screen bg-slate-50 px-8 py-8">
      <div className="mx-auto max-w-7xl space-y-6">
        <header className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <p className="text-sm font-medium text-slate-500">
              Worker dashboard
            </p>
            <h1 className="mt-1 text-2xl font-semibold text-slate-900">
              Welcome back, {data.worker.name}
            </h1>
            <p className="mt-1 text-sm text-slate-500">
              Review data requests and check your audit history.
            </p>
          </div>

          <Link
            href="/worker/dataAccess"
            className="rounded-xl bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-800"
          >
            Manage data access
          </Link>
        </header>

        <section className="grid gap-4 md:grid-cols-3">
          <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
            <p className="text-sm text-slate-500">Pending</p>
            <p className="mt-2 text-3xl font-semibold text-slate-900">
              {pendingCount}
            </p>
            <p className="mt-2 text-xs text-slate-500">
              Requests waiting for your review
            </p>
          </div>

          <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
            <p className="text-sm text-slate-500">Reviewed</p>
            <p className="mt-2 text-3xl font-semibold text-slate-900">
              {reviewedCount}
            </p>
            <p className="mt-2 text-xs text-slate-500">
              Requests you have already reviewed
            </p>
          </div>

          <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
            <p className="text-sm text-slate-500">Total requests</p>
            <p className="mt-2 text-3xl font-semibold text-slate-900">
              {totalRequests}
            </p>
            <p className="mt-2 text-xs text-slate-500">
              Requests received from companies
            </p>
          </div>
        </section>

        <section className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
          <div className="mb-5 flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <h2 className="text-lg font-semibold text-slate-900">
                Latest requests
              </h2>
              <p className="mt-1 text-sm text-slate-500">
                Latest 3 data access requests grouped by request.
              </p>
            </div>

            <Link
              href="/worker/dataAccess"
              className="text-sm font-semibold text-blue-600 hover:text-blue-700"
            >
              View all requests
            </Link>
          </div>

          {data.latestRequests.length === 0 ? (
            <div className="rounded-xl border border-dashed border-slate-300 p-6 text-sm text-slate-500">
              No data access requests yet.
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full min-w-[820px] border-collapse text-left">
                <thead>
                  <tr className="border-b border-slate-200 text-sm font-semibold text-slate-900">
                    <th className="pb-4 pr-6">Company</th>
                    <th className="pb-4 pr-6">Information</th>
                    <th className="pb-4 pr-6">Purpose</th>
                    <th className="pb-4 pr-6">Requested</th>
                    <th className="pb-4 pr-6">Status</th>
                    <th className="pb-4">Expires</th>
                  </tr>
                </thead>

                <tbody>
                  {data.latestRequests.map((request) => (
                    <tr
                      key={request.requestId}
                      className="border-b border-slate-100 text-sm text-slate-900 last:border-b-0"
                    >
                      <td className="py-4 pr-6 font-medium">
                        {request.employerName || 'Unknown company'}
                      </td>

                      <td className="max-w-xs py-4 pr-6">
                        <span className="line-clamp-2">
                          {request.requestedInformation || '-'}
                        </span>
                      </td>

                      <td className="max-w-xs py-4 pr-6">
                        <span className="line-clamp-2">
                          {request.checkPurpose || '-'}
                        </span>
                      </td>

                      <td className="py-4 pr-6">
                        {formatDateTime(request.createdAt)}
                      </td>

                      <td className="py-4 pr-6">
                        <span
                          className={`rounded-full px-3 py-1 text-xs font-medium ${getStatusBadgeClass(
                            request.status,
                          )}`}
                        >
                          {getStatusLabel(request.status)}
                        </span>
                      </td>

                      <td className="py-4">
                        {formatDate(request.expiresAt)}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </section>

        <section className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
          <div className="flex items-start justify-between gap-4">
            <div>
              <h2 className="text-lg font-semibold text-slate-900">
                Recent blockchain activity
              </h2>
              <p className="mt-1 text-sm text-slate-500">
                Recent access-related actions recorded on chain.
              </p>
            </div>

            <Link
              href="/worker/auditLog"
              className="text-sm font-semibold text-blue-600 hover:text-blue-700"
            >
              View audit log
            </Link>
          </div>

          {!data.blockchainAvailable ? (
            <div className="mt-5 rounded-xl border border-dashed border-slate-300 bg-slate-50 p-5">
              <p className="text-sm font-medium text-slate-800">
                Blockchain audit is currently unavailable.
              </p>
              <p className="mt-1 text-sm text-slate-500">
                Your normal access request flow still works. On-chain audit
                records will appear here when the blockchain service is running.
              </p>
            </div>
          ) : data.blockchainRecords.length === 0 ? (
            <div className="mt-5 rounded-xl border border-dashed border-slate-300 p-5 text-sm text-slate-500">
              No blockchain records available yet.
            </div>
          ) : (
            <div className="mt-5 divide-y divide-slate-100">
              {data.blockchainRecords.map((record) => (
                <div key={record.txHash} className="py-4 first:pt-0">
                  <div className="flex items-start justify-between gap-3">
                    <div>
                      <p className="font-medium text-slate-900">
                        {record.actionLabel}
                      </p>
                      <p className="mt-1 whitespace-pre-line text-sm text-slate-600">
                        {record.userMessage}
                      </p>
                    </div>

                    <p className="shrink-0 text-xs text-slate-400">
                      {formatDateTime(record.date)}
                    </p>
                  </div>

                  <p className="mt-2 text-xs text-slate-500">
                    Proof: {shortenTxHash(record.txHash)}
                  </p>
                </div>
              ))}
            </div>
          )}
        </section>
      </div>
    </main>
  );
}
