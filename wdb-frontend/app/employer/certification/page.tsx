'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/context/AuthContext';

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5258';

type CertificationStatus = 'Pending' | 'Approved' | 'Rejected' | null;

interface CertificationStatusResponse {
    status: CertificationStatus;
    fileName: string | null;
    uploadedAt: string | null;
}

export default function CertificationPage() {
    const router = useRouter();
    const { token, role, isAuthReady } = useAuth();

    const [status, setStatus] = useState<CertificationStatusResponse | null>(null);
    const [isLoading, setIsLoading] = useState(false);
    const [isUploading, setIsUploading] = useState(false);
    const [errorMsg, setErrorMsg] = useState('');
    const [successMsg, setSuccessMsg] = useState('');
    const [selectedFile, setSelectedFile] = useState<File | null>(null);

    async function loadStatus() {
        if (!token) return;
        setIsLoading(true);
        setErrorMsg('');
        try {
            const res = await fetch(`${API_URL}/api/certification/status`, {
                headers: { Authorization: `Bearer ${token}` }
            });
            if (!res.ok) throw new Error('Failed to load certification status');
            const data = await res.json();
            setStatus(data);
        } catch (error) {
            setErrorMsg(error instanceof Error ? error.message : String(error));
        } finally {
            setIsLoading(false);
        }
    }

    async function handleUpload() {
        if (!token || !selectedFile) return;
        setIsUploading(true);
        setErrorMsg('');
        setSuccessMsg('');
        try {
            const formData = new FormData();
            formData.append('file', selectedFile);
            const res = await fetch(`${API_URL}/api/certification/upload`, {
                method: 'POST',
                headers: { Authorization: `Bearer ${token}` },
                body: formData
            });
            if (!res.ok) {
                const err = await res.json();
                throw new Error(err.message || 'Upload failed');
            }
            setSuccessMsg('Certification document uploaded successfully.');
            setSelectedFile(null);
            await loadStatus();
        } catch (error) {
            setErrorMsg(error instanceof Error ? error.message : String(error));
        } finally {
            setIsUploading(false);
        }
    }

    useEffect(() => {
        if (!isAuthReady) return;
        if (!token || role !== 'employer') {
            router.push('/');
            return;
        }
        loadStatus();
    }, [isAuthReady, token, role]);

    if (!isAuthReady || isLoading) {
        return (
            <main className="min-h-screen bg-slate-50 px-8 py-8">
                <p className="text-sm text-slate-500">Loading...</p>
            </main>
        );
    }

    const canUpload = status?.status == null || status?.status === 'Rejected';

    function statusBadge(s: CertificationStatus) {
        if (!s) return <span className="inline-flex rounded-full px-2 py-0.5 text-xs font-medium bg-slate-100 text-slate-500">Not Submitted</span>;
        if (s === 'Pending') return <span className="inline-flex rounded-full px-2 py-0.5 text-xs font-medium bg-orange-50 text-orange-700">Pending Review</span>;
        if (s === 'Approved') return <span className="inline-flex rounded-full px-2 py-0.5 text-xs font-medium bg-emerald-50 text-emerald-700">Approved</span>;
        if (s === 'Rejected') return <span className="inline-flex rounded-full px-2 py-0.5 text-xs font-medium bg-red-50 text-red-700">Rejected</span>;
    }

    return (
        <main className="min-h-screen bg-slate-50 px-8 py-8">
            <div className="mx-auto max-w-3xl space-y-6">

                {/* Header */}
                <header className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
                    <p className="text-sm font-medium text-slate-500">Employer portal</p>
                    <h1 className="mt-1 text-2xl font-semibold text-slate-900">Certification</h1>
                    <p className="mt-2 text-sm text-slate-500">
                        Upload your company certification documents for verification. Once approved, your account will be marked as verified.
                    </p>
                </header>

                {/* Messages */}
                {(errorMsg || successMsg) && (
                    <section className={`rounded-xl border px-4 py-3 text-sm ${errorMsg
                        ? 'border-red-200 bg-red-50 text-red-700'
                        : 'border-emerald-200 bg-emerald-50 text-emerald-700'
                        }`}>
                        {errorMsg || successMsg}
                    </section>
                )}

                {/* Current Status */}
                <section className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
                    <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Current Status</h2>
                    <div className="mt-4 flex items-center gap-4">
                        {statusBadge(status?.status ?? null)}
                        {status?.fileName && (
                            <p className="text-sm text-slate-600">
                                <span className="font-medium">File:</span> {status.fileName}
                            </p>
                        )}
                        {status?.uploadedAt && (
                            <p className="text-sm text-slate-500">
                                Uploaded: {new Date(status.uploadedAt).toLocaleString()}
                            </p>
                        )}
                    </div>
                </section>

                {/* Upload Section */}
                {canUpload && (
                    <section className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
                        <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">
                            {status?.status === 'Rejected' ? 'Re-upload Document' : 'Upload Document'}
                        </h2>
                        <p className="mt-1 text-sm text-slate-500">
                            Please upload your company registration certificate or equivalent document.
                        </p>

                        <div className="mt-4 space-y-4">
                            <input
                                type="file"
                                accept=".pdf,.jpg,.jpeg,.png"
                                onChange={(e) => setSelectedFile(e.target.files?.[0] ?? null)}
                                className="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900 file:mr-4 file:rounded-lg file:border-0 file:bg-slate-900 file:px-4 file:py-1.5 file:text-sm file:font-semibold file:text-white hover:file:bg-slate-800"
                            />

                            {selectedFile && (
                                <p className="text-sm text-slate-600">
                                    Selected: <span className="font-medium">{selectedFile.name}</span>
                                </p>
                            )}

                            <button
                                disabled={!selectedFile || isUploading}
                                onClick={handleUpload}
                                className="rounded-lg bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-800 disabled:cursor-not-allowed disabled:opacity-40"
                            >
                                {isUploading ? 'Uploading...' : 'Upload'}
                            </button>
                        </div>
                    </section>
                )}

                {/* Approved message */}
                {status?.status === 'Approved' && (
                    <section className="rounded-2xl border border-emerald-200 bg-emerald-50 p-6">
                        <p className="text-sm font-semibold text-emerald-700">
                            Your certification has been approved. Your account is now verified.
                        </p>
                    </section>
                )}

                {/* Pending message */}
                {status?.status === 'Pending' && (
                    <section className="rounded-2xl border border-orange-200 bg-orange-50 p-6">
                        <p className="text-sm font-semibold text-orange-700">
                            Your document is under review. We will notify you once the review is complete.
                        </p>
                    </section>
                )}

            </div>
        </main>
    );
}