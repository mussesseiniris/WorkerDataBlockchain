"use client";

import RequestRow, { Row, Field } from "../../components/RequestRow"

interface RowList {
    requests: Row[]
}
export default function RequestRowTab({requests}: RowList){
    if (requests.length === 0) {
        return <p className="text-sm text-gray-500">No active permission requests</p>;
    }

     return (
        <div>
            {requests.map((item) => (
                <RequestRow key={item.id} {...item} />
            ))}
        </div>
    );
}