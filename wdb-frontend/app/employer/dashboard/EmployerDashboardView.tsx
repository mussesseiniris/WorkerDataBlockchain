'use client';

import Link from 'next/link';
import { useState } from 'react';
import type {
  EmployerDashboardData,
  EmployerRequestStatus,
} from '@/lib/employerDashboardApi';
import RequestModal from '../requests/RequestModal';

type EmployerDashboardViewProps = {
  data: EmployerDashboardData;
};

const FIELD_PREVIEW_LIMIT = 3;

function getStatusClassName(status: EmployerRequestStatus) {
  if (status === 'Pending') {
    return 'border-amber-200 bg-amber-50 text-amber-700';
  }

  if (status === 'Revoked') {
    return 'border-slate-300 bg-slate-100 text-slate-600';
  }

  return 'border-blue-200 bg-blue-50 text-blue-700';
}

function formatStatus(status: EmployerRequestStatus) {
  if (status === 'Pending') return 'Pending';
  if (status === 'Revoked') return 'Revoked';

  return 'Reviewed';
}

function formatDate(dateString: string) {
  if (!dateString) return '-';
  return new Date(dateString).toLocaleDateString('en-NZ');
}

function formatRequestedFields(fields: string[]) {
  if (!fields || fields.length === 0) {
    return '-';
  }

  if (fields.length <= FIELD_PREVIEW_LIMIT) {
    return fields.join(', ');
  }

  const preview = fields.slice(0, FIELD_PREVIEW_LIMIT).join(', ');
  const remainingCount = fields.length - FIELD_PREVIEW_LIMIT;

  return `${preview} +${remainingCount} more`;
}

export default function EmployerDashboardView({
  data,
}: EmployerDashboardViewProps) {
  const [showModal, setShowModal] = useState(false);

  return (
    <main className="min-h-screen bg-slate-50 px-8 py-8">
      {showModal && (
        <div
          className="fixed inset-0 z-10 flex items-center justify-center bg-black bg-opacity-50"
          onClick={() => setShowModal(false)}
        >
          <div
            onClick={(e) => e.stopPropagation()}
            className="rounded-lg bg-white p-6"
          >
            <RequestModal onClose={() => setShowModal(false)} />
          </div>
        </div>
      )}

      <div className="mx-auto max-w-6xl space-y-6">
        <header>
          <h1 className="text-2xl font-semibold text-slate-900">
            Employer Dashboard
          </h1>
        </header>

        <button
          onClick={() => setShowModal(true)}
          className="mb-4 rounded-lg bg-[#49454F] px-4 py-2 text-sm font-medium text-white hover:bg-[#49454F]/90"
        >
          Create New Request
        </button>

        <section className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
          <h2 className="mb-4 text-lg font-semibold text-slate-900">
            Company Information
          </h2>

          <div className="grid gap-4 md:grid-cols-3">
            <div>
              <p className="text-sm text-slate-500">Company Name</p>
              <p className="mt-1 font-medium text-slate-900">
                {data.company.name}
              </p>
            </div>

            <div>
              <p className="text-sm text-slate-500">Email</p>
              <p className="mt-1 font-medium text-slate-900">
                {data.company.email}
              </p>
            </div>

            <div>
              <p className="text-sm text-slate-500">Status</p>
              <p className="mt-1 font-medium text-slate-900">
                {data.company.verified ? 'Verified' : 'Not verified'}
              </p>
            </div>
          </div>
        </section>

        <section className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
          <h2 className="mb-4 text-lg font-semibold text-slate-900">
            Request Summary
          </h2>

          <div className="grid gap-4 md:grid-cols-3">
            <div className="rounded-2xl border border-slate-200 bg-slate-50 p-5">
              <p className="text-sm text-slate-500">Pending</p>
              <p className="mt-2 text-3xl font-semibold text-slate-900">
                {data.summary.pendingRequests}
              </p>
              <p className="mt-1 text-sm text-slate-500">
                Requests waiting for worker review
              </p>
            </div>

            <div className="rounded-2xl border border-slate-200 bg-slate-50 p-5">
              <p className="text-sm text-slate-500">Reviewed</p>
              <p className="mt-2 text-3xl font-semibold text-slate-900">
                {data.summary.reviewedRequests}
              </p>
              <p className="mt-1 text-sm text-slate-500">
                Requests already processed by workers
              </p>
            </div>

            <div className="rounded-2xl border border-slate-200 bg-slate-50 p-5">
              <p className="text-sm text-slate-500">Total Requests</p>
              <p className="mt-2 text-3xl font-semibold text-slate-900">
                {data.summary.totalRequests}
              </p>
              <p className="mt-1 text-sm text-slate-500">
                All requests you have sent
              </p>
            </div>
          </div>
        </section>

        <section className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
          <div className="mb-5 flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <h2 className="text-lg font-semibold text-slate-900">
                Recent Access Requests
              </h2>
              <p className="mt-1 text-sm text-slate-500">
                Latest 3 access requests and their current overall status.
              </p>
            </div>

            <Link
              href="/employer/dataAccess"
              className="text-sm font-semibold text-blue-600 hover:text-blue-700"
            >
              View all requests
            </Link>
          </div>

          {data.recentRequests.length === 0 ? (
            <p className="text-sm text-slate-500">
              No recent access requests.
            </p>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full min-w-[900px] border-collapse text-left">
                <thead>
                  <tr className="border-b border-slate-200 text-sm font-semibold text-slate-900">
                    <th className="pb-4 pr-6">Worker</th>
                    <th className="pb-4 pr-6">Requested Fields</th>
                    <th className="pb-4 pr-6">Reason</th>
                    <th className="pb-4 pr-6">Status</th>
                    <th className="pb-4 text-right">Last Updated</th>
                  </tr>
                </thead>

                <tbody>
                  {data.recentRequests.map((request) => (
                    <tr
                      key={request.requestId}
                      className="border-b border-slate-100 text-sm text-slate-900 last:border-b-0"
                    >
                      <td className="py-4 pr-6 font-medium">
                        {request.workerName}
                      </td>

                      <td className="max-w-sm py-4 pr-6 text-slate-700">
                        <span className="line-clamp-2">
                          {formatRequestedFields(request.requestedFields)}
                        </span>
                      </td>

                      <td className="max-w-xs py-4 pr-6 text-slate-700">
                        <span className="line-clamp-2">
                          {request.reason || '-'}
                        </span>
                      </td>

                      <td className="py-4 pr-6">
                        <span
                          className={`inline-flex w-28 justify-center rounded-full border px-3 py-1 text-xs font-medium ${getStatusClassName(
                            request.status
                          )}`}
                        >
                          {formatStatus(request.status)}
                        </span>
                      </td>

                      <td className="py-4 text-right text-slate-600">
                        {formatDate(request.lastUpdatedAt)}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </section>
      </div>
    </main>
  );
}
