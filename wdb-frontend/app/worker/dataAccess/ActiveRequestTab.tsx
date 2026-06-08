'use client';

import { useEffect, useMemo, useState } from 'react';
import {
  getWorkerActiveRequests,
  submitWorkerRequestReview,
  type WorkerActiveRequest,
  type WorkerRequestReviewItem,
} from '@/lib/workerDataAccessApi';

interface ActiveRequestTabProps {
  token: string;
  refreshKey: number;
  onChanged: () => void;
}

type ReviewDecision = 'approved' | 'rejected';

type CustomFormState = {
  label: string;
  type: 'text' | 'file';
  value: string;
};

type RequestReviewState = {
  itemDecisions: Record<string, ReviewDecision>;
  customDecision?: ReviewDecision;
  customForm: CustomFormState;
  expiryDate: string;
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

function categoryLabel(category: string) {
  switch (category) {
    case 'PersonalInformation':
      return 'Personal information';
    case 'MedicalInformation':
      return 'Medical information';
    case 'CareerInformation':
      return 'Career information';
    case 'WorkplaceInformation':
      return 'Workplace information';
    case 'FinancialInformation':
      return 'Financial information';
    case 'OtherInformation':
      return 'Other information';
    default:
      return category || 'Other information';
  }
}

function groupItemsByCategory(items: WorkerRequestReviewItem[]) {
  return items.reduce<Record<string, WorkerRequestReviewItem[]>>(
    (groups, item) => {
      const category = item.category || 'OtherInformation';

      if (!groups[category]) {
        groups[category] = [];
      }

      groups[category].push(item);
      return groups;
    },
    {},
  );
}

function getDefaultCustomForm(request: WorkerActiveRequest): CustomFormState {
  return {
    label: request.customRequest?.description ?? '',
    type: 'text',
    value: '',
  };
}

function hasAnyApprovedDecision(
  request: WorkerActiveRequest,
  state?: RequestReviewState,
) {
  if (!state) return false;

  const hasApprovedItem = request.items.some(
    (item) => state.itemDecisions[item.permissionId] === 'approved',
  );

  const hasApprovedCustomRequest =
    request.customRequest && state.customDecision === 'approved';

  return hasApprovedItem || Boolean(hasApprovedCustomRequest);
}

export default function ActiveRequestTab({
  token,
  refreshKey,
  onChanged,
}: ActiveRequestTabProps) {
  const [requests, setRequests] = useState<WorkerActiveRequest[]>([]);
  const [reviewStates, setReviewStates] = useState<
    Record<string, RequestReviewState>
  >({});
  const [isLoading, setIsLoading] = useState(false);
  const [submittingRequestId, setSubmittingRequestId] = useState<string | null>(
    null,
  );
  const [errorMsg, setErrorMsg] = useState('');

  useEffect(() => {
    async function loadRequests() {
      setIsLoading(true);
      setErrorMsg('');

      try {
        const data = await getWorkerActiveRequests(token);
        setRequests(data);

        setReviewStates((current) => {
          const next: Record<string, RequestReviewState> = { ...current };

          data.forEach((request) => {
            if (!next[request.requestId]) {
              next[request.requestId] = {
                itemDecisions: {},
                customDecision: undefined,
                customForm: getDefaultCustomForm(request),
                expiryDate: '',
              };
            }
          });

          return next;
        });
      } catch (error) {
        setErrorMsg(error instanceof Error ? error.message : String(error));
      } finally {
        setIsLoading(false);
      }
    }

    loadRequests();
  }, [token, refreshKey]);

  const totalPendingItems = useMemo(() => {
    return requests.reduce((total, request) => {
      const itemCount = request.items.length;
      const customCount = request.customRequest ? 1 : 0;
      return total + itemCount + customCount;
    }, 0);
  }, [requests]);

  function updateItemDecision(
    requestId: string,
    permissionId: string,
    decision: ReviewDecision,
  ) {
    setReviewStates((current) => ({
      ...current,
      [requestId]: {
        itemDecisions: {
          ...(current[requestId]?.itemDecisions ?? {}),
          [permissionId]: decision,
        },
        customDecision: current[requestId]?.customDecision,
        customForm: current[requestId]?.customForm ?? {
          label: '',
          type: 'text',
          value: '',
        },
        expiryDate: current[requestId]?.expiryDate ?? '',
      },
    }));
  }

  function updateCustomDecision(requestId: string, decision: ReviewDecision) {
    setReviewStates((current) => ({
      ...current,
      [requestId]: {
        itemDecisions: current[requestId]?.itemDecisions ?? {},
        customDecision: decision,
        customForm: current[requestId]?.customForm ?? {
          label: '',
          type: 'text',
          value: '',
        },
        expiryDate: current[requestId]?.expiryDate ?? '',
      },
    }));
  }

  function updateCustomForm(
    requestId: string,
    updates: Partial<CustomFormState>,
  ) {
    setReviewStates((current) => ({
      ...current,
      [requestId]: {
        itemDecisions: current[requestId]?.itemDecisions ?? {},
        customDecision: current[requestId]?.customDecision,
        customForm: {
          label: current[requestId]?.customForm.label ?? '',
          type: current[requestId]?.customForm.type ?? 'text',
          value: current[requestId]?.customForm.value ?? '',
          ...updates,
        },
        expiryDate: current[requestId]?.expiryDate ?? '',
      },
    }));
  }

  function updateExpiryDate(requestId: string, expiryDate: string) {
    setReviewStates((current) => ({
      ...current,
      [requestId]: {
        itemDecisions: current[requestId]?.itemDecisions ?? {},
        customDecision: current[requestId]?.customDecision,
        customForm: current[requestId]?.customForm ?? {
          label: '',
          type: 'text',
          value: '',
        },
        expiryDate,
      },
    }));
  }

  function getRequestValidationError(request: WorkerActiveRequest) {
    const state = reviewStates[request.requestId];

    if (!state) {
      return 'Please review all requested items before submitting.';
    }

    for (const item of request.items) {
      const decision = state.itemDecisions[item.permissionId];

      if (!decision) {
        return `Please approve or reject "${item.label}" before submitting.`;
      }

      if (decision === 'approved' && !item.canApprove) {
        return `"${item.label}" has no saved value and cannot be approved. Please reject it or add the value in Personal Data first.`;
      }
    }

    if (request.customRequest) {
      if (!state.customDecision) {
        return 'Please approve or reject the custom request before submitting.';
      }

      if (state.customDecision === 'approved') {
        if (!state.customForm.label.trim()) {
          return 'Please enter a label for the new custom information.';
        }

        if (!state.customForm.value.trim()) {
          return 'Please enter a value for the new custom information.';
        }
      }
    }

    if (hasAnyApprovedDecision(request, state)) {
      if (!state.expiryDate) {
        return 'Please set an expiry date for the approved access.';
      }

      const expiry = new Date(`${state.expiryDate}T23:59:59`);

      if (Number.isNaN(expiry.getTime()) || expiry <= new Date()) {
        return 'Expiry date must be in the future.';
      }
    }

    return '';
  }

  function isRequestReadyToSubmit(request: WorkerActiveRequest) {
    return getRequestValidationError(request) === '';
  }

  async function submitRequestReview(request: WorkerActiveRequest) {
    const validationError = getRequestValidationError(request);

    if (validationError) {
      setErrorMsg(validationError);
      return;
    }

    const state = reviewStates[request.requestId];

    if (!state) {
      setErrorMsg('Please review all requested items before submitting.');
      return;
    }

    const approved = hasAnyApprovedDecision(request, state);

    setSubmittingRequestId(request.requestId);
    setErrorMsg('');

    try {
      await submitWorkerRequestReview(token, request.requestId, {
        expiryDate: approved
          ? new Date(`${state.expiryDate}T23:59:59`).toISOString()
          : null,
        items: request.items.map((item) => ({
          permissionId: item.permissionId,
          decision: state.itemDecisions[item.permissionId],
        })),
        customRequestDecision: request.customRequest
          ? state.customDecision === 'approved'
            ? {
              decision: 'approved',
              label: state.customForm.label.trim(),
              type: state.customForm.type,
              value: state.customForm.value.trim(),
            }
            : {
              decision: 'rejected',
            }
          : null,
      });

      onChanged();
    } catch (error) {
      setErrorMsg(error instanceof Error ? error.message : String(error));
    } finally {
      setSubmittingRequestId(null);
    }
  }

  if (isLoading) {
    return (
      <div className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
        <p className="text-sm text-slate-600">Loading active requests...</p>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <section className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
        <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <h2 className="text-lg font-semibold text-slate-900">
              Active Requests
            </h2>
            <p className="mt-1 text-sm text-slate-600">
              Review each request as a group. Choose approve or reject for every
              item, set an expiry date if approving access, then submit the whole
              request.
            </p>
          </div>

          <span className="rounded-full bg-slate-100 px-3 py-1 text-sm font-medium text-slate-700">
            {totalPendingItems} pending item
            {totalPendingItems === 1 ? '' : 's'}
          </span>
        </div>

        {errorMsg && (
          <div className="mt-4 rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
            {errorMsg}
          </div>
        )}
      </section>

      {requests.length === 0 ? (
        <section className="rounded-2xl border border-dashed border-slate-300 bg-white p-8 text-sm text-slate-600">
          No active permission requests.
        </section>
      ) : (
        requests.map((request) => {
          const state = reviewStates[request.requestId];
          const groupedItems = groupItemsByCategory(request.items);
          const readyToSubmit = isRequestReadyToSubmit(request);
          const isSubmitting = submittingRequestId === request.requestId;
          const approvalSelected = hasAnyApprovedDecision(request, state);

          return (
            <section
              key={request.requestId}
              className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm"
            >
              <div className="flex flex-col gap-3 border-b border-slate-100 pb-4 sm:flex-row sm:items-start sm:justify-between">
                <div>
                  <h3 className="text-base font-semibold text-slate-900">
                    {request.companyName || 'Unknown company'}
                  </h3>
                  <p className="mt-1 text-sm text-slate-700">
                    Purpose: {request.reason || '-'}
                  </p>
                  <p className="mt-1 text-xs font-medium text-slate-600">
                    Requested {formatDateTime(request.createdAt)}
                  </p>
                </div>

                <span className="rounded-full bg-orange-100 px-3 py-1 text-xs font-medium text-orange-700">
                  Pending review
                </span>
              </div>

              <div className="mt-4 space-y-4">
                {Object.entries(groupedItems).map(([category, items]) => (
                  <div
                    key={category}
                    className="rounded-xl border border-slate-200 bg-slate-50 p-4"
                  >
                    <div className="mb-3 flex items-center justify-between">
                      <h4 className="text-sm font-semibold text-slate-900">
                        {categoryLabel(category)}
                      </h4>
                      <span className="rounded-full bg-white px-2 py-1 text-xs text-slate-600">
                        {items.length} item{items.length === 1 ? '' : 's'}
                      </span>
                    </div>

                    <div className="space-y-3">
                      {items.map((item) => {
                        const selectedDecision =
                          state?.itemDecisions[item.permissionId];

                        return (
                          <div
                            key={item.permissionId}
                            className="rounded-xl border border-slate-200 bg-white p-4"
                          >
                            <div className="flex flex-col gap-3 md:flex-row md:items-start md:justify-between">
                              <div>
                                <div className="flex flex-wrap items-center gap-2">
                                  <p className="font-medium text-slate-900">
                                    {item.label}
                                  </p>

                                  {selectedDecision && (
                                    <span
                                      className={
                                        selectedDecision === 'approved'
                                          ? 'rounded-full bg-emerald-100 px-2 py-1 text-xs font-medium text-emerald-700'
                                          : 'rounded-full bg-red-100 px-2 py-1 text-xs font-medium text-red-700'
                                      }
                                    >
                                      {selectedDecision === 'approved'
                                        ? 'Will approve'
                                        : 'Will reject'}
                                    </span>
                                  )}
                                </div>

                                <p className="mt-1 text-sm text-slate-600">
                                  {item.hasValue
                                    ? `Saved value: ${item.value}`
                                    : item.cannotApproveReason ??
                                    'No saved value available.'}
                                </p>
                              </div>

                              <div className="flex gap-2">
                                <button
                                  type="button"
                                  disabled={!item.canApprove || isSubmitting}
                                  onClick={() =>
                                    updateItemDecision(
                                      request.requestId,
                                      item.permissionId,
                                      'approved',
                                    )
                                  }
                                  className={
                                    selectedDecision === 'approved'
                                      ? 'rounded-lg bg-emerald-600 px-3 py-2 text-sm font-semibold text-white disabled:cursor-not-allowed disabled:opacity-40'
                                      : 'rounded-lg border border-emerald-200 px-3 py-2 text-sm font-semibold text-emerald-700 hover:bg-emerald-50 disabled:cursor-not-allowed disabled:opacity-40'
                                  }
                                >
                                  Approve
                                </button>

                                <button
                                  type="button"
                                  disabled={isSubmitting}
                                  onClick={() =>
                                    updateItemDecision(
                                      request.requestId,
                                      item.permissionId,
                                      'rejected',
                                    )
                                  }
                                  className={
                                    selectedDecision === 'rejected'
                                      ? 'rounded-lg bg-red-600 px-3 py-2 text-sm font-semibold text-white disabled:cursor-not-allowed disabled:opacity-40'
                                      : 'rounded-lg border border-red-200 px-3 py-2 text-sm font-semibold text-red-700 hover:bg-red-50 disabled:cursor-not-allowed disabled:opacity-40'
                                  }
                                >
                                  Reject
                                </button>
                              </div>
                            </div>
                          </div>
                        );
                      })}
                    </div>
                  </div>
                ))}

                {request.customRequest && (
                  <div className="rounded-xl border border-blue-200 bg-blue-50 p-4">
                    <div className="flex flex-col gap-3 lg:flex-row lg:items-start lg:justify-between">
                      <div className="flex-1">
                        <div className="flex flex-wrap items-center gap-2">
                          <p className="font-medium text-slate-900">
                            New information requested
                          </p>
                          <span className="rounded-full bg-blue-100 px-2 py-1 text-xs font-medium text-blue-700">
                            Custom request
                          </span>
                          {state?.customDecision && (
                            <span
                              className={
                                state.customDecision === 'approved'
                                  ? 'rounded-full bg-emerald-100 px-2 py-1 text-xs font-medium text-emerald-700'
                                  : 'rounded-full bg-red-100 px-2 py-1 text-xs font-medium text-red-700'
                              }
                            >
                              {state.customDecision === 'approved'
                                ? 'Will add & approve'
                                : 'Will reject'}
                            </span>
                          )}
                        </div>

                        <p className="mt-1 text-sm text-slate-700">
                          {request.customRequest.description}
                        </p>

                        <div className="mt-4 flex gap-2">
                          <button
                            type="button"
                            disabled={isSubmitting}
                            onClick={() =>
                              updateCustomDecision(
                                request.requestId,
                                'approved',
                              )
                            }
                            className={
                              state?.customDecision === 'approved'
                                ? 'rounded-lg bg-emerald-600 px-3 py-2 text-sm font-semibold text-white disabled:cursor-not-allowed disabled:opacity-40'
                                : 'rounded-lg border border-emerald-200 bg-white px-3 py-2 text-sm font-semibold text-emerald-700 hover:bg-emerald-50 disabled:cursor-not-allowed disabled:opacity-40'
                            }
                          >
                            Add & Approve
                          </button>

                          <button
                            type="button"
                            disabled={isSubmitting}
                            onClick={() =>
                              updateCustomDecision(
                                request.requestId,
                                'rejected',
                              )
                            }
                            className={
                              state?.customDecision === 'rejected'
                                ? 'rounded-lg bg-red-600 px-3 py-2 text-sm font-semibold text-white disabled:cursor-not-allowed disabled:opacity-40'
                                : 'rounded-lg border border-red-200 bg-white px-3 py-2 text-sm font-semibold text-red-700 hover:bg-red-50 disabled:cursor-not-allowed disabled:opacity-40'
                            }
                          >
                            Reject
                          </button>
                        </div>

                        {state?.customDecision === 'approved' && (
                          <div className="mt-4 grid gap-3 md:grid-cols-[1fr_140px_1fr]">
                            <div>
                              <label className="mb-1 block text-xs font-medium text-slate-700">
                                Label
                              </label>
                              <input
                                value={state.customForm.label}
                                onChange={(event) =>
                                  updateCustomForm(request.requestId, {
                                    label: event.target.value,
                                  })
                                }
                                className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm text-slate-900 focus:border-slate-900 focus:outline-none"
                                placeholder="e.g. Site Access Card Number"
                              />
                            </div>

                            <div>
                              <label className="mb-1 block text-xs font-medium text-slate-700">
                                Type
                              </label>
                              <select
                                value={state.customForm.type}
                                onChange={(event) =>
                                  updateCustomForm(request.requestId, {
                                    type: event.target.value as 'text' | 'file',
                                  })
                                }
                                className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm text-slate-900 focus:border-slate-900 focus:outline-none"
                              >
                                <option value="text">Text</option>
                                <option value="file">File</option>
                              </select>
                            </div>

                            <div>
                              <label className="mb-1 block text-xs font-medium text-slate-700">
                                Value
                              </label>
                              <input
                                value={state.customForm.value}
                                onChange={(event) =>
                                  updateCustomForm(request.requestId, {
                                    value: event.target.value,
                                  })
                                }
                                className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm text-slate-900 focus:border-slate-900 focus:outline-none"
                                placeholder="Enter the information to share"
                              />
                            </div>
                          </div>
                        )}
                      </div>
                    </div>
                  </div>
                )}

                <div className="rounded-xl border border-slate-300 bg-white p-4">
                  <label className="block text-sm font-semibold text-slate-950">
                    Access expiry date
                  </label>

                  <p className="mt-1 text-sm text-slate-700">
                    Required only if you approve at least one item. The company
                    can access approved data until the end of this date.
                  </p>

                  <input
                    type="date"
                    value={state?.expiryDate ?? ''}
                    disabled={!approvalSelected || isSubmitting}
                    onChange={(event) =>
                      updateExpiryDate(request.requestId, event.target.value)
                    }
                    className="mt-3 w-full max-w-xs rounded-lg border border-slate-400 bg-white px-3 py-2 text-sm font-medium text-slate-950 focus:border-slate-900 focus:outline-none disabled:bg-slate-100 disabled:text-slate-600"
                  />
                </div>

                <div className="flex flex-col gap-3 border-t border-slate-100 pt-4 sm:flex-row sm:items-center sm:justify-between">
                  <p className="text-xs font-medium text-slate-600">
                    This request will only be submitted after every item has a
                    decision.
                  </p>

                  <button
                    type="button"
                    disabled={!readyToSubmit || isSubmitting}
                    onClick={() => submitRequestReview(request)}
                    className="rounded-lg bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-800 disabled:cursor-not-allowed disabled:opacity-40"
                  >
                    {isSubmitting ? 'Submitting...' : 'Submit review'}
                  </button>
                </div>
              </div>
            </section>
          );
        })
      )}
    </div>
  );
}
