"use client";

import { useState } from "react";

interface Field {
    label: string;
    checked: boolean;
}

export interface Request {
    id: string;
    company: string;
    date:string;
    fields: Field[];
    reason: string;
}

export default function RequestRow({ id, company, date, fields, reason}: Request){
    const [checkedFields, setCheckedFields] = useState<Field[]>(fields);

    const toggleField = (label: string) => {
        setCheckedFields((prev) =>
        prev.map((f) => f.label === label ? {...f, checked: !f.checked}: f)
        );
    };

    return (
        <div>

        <div>
            <p> {company} </p>
            <p> {date} </p>
            <div>
                {checkedFields.map((field) => (
                    <label key={field.label}>
                        <input 
                        type="checkbox"
                        checked={field.checked}
                        onChange={() => toggleField(field.label)}
                        />
                        {field.label}
                    </label>
                ))}
            </div>
            <p> {reason} </p>
        </div>

        <div>
            <button>✔</button>
            <button>✖</button>
        </div>

        </div>
    )
}