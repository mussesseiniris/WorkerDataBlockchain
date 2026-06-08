'use client'

import { useEffect, useState } from 'react';
import { useAuth } from '@/context/AuthContext';

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5258';

interface NotificationItem {
    id: string;
    employerName: string;
    notificationType: string;
    workerInfoDesc: string;
    notificationTime: string;
}

const formatTime = (iso: string) =>
    new Date(iso).toLocaleString('en-US', {
        year: 'numeric', month: 'short', day: 'numeric',
        hour: '2-digit', minute: '2-digit', second: '2-digit',
        hour12: false
    });

export default function AllNotificationsPage() {
    const [unread, setUnread] = useState<NotificationItem[]>([]);
    const [read, setRead] = useState<NotificationItem[]>([]);
    // Centralized auth state. The effect below depends on userId/token so it re-runs after login.
    const { userId, token, isAuthReady } = useAuth();

    useEffect(() => {
        // Wait until AuthContext finishes restoring; then skip when no user is present.
        if (!isAuthReady || !userId || !token) return;
        const loadAll = async () => {
            const headers = {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            };

            const [unreadRes, readRes] = await Promise.all([
                fetch(`${API_URL}/api/notification/employer/me?isRead=false`, { headers }),
                fetch(`${API_URL}/api/notification/employer/me?isRead=true`, { headers })
            ]);

            if (unreadRes.ok) {
                const data = await unreadRes.json();
                setUnread(Array.isArray(data) ? data : data.data ?? []);
            }
            if (readRes.ok) {
                const data = await readRes.json();
                setRead(Array.isArray(data) ? data : data.data ?? []);
            }
        };
        loadAll();
    }, [isAuthReady, userId, token]);

    async function markAsRead(notificationId: string) {
        if (!token) return;
        const res = await fetch(`${API_URL}/api/notification/${notificationId}`, {
            method: 'PATCH',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            }
        });
        if (!res.ok) return;
        // move from unread to read in local state
        const item = unread.find(n => n.id === notificationId);
        if (!item) return;
        setUnread(prev => prev.filter(n => n.id !== notificationId));
        setRead(prev => [item, ...prev]);
    }

    return (
        <div className="max-w-4xl mx-auto px-10 py-10">
            <h1 className="text-2xl font-semibold text-gray-900 mb-8">All Notifications</h1>

            {/* Unread section */}
            <section className="mb-8">
                <div className="flex items-center gap-2 mb-3">
                    <h2 className="text-xs font-semibold text-gray-500 uppercase tracking-wider">Unread</h2>
                    {unread.length > 0 && (
                        <span className="bg-red-500 text-white text-[10px] font-bold rounded-full px-1.5 py-0.5">
                            {unread.length}
                        </span>
                    )}
                </div>

                {unread.length === 0 ? (
                    <p className="text-sm text-gray-400">No unread notifications.</p>
                ) : (
                    <ul className="flex flex-col gap-3">
                        {unread.map(n => (
                            <li
                                key={n.id}
                                onClick={() => markAsRead(n.id)}
                                className="flex items-center gap-4 px-5 py-4 bg-white border border-gray-100 rounded-xl shadow-sm hover:shadow-md hover:border-gray-200 cursor-pointer transition-all"
                            >
                                <span className="w-2.5 h-2.5 flex-shrink-0 rounded-full bg-red-500" />
                                <p className="flex-1 text-sm font-medium text-gray-800">
                                    [{n.notificationType}] {n.employerName} — [{n.workerInfoDesc}]
                                </p>
                                <p className="text-xs text-gray-400 whitespace-nowrap">{formatTime(n.notificationTime)}</p>
                            </li>
                        ))}
                    </ul>
                )}
            </section>

            {/* Read section */}
            <section>
                <h2 className="text-xs font-semibold text-gray-500 uppercase tracking-wider mb-3">Read</h2>

                {read.length === 0 ? (
                    <p className="text-sm text-gray-400">No read notifications.</p>
                ) : (
                    <ul className="flex flex-col gap-3">
                        {read.map(n => (
                            <li key={n.id} className="flex items-center gap-4 px-5 py-4 bg-gray-50 border border-gray-100 rounded-xl">
                                <span className="w-2.5 h-2.5 flex-shrink-0 rounded-full bg-gray-300" />
                                <p className="flex-1 text-sm text-gray-400">
                                    [{n.notificationType}] {n.employerName} — [{n.workerInfoDesc}]
                                </p>
                                <p className="text-xs text-gray-300 whitespace-nowrap">{formatTime(n.notificationTime)}</p>
                            </li>
                        ))}
                    </ul>
                )}
            </section>
        </div>
    );
}
