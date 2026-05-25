'use client';

// Employer requests: submit new data access requests to workers
import { FetchApi } from '@/lib/api';
import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/context/AuthContext';

interface RequestModalProps {
    onClose: () => void;
}

export default function RequestModal({ onClose }: RequestModalProps) {

    const router = useRouter();

    // Read auth state from AuthContext
    const { token, role, isAuthReady } = useAuth();

    type WorkerInfo = {
        id: string;
        desc: string;
        value: string;
        status: string;
    };

    type Worker = {
        id: string;
        name: string;
        email: string;
    };

    const [email, setEmail] = useState('');
    const [reason, setReason] = useState('');

    const [worker, setWorker] = useState<Worker | null>(null);
    const [workerInfos, setWorkerInfos] = useState<WorkerInfo[]>([]);
    const [workerInfosHasRequested, setWorkerInfosHasRequested] = useState<WorkerInfo[]>([]);

    const [isLoading, setIsLoading] = useState(false);
    const [errorMsg, setErrorMsg] = useState('');
    const [sentMsg, setSentMsg] = useState('');
    const [findWorker, setFindWorker] = useState(false);
    const [isOpen, setIsOpen] = useState(true);

    // Selected worker info item IDs
    const [isSelected, setSelected] = useState<Set<string>>(new Set());

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

    // Search for a worker by email and fetch their available info
    async function handleSearch(email: string) {
        if (!email) {
            alert('Please enter an email');
            return;
        }

        if (!ensureEmployerAuth()) {
            return;
        }

        setIsLoading(true);
        setErrorMsg('');
        setSentMsg('');

        try {
            const worker = await FetchApi(
                `/api/Employer/GetWorkerByEmail?email=${email}`,
                {
                    method: 'GET',
                    headers: {
                        Authorization: `Bearer ${token}`,
                    },
                }
            );

            if (!worker) {
                alert('There is no worker information for this email');
                return;
            }

            setWorker(worker);

            const workerInfos = await FetchApi(`/api/Employer?email=${email}`, {
                method: 'GET',
                headers: {
                    Authorization: `Bearer ${token}`,
                },
            });

            setWorkerInfos(workerInfos);

            const requestedWorkerInfos = await FetchApi(
                `/api/Employer/GetRequestedWorkerInfosByEmail?email=${email}`,
                {
                    method: 'GET',
                    headers: {
                        Authorization: `Bearer ${token}`,
                    },
                }
            );

            setWorkerInfosHasRequested(requestedWorkerInfos);
            setFindWorker(true);
        } catch (error) {
            console.error('Failed to search worker:', error);
            setErrorMsg('Worker not found');
        } finally {
            setIsLoading(false);
        }
    }

    // Toggle selection of a worker info item
    function toggle(id: string) {
        setSelected((prev) => {
            const next = new Set(prev);

            if (next.has(id)) {
                next.delete(id);
            } else {
                next.add(id);
            }

            return next;
        });
    }

    // Submit access request for selected worker info items
    async function handleRequest() {
        if (!reason) {
            alert('Please fill in the reason');
            return;
        }

        if (isSelected.size === 0) {
            alert('Please select at least one item');
            return;
        }

        if (!ensureEmployerAuth()) {
            return;
        }

        setSentMsg('Sending...');

        try {
            await FetchApi('/api/Employer/AccessRequests', {
                method: 'POST',
                headers: {
                    Authorization: `Bearer ${token}`,
                },
                body: JSON.stringify({
                    email,
                    infoDesc: Array.from(isSelected),
                    reason,
                }),
            });

            setSentMsg('Request has been sent');
        } catch (error) {
            console.error('Failed to submit access request:', error);
            setSentMsg('Something went wrong, please try again');
        }
    }

    if (!isAuthReady) {
        return (
            <main className="p-8">
                <p className="text-gray-500">Loading request page...</p>
            </main>
        );
    }

    if (!token || role !== 'employer') {
        return null;
    }

    return (
        <div className="relative max-w-lg mx-auto mt-10 p-6 border border-gray-600 rounded-xl shadow-md">
            <button
                className="absolute top-5 right-8 text-gray-600 text-xl"
                onClick={onClose}
            >
                x
            </button>

            <div className="flex items-center justify-center flex-col gap-2">
                <p className="text-left w-full text-gray-600">Create new request</p>

                {!findWorker && (
                    <div className="w-full gap-2">
                        <div className="relative border border-gray-300 rounded-xl px-4 pt-5 pb-2 w-full">
                            <label className="absolute top-2 left-4 text-xs text-gray-400">
                                Email
                            </label>

                            <input
                                type="text"
                                value={email}
                                onChange={(e) => setEmail(e.target.value)}
                                className="w-full outline-none text-gray-800"
                                placeholder="Workeremail@gmail.com"
                            />
                        </div>

                        <button
                            onClick={() => handleSearch(email)}
                            disabled={isLoading}
                            className="bg-[#49454F] px-6 py-2 rounded-lg text-white w-full my-2 disabled:opacity-70"
                        >
                            {isLoading ? 'Searching' : 'Search'}
                        </button>

                        {errorMsg && (
                            <p className="text-[#49454F] text-center">{errorMsg}</p>
                        )}
                    </div>
                )}

                {findWorker && (
                    <div className="w-full flex flex-col">
                        <div className="text-gray-600 bg-gray-100 w-full rounded-lg my-4 p-4">
                            <p>Email: {worker?.email}</p>
                            <p>Name: {worker?.name}</p>
                        </div>

                        {workerInfos.length === 0 ? (
                            <>
                                <p className="mb-4">This worker has no info items</p>
                                <button
                                    className="px-6 py-2 rounded-lg bg-[#49454F] text-white w-full"
                                    onClick={() => setFindWorker(false)}
                                >
                                    Back
                                </button>
                            </>
                        ) : (
                            <>
                                {workerInfosHasRequested.length > 0 && (
                                    <>
                                        <p className="text-gray-600">
                                            The info you have requested:
                                        </p>

                                        <div className="flex flex-col gap-2 mb-4">
                                            {workerInfosHasRequested.map((w) => (
                                                <div
                                                    key={w.id}
                                                    className="flex items-center justify-center border rounded-lg border-gray-300 w-full"
                                                >
                                                    <p className="flex-1 text-black">
                                                        {w.desc}: {w.status}
                                                    </p>
                                                </div>
                                            ))}
                                        </div>
                                    </>
                                )}

                                <p className="text-gray-600">
                                    Please choose the info you want to request
                                </p>

                                <div className="flex flex-col gap-2">
                                    {workerInfos.map((w) => (
                                        <div
                                            key={w.id}
                                            className="flex items-center justify-center border rounded-lg border-gray-300 w-full"
                                        >
                                            <input
                                                type="checkbox"
                                                className="border rounded-lg gap-2 m-2 accent-[#49454F]"
                                                checked={isSelected.has(w.id)}
                                                onChange={() => toggle(w.id)}
                                            />

                                            <p className="flex-1 text-black">{w.desc}</p>
                                        </div>
                                    ))}
                                </div>

                                <div className="flex flex-col items-center gap-4 rounded-lg my-4 w-full">
                                    <div className="relative border border-gray-300 rounded-xl px-4 pt-5 pb-2 w-full">
                                        <label className="absolute top-2 left-4 text-xs text-gray-400">
                                            Reason
                                        </label>

                                        <input
                                            type="text"
                                            className="w-full outline-none text-gray-800"
                                            value={reason}
                                            onChange={(e) => setReason(e.target.value)}
                                        />
                                    </div>

                                    <button
                                        onClick={handleRequest}
                                        className="px-6 py-2 rounded-lg bg-[#49454F] text-white w-full"
                                    >
                                        Submit
                                    </button>

                                    <p className="flex items-center">{sentMsg}</p>
                                </div>
                            </>
                        )}
                    </div>
                )}
            </div>
        </div>
    )
}
