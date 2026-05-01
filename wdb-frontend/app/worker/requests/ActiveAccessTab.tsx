"use client";

import ActiveRow, { ActiveRowData } from "../../components/ActiveRow";

interface ActiveAccessTabProps {
    permission: ActiveRowData[];
    onRevoke: (itemId: string, workerInfoId: string[]) => void;
}


export default function ActiveAccessTab({ permission, onRevoke }: ActiveAccessTabProps) {
    if (permission.length === 0) {
        return <p className="text-sm text-gray-500">No active access grants.</p>;
    }

    return (
        <div>
            {permission.map((item) => (
                <ActiveRow key={item.id} {...item} onRevoke={onRevoke} />
            ))}
        </div>
    );
}