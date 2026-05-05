"use client";

import { useEffect, useState } from "react";
import ActiveRow, { ActiveRowData } from "../../components/ActiveRow";
import { FetchApi } from "../../../lib/api";

interface ActiveAccessTabProps {
    workerId: string;
    onRevoke: (itemId: string, workerInfoId: string[]) => void;
    refreshTrigger: number;
}

export default function ActiveAccessTab({ workerId, onRevoke, refreshTrigger }: ActiveAccessTabProps) {
    const [permissions, setPermissions] = useState<ActiveRowData[]>([]);
    const [isLoading, setIsLoading] = useState(false);

    useEffect(() => {
        setIsLoading(true);
        FetchApi(`/api/Worker/${workerId}/active-access`)
            .then((data) =>
                setPermissions(
                    data.map((item: any) => ({
                        id: item.id,
                        company: item.company,
                        date: item.date,
                        reason: item.reason,
                        workerInfo: item.workerInfo,
                    }))
                )
            )
            .finally(() => setIsLoading(false));
    }, [workerId, refreshTrigger]);

    if (isLoading) return <p className="text-sm text-gray-500">Loading...</p>;
    if (permissions.length === 0) return <p className="text-sm text-gray-500">No active access grants.</p>;

    return (
        <div>
            {permissions.map((item) => (
                <ActiveRow key={item.id} {...item} onRevoke={onRevoke} />
            ))}
        </div>
    );
}
