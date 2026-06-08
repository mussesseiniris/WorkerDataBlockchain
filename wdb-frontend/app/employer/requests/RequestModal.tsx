'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/context/AuthContext';
import {
  createEmployerRequest,
  fetchRequestCatalog,
  type EmployerRequestCatalog,
} from '@/lib/api/employerRequestApi';

interface RequestModalProps {
  onClose: () => void;
}

export default function RequestModal({ onClose }: RequestModalProps) {
  const router = useRouter();
  const { token, role, isAuthReady } = useAuth();

  // Step 1: search worker
  const [email, setEmail] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  // Step 2: build request
  const [catalog, setCatalog] = useState<EmployerRequestCatalog | null>(null);
  const [reason, setReason] = useState('');
  const [selectedFieldIds, setSelectedFieldIds] = useState<Set<string>>(
    new Set(),
  );
  const [selectedCustomItemIds, setSelectedCustomItemIds] = useState<
    Set<string>
  >(new Set());
  const [customRequest, setCustomRequest] = useState('');
  const [showCustomRequest, setShowCustomRequest] = useState(false);

  const [errorMsg, setErrorMsg] = useState('');
  const [sentMsg, setSentMsg] = useState('');

  function ensureEmployerAuth() {
    if (!isAuthReady) {
      setErrorMsg('Loading authentication. Please try again in a moment.');
      return false;
    }

    if (!token || role !== 'employer') {
      router.push('/login');
      return false;
    }

    return true;
  }

  async function handleSearch() {
    if (!email.trim()) {
      setErrorMsg('Please enter a worker email');
      return;
    }

    if (!ensureEmployerAuth()) return;

    setIsLoading(true);
    setErrorMsg('');
    setSentMsg('');

    try {
      const result = await fetchRequestCatalog(token!, email.trim());
      setCatalog(result);
    } catch (err) {
      console.error('Failed to load catalog:', err);
      setErrorMsg('Worker not found');
    } finally {
      setIsLoading(false);
    }
  }

  function toggleFieldId(id: string) {
    setSelectedFieldIds((prev) => {
      const next = new Set(prev);

      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }

      return next;
    });
  }

  function toggleCustomItemId(id: string) {
    setSelectedCustomItemIds((prev) => {
      const next = new Set(prev);

      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }

      return next;
    });
  }

  async function handleSubmit() {
    if (!ensureEmployerAuth() || !catalog) return;

    if (!reason.trim()) {
      setErrorMsg('Please provide a reason');
      return;
    }

    if (
      selectedFieldIds.size === 0 &&
      selectedCustomItemIds.size === 0 &&
      !customRequest.trim()
    ) {
      setErrorMsg(
        'Please select at least one field, custom item, or write a custom request',
      );
      return;
    }

    setSentMsg('Sending...');
    setErrorMsg('');

    try {
      await createEmployerRequest(token!, {
        workerEmail: catalog.worker.email,
        reason: reason.trim(),
        presetFieldIds: Array.from(selectedFieldIds),
        customWorkerInfoIds: Array.from(selectedCustomItemIds),
        customRequest: customRequest.trim() ? customRequest.trim() : undefined,
      });

      setSentMsg('Request sent!');

      // Close the modal and return to the employer dashboard page.
      // The request has already been saved before this runs.
      onClose();
      router.push('/employer/dashboard');
      router.refresh();
    } catch (err) {
      console.error('Failed to submit request:', err);
      setSentMsg('');
      setErrorMsg('Failed to send request');
    }
  }

  if (!isAuthReady) {
    return (
      <div className="p-8">
        <p className="text-slate-500">Loading...</p>
      </div>
    );
  }

  if (!token || role !== 'employer') return null;

  return (
    <div className="relative mx-auto max-h-[90vh] w-full max-w-2xl overflow-y-auto rounded-2xl bg-white p-8 shadow-lg">
      <div className="mb-6 flex items-center justify-between">
        <h2 className="text-2xl font-bold text-slate-900">
          Create new request
        </h2>

        <button
          onClick={onClose}
          className="text-2xl text-slate-400 hover:text-slate-600"
        >
          ✕
        </button>
      </div>

      {!catalog && (
        <div className="flex flex-col gap-4">
          <div className="relative rounded-xl border border-gray-300 px-4 pb-2 pt-5">
            <label className="absolute left-4 top-2 text-xs text-gray-500">
              Worker Email
            </label>

            <input
              type="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              className="w-full text-gray-900 outline-none"
              placeholder="worker@example.com"
            />
          </div>

          <button
            onClick={handleSearch}
            disabled={isLoading}
            className="w-full rounded-xl bg-slate-800 px-6 py-3 font-medium text-white disabled:opacity-70"
          >
            {isLoading ? 'Searching...' : 'Search'}
          </button>

          {errorMsg && (
            <p className="text-center text-red-500">{errorMsg}</p>
          )}
        </div>
      )}

      {catalog && (
        <div className="flex flex-col gap-5">
          <div className="rounded-xl bg-slate-50 p-4 text-sm text-slate-700">
            <p>
              Email:{' '}
              <span className="text-blue-700">{catalog.worker.email}</span>
            </p>
            <p>Name: {catalog.worker.name}</p>
          </div>

          <div className="rounded-xl border border-blue-100 bg-blue-50 px-4 py-3 text-sm text-blue-900">
            The worker will set the access expiry date when reviewing this
            request.
          </div>

          {catalog.categories.map((category) => {
            const hasContent =
              category.presetFields.length > 0 ||
              category.customItems.length > 0;

            if (!hasContent) return null;

            return (
              <div key={category.id}>
                <p className="mb-2 font-semibold text-slate-900">
                  {category.name}
                </p>

                <div className="grid grid-cols-2 gap-2">
                  {category.presetFields.map((field) => (
                    <label
                      key={field.fieldId}
                      className="flex cursor-pointer items-center gap-2 rounded-lg border px-3 py-2 hover:border-slate-400"
                    >
                      <input
                        type="checkbox"
                        className="accent-slate-800"
                        checked={selectedFieldIds.has(field.fieldId)}
                        onChange={() => toggleFieldId(field.fieldId)}
                      />

                      <span className="text-sm text-slate-800">
                        {field.label}
                        {field.allowedType === 'file' && (
                          <span className="ml-1 text-xs text-slate-500">
                            (file)
                          </span>
                        )}
                      </span>
                    </label>
                  ))}

                  {category.customItems.map((item) => (
                    <label
                      key={item.workerInfoId}
                      className="flex cursor-pointer items-center gap-2 rounded-lg border px-3 py-2 hover:border-slate-400"
                    >
                      <input
                        type="checkbox"
                        className="accent-slate-800"
                        checked={selectedCustomItemIds.has(item.workerInfoId)}
                        onChange={() => toggleCustomItemId(item.workerInfoId)}
                      />

                      <span className="text-sm text-slate-800">
                        {item.label}
                        {item.type === 'file' && (
                          <span className="ml-1 text-xs text-slate-500">
                            (file)
                          </span>
                        )}
                      </span>
                    </label>
                  ))}
                </div>
              </div>
            );
          })}

          <div>
            <button
              onClick={() => setShowCustomRequest((value) => !value)}
              className="text-sm text-slate-700 underline"
            >
              {showCustomRequest
                ? 'Hide custom request'
                : '+ Add custom request (Optional)'}
            </button>

            {showCustomRequest && (
              <textarea
                className="mt-3 w-full rounded-xl border border-gray-300 px-4 py-3 text-gray-900 outline-none"
                rows={3}
                placeholder="Describe any extra information you need that is not in the list above"
                value={customRequest}
                onChange={(event) => setCustomRequest(event.target.value)}
              />
            )}
          </div>

          <div className="relative rounded-xl border border-gray-300 px-4 pb-2 pt-5">
            <label className="absolute left-4 top-2 text-xs text-gray-500">
              Reason
            </label>

            <input
              type="text"
              className="w-full text-gray-900 outline-none"
              placeholder="Why do you need this information?"
              value={reason}
              onChange={(event) => setReason(event.target.value)}
            />
          </div>

          <button
            onClick={handleSubmit}
            className="w-full rounded-xl bg-slate-800 px-6 py-3 font-medium text-white"
          >
            Submit
          </button>

          {errorMsg && (
            <p className="text-center text-red-500">{errorMsg}</p>
          )}

          {sentMsg && (
            <p className="text-center text-sm text-slate-700">{sentMsg}</p>
          )}
        </div>
      )}
    </div>
  );
}


