'use client';

import { useEffect, useState } from 'react';
import {
  viewAccessRequest,
  ViewAccessError,
  type RequestAccessViewResult,
} from '@/lib/api/employerActiveAccessApi';

interface AccessViewModalProps {
  requestId: string;
  workerName: string;
  workerEmail: string;
  reason: string;
  onClose: () => void;
}

export default function AccessViewModal({
  requestId,
  workerName,
  workerEmail,
  reason,
  onClose,
}: AccessViewModalProps) {
  const [result, setResult] = useState<RequestAccessViewResult | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMsg, setErrorMsg] = useState('');

  useEffect(() => {
    async function load() {
      const token = localStorage.getItem('accessToken');

      if (!token) {
        setErrorMsg('Not authenticated.');
        setIsLoading(false);
        return;
      }

      try {
        const data = await viewAccessRequest(token, requestId);
        setResult(data);
      } catch (err) {
        if (err instanceof ViewAccessError) {
          if (err.status === 503 && err.detail === 'BLOCKCHAIN_LOG_FAILED') {
            setErrorMsg('Blockchain logging failed. The data was not displayed.');
          } else if (err.status === 503) {
            setErrorMsg(err.detail || 'A required service is not available.');
          } else if (err.status === 404) {
            setErrorMsg('Request not found.');
          } else if (err.status === 422) {
            setErrorMsg(err.detail || 'Cannot view this request data right now.');
          } else if (err.status === 403) {
            setErrorMsg('You do not have access to this request.');
          } else {
            setErrorMsg('Failed to load request data.');
          }
        } else {
          setErrorMsg('Failed to load request data.');
        }
      } finally {
        setIsLoading(false);
      }
    }

    load();
  }, [requestId]);

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/50"
      onClick={onClose}
    >
      <div
        onClick={(e) => e.stopPropagation()}
        className="relative max-h-[90vh] w-full max-w-5xl overflow-y-auto rounded-2xl bg-white p-6 shadow-lg"
      >
        <div className="mb-4 flex items-start justify-between gap-4">
          <div>
            <h2 className="text-xl font-semibold text-slate-900">
              Approved Request Data
            </h2>

            <p className="mt-1 text-sm text-slate-500">
              {workerName} · {workerEmail}
            </p>

            <p className="mt-2 text-sm text-slate-600">
              <span className="font-medium text-slate-700">Reason:</span> {reason}
            </p>

            <p className="mt-2 text-xs text-slate-400">
              Opening this modal records one request-level data access event on blockchain.
            </p>
          </div>

          <button
            type="button"
            onClick={onClose}
            className="text-2xl leading-none text-slate-400 hover:text-slate-600"
          >
            ✕
          </button>
        </div>

        {isLoading && <p className="text-sm text-slate-500">Loading request data...</p>}

        {!isLoading && errorMsg && (
          <p className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
            {errorMsg}
          </p>
        )}

        {!isLoading && !errorMsg && result && (
          <div className="space-y-5">
            <div className="rounded-lg border border-slate-200 bg-slate-50 px-4 py-3">
              <div className="grid gap-3 text-sm md:grid-cols-3">
                <div>
                  <p className="text-xs text-slate-500">Viewed</p>
                  <p className="mt-1 text-slate-800">
                    {new Date(result.viewedAt).toLocaleString()}
                  </p>
                </div>

                <div>
                  <p className="text-xs text-slate-500">Granted</p>
                  <p className="mt-1 text-slate-800">
                    {new Date(result.grantedAt).toLocaleDateString()}
                  </p>
                </div>

                <div>
                  <p className="text-xs text-slate-500">Expires</p>
                  <p className="mt-1 text-slate-800">
                    {new Date(result.expiryDate).toLocaleDateString()}
                  </p>
                </div>
              </div>
            </div>

            {result.categories.map((category) => (
              <section
                key={category.name}
                className="rounded-xl border border-slate-200 bg-white p-4"
              >
                <h3 className="mb-3 text-sm font-semibold text-slate-900">
                  {category.name}
                </h3>

                <div className="space-y-3">
                  {category.items.map((item) => (
                    <div
                      key={item.permissionId}
                      className="rounded-lg border border-slate-200 bg-slate-50 px-4 py-3"
                    >
                      <div className="mb-2 flex items-center justify-between gap-3">
                        <div>
                          <p className="text-sm font-medium text-slate-800">
                            {item.label}
                          </p>

                          <p className="mt-0.5 text-xs text-slate-400">
                            {item.type === 'file' ? 'File item' : 'Text item'}
                            {item.isCustom ? ' · Custom' : ''}
                          </p>
                        </div>
                      </div>

                      {item.type === 'text' && (
                        <pre className="whitespace-pre-wrap break-words rounded-md bg-white px-3 py-2 font-sans text-sm text-slate-800">
                          {item.value ?? '(empty)'}
                        </pre>
                      )}

                      {item.type === 'file' && (
                        <div className="rounded-md bg-white px-3 py-2">
                          {item.url ? (
                            <div>
                              <a
                                href={item.url}
                                target="_blank"
                                rel="noreferrer"
                                className="inline-flex rounded-md bg-slate-800 px-3 py-1.5 text-xs font-medium text-white hover:bg-slate-700"
                              >
                                Open file
                              </a>

                              {item.urlExpiresAt && (
                                <p className="mt-2 text-xs text-slate-400">
                                  Link expires at{' '}
                                  {new Date(item.urlExpiresAt).toLocaleTimeString()}
                                </p>
                              )}
                            </div>
                          ) : (
                            <p className="text-sm text-slate-500">
                              No file link is available.
                            </p>
                          )}
                        </div>
                      )}
                    </div>
                  ))}
                </div>
              </section>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
