'use client';

import { useEffect, useMemo, useState } from 'react';
import { useRouter } from 'next/navigation';
import {
  fetchActiveAccess,
  type EmployerActiveAccess,
} from '@/lib/api/employerActiveAccessApi';
import AccessViewModal from './AccessViewModal';

const PAGE_SIZE = 6;

interface ViewTarget {
  requestId: string;
  workerName: string;
  workerEmail: string;
  reason: string;
}

export default function MyAccessTab() {
  const router = useRouter();

  const [accessList, setAccessList] = useState<EmployerActiveAccess[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMsg, setErrorMsg] = useState('');

  const [searchText, setSearchText] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [viewTarget, setViewTarget] = useState<ViewTarget | null>(null);

  // The dataType filter was removed: items are now grouped by category in the
  // API response, so a flat "filter by data type" no longer maps cleanly.

  useEffect(() => {
    async function load() {
      const token = localStorage.getItem('accessToken');
      const role = localStorage.getItem('role');

      if (!token || role !== 'employer') {
        router.push('/login');
        return;
      }

      setIsLoading(true);
      setErrorMsg('');

      try {
        const data = await fetchActiveAccess(token);
        setAccessList(data);
      } catch {
        setErrorMsg('Failed to load active access.');
      } finally {
        setIsLoading(false);
      }
    }

    load();
  }, [router]);

  useEffect(() => {
    setCurrentPage(1);
  }, [searchText]);

  const filteredAccessList = useMemo(() => {
    const search = searchText.trim().toLowerCase();
    if (!search) return accessList;

    return accessList.filter(
      (a) =>
        a.workerName.toLowerCase().includes(search) ||
        a.workerEmail.toLowerCase().includes(search),
    );
  }, [accessList, searchText]);

  const totalPages = Math.max(1, Math.ceil(filteredAccessList.length / PAGE_SIZE));
  const paginatedAccessList = filteredAccessList.slice(
    (currentPage - 1) * PAGE_SIZE,
    currentPage * PAGE_SIZE,
  );

  if (isLoading) return <p className="text-sm text-gray-500">Loading active access...</p>;
  if (errorMsg) return <p className="text-sm text-red-500">{errorMsg}</p>;

  if (accessList.length === 0) {
    return (
      <div className="rounded-xl border border-gray-200 bg-white p-4 text-sm text-gray-500">
        No active access found.
      </div>
    );
  }

  return (
    <div>
      <div className="mb-4">
        <h2 className="text-lg font-semibold text-gray-900">My Access</h2>
        <p className="mt-1 text-sm text-gray-500">
          Worker data your company is currently approved to access. Click View Request Data to
          view all approved items under one request.
        </p>
      </div>

      <div className="mb-5 rounded-xl border border-gray-200 bg-white p-4">
        <label className="mb-1 block text-sm font-medium text-gray-700">
          Search worker
        </label>
        <input
          type="text"
          value={searchText}
          onChange={(e) => setSearchText(e.target.value)}
          placeholder="Search by name or email"
          className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm text-gray-900 placeholder:text-gray-400 focus:border-gray-900 focus:outline-none"
        />
      </div>

      {filteredAccessList.length === 0 ? (
        <p className="text-sm text-gray-500">No active access matches your filter.</p>
      ) : (
        <>
          <div className="space-y-3">
            {paginatedAccessList.map((a) => (
              <div
                key={a.requestId}
                className="space-y-4 rounded-xl border border-gray-200 bg-white p-4"
              >
                <div className="grid gap-3 md:grid-cols-[1.5fr_2fr_8rem_8rem_10rem]">
                  <div>
                    <p className="text-sm text-gray-500">Worker</p>
                    <p className="mt-1 font-medium text-gray-900">{a.workerName}</p>
                    <p className="mt-1 text-xs text-gray-500">{a.workerEmail}</p>
                  </div>

                  <div>
                    <p className="text-sm text-gray-500">Reason</p>
                    <p className="mt-1 text-sm text-gray-700">{a.reason}</p>
                  </div>

                  <div>
                    <p className="text-sm text-gray-500">Granted</p>
                    <p className="mt-1 text-sm text-gray-600">
                      {new Date(a.grantedAt).toLocaleDateString()}
                    </p>
                  </div>

                  <div>
                    <p className="text-sm text-gray-500">Expires</p>
                    <p className="mt-1 text-sm text-gray-600">
                      {new Date(a.expiryDate).toLocaleDateString()}
                    </p>
                  </div>

                  <div className="md:text-right">
                    <button
                      type="button"
                      onClick={() =>
                        setViewTarget({
                          requestId: a.requestId,
                          workerName: a.workerName,
                          workerEmail: a.workerEmail,
                          reason: a.reason,
                        })
                      }
                      className="rounded-md bg-slate-800 px-3 py-2 text-xs font-medium text-white hover:bg-slate-700"
                    >
                      View Request Data
                    </button>
                  </div>
                </div>

                {a.categories.map((cat) => (
                  <div key={cat.name}>
                    <p className="mb-2 text-sm font-semibold text-slate-800">{cat.name}</p>
                    <div className="space-y-1.5">
                      {cat.items.map((item) => (
                        <div
                          key={item.permissionId}
                          className="flex items-center justify-between rounded-lg border border-slate-200 px-3 py-2"
                        >
                          <span className="text-sm text-slate-700">
                            {item.label}
                            {item.type === 'file' && (
                              <span className="ml-1 text-xs text-slate-400">(file)</span>
                            )}
                            {item.isCustom && (
                              <span className="ml-1 text-xs text-slate-400">(custom)</span>
                            )}
                          </span>

                          <span className="text-xs text-slate-400">
                            Included in request
                          </span>
                        </div>
                      ))}
                    </div>
                  </div>
                ))}
              </div>
            ))}
          </div>

          <div className="mt-4 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
            <p className="text-sm text-gray-500">
              Showing {(currentPage - 1) * PAGE_SIZE + 1}
              {' - '}
              {Math.min(currentPage * PAGE_SIZE, filteredAccessList.length)}
              {' of '}
              {filteredAccessList.length}
              {' access records'}
            </p>

            <div className="flex items-center gap-2">
              <button
                type="button"
                disabled={currentPage === 1}
                onClick={() => setCurrentPage((p) => p - 1)}
                className="rounded-md border border-gray-300 px-3 py-1.5 text-sm text-gray-700 transition-colors hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-50"
              >
                Previous
              </button>

              <span className="text-sm text-gray-600">
                Page {currentPage} of {totalPages}
              </span>

              <button
                type="button"
                disabled={currentPage === totalPages}
                onClick={() => setCurrentPage((p) => p + 1)}
                className="rounded-md border border-gray-300 px-3 py-1.5 text-sm text-gray-700 transition-colors hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-50"
              >
                Next
              </button>
            </div>
          </div>
        </>
      )}

      {viewTarget && (
        <AccessViewModal
          requestId={viewTarget.requestId}
          workerName={viewTarget.workerName}
          workerEmail={viewTarget.workerEmail}
          reason={viewTarget.reason}
          onClose={() => setViewTarget(null)}
        />
      )}
    </div>
  );
}
