"use client";

import RequestRow, { Row, Field } from "../../components/RequestRow"
import { useState } from "react";

interface RowList {
    requests: Row[]
}

export default function RequestRowTab({requests}: RowList){
    const [rows, setRows] = useState<Row[]>(requests); 
    
    const handleComplete = (id: string) => {
    setRows((prev) => prev.filter((r) => r.id !== id));
    };
    
    if (requests.length === 0) {
        return <p className="text-sm text-gray-500">No active permission requests</p>;
    }

     return (
        <div>
            {rows.map((item) => (
                <RequestRow key={item.id} {...item} onComplete={handleComplete}/>
            ))}
        </div>
    );
}