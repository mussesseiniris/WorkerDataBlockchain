'use client';

import * as signalR from '@microsoft/signalr';
import { useEffect } from 'react';
import { toast, ToastContainer } from 'react-toastify';
import { useAuth } from '@/context/AuthContext';
import { useNotificationRefresh } from '@/context/NotificationRefreshContext';
import { NotificationIcon, getTypeStyle } from '@/app/notification/displayConfig';

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5258';

interface NotificationPayload {
    type: string;
    employerName?: string | null;
    workerInfoDesc?: string | null;
}

export default function SignalRListener() {
    // Consume centralized auth state so the listener re-subscribes when the user logs in.
    // Root layout never unmounts, so reading localStorage once on mount would miss post-login changes.
    const { userId, isAuthReady } = useAuth();
    const { bumpRefresh } = useNotificationRefresh();

    useEffect(() => {
        // Wait until AuthContext finishes restoring auth state from localStorage,
        // then skip until a user is actually present.
        if (!isAuthReady || !userId) return;

        const connection = new signalR.HubConnectionBuilder()
            .withUrl(`${API_URL}/hubs/notifications?workerId=${userId}`)
            .withAutomaticReconnect()
            .build();

        // Register the handler before start() so a message arriving during the start
        // handshake is not dropped.
        connection.on("NotificationInfo", (payload: NotificationPayload | string) => {
            // Older backend versions sent a plain string; new backend sends an object.
            // Be defensive so a deploy lag doesn't break the toast.
            if (typeof payload === 'string') {
                toast.info(payload);
            } else {
                const style = getTypeStyle(payload.type);
                toast.info(
                    <div className="flex items-start gap-3">
                        <NotificationIcon type={payload.type} size="sm" />
                        <div className="min-w-0 flex-1">
                            <p className="text-sm font-semibold text-slate-800">
                                {style.label || payload.type}
                                {payload.employerName ? ` · ${payload.employerName}` : ''}
                            </p>
                            {payload.workerInfoDesc && (
                                <p className="text-xs text-slate-500 mt-0.5 line-clamp-2">
                                    {payload.workerInfoDesc}
                                </p>
                            )}
                        </div>
                    </div>,
                    { icon: false }
                );
            }
            // Signal other notification consumers (e.g. the bell) to refetch.
            bumpRefresh();
        });

        connection.start().catch(console.error);

        return () => { connection.stop(); };
    }, [isAuthReady, userId, bumpRefresh]);

    return <ToastContainer />;
}
