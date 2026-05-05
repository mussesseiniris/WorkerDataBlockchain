"use client";

import { useState } from "react";
import { X, Info } from "lucide-react";
import RevokeConfirmModal from "./RevokeConfirmModal";


export interface ActiveField {
    id: string;
    label: string;
}

export interface ActiveRowData {
    id: string;
    company: string;
    date: string;
    workerInfo: ActiveField[];
    reason: string;
}

interface ActiveRowProps extends ActiveRowData {
    onRevoke: (itemId: string, fieldIds: string[]) => void;
}

export default function ActiveRow({ id, company, date, workerInfo, reason, onRevoke }: ActiveRowProps) {

    const [checked, setChecked] = useState<string[]>([]);

    const toggle = (fieldId: string) => {
        setChecked((prev) =>
            prev.includes(fieldId) ? prev.filter((f) => f !== fieldId) : [...prev, fieldId]
        );
    };

    const [showModal, setShowModal] = useState(false);


    return (
        <div className="flex justify-between items-center px-5 py-4 border-b border-gray-200 ">

            <div className="flex flex-col gap-2">
                <p className="text-sm text-gray-900 ">{company}</p>
                <p className="text-xs text-gray-500 ">{date}</p>

                <div className="flex gap-3 flex-wrap">
                    {workerInfo.map((field) => (
                        <label
                            key={field.id}
                            className="flex items-center gap-2 border border-gray-300  rounded-md px-3 py-1 text-sm text-gray-700 cursor-pointer"
                        >
                            <input
                                type="checkbox"
                                checked={checked.includes(field.id)}
                                onChange={() => toggle(field.id)}
                                className="cursor-pointer"
                            />
                            {field.label}
                        </label>
                    ))}
                </div>
                <div className="flex items-center gap-1 text-xs text-gray-500 ">
                    <Info size={14} />
                    <span>{reason}</span>
                </div>
            </div>

            <button
                onClick={() => setShowModal(true)}
                disabled={checked.length === 0}
                className="bg-red-400 hover:bg-red-500 disabled:opacity-40 disabled:cursor-not-allowed text-white rounded-xl p-2 cursor-pointer transition-colors"
            >
                <X size={20} strokeWidth={2.5} />
            </button>

            {showModal && (
                <RevokeConfirmModal
                    company={company}
                    selectedFields={workerInfo
                        .filter((f) => checked.includes(f.id))
                        .map((f) => f.label)}
                    onConfirm={() => {
                        onRevoke(id, checked);
                        setShowModal(false);
                    }}
                    onCancel={() => setShowModal(false)}
                />
            )}


        </div>
    );
}