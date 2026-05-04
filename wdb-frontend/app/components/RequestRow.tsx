"use client";

import { useState } from "react";
import { FetchApi } from '../../lib/api';
import ConfirmModal from "./ConfirmModal";

export interface Field {
    id: string;
    label: string;
    checked: boolean;
}

export interface Row {
    id: string;
    company: string;
    date:string;
    fields: Field[];
    reason: string;
    onComplete: (id:string) => void;
}


export default function RequestRow({ id, company, date, fields, reason, onComplete}: Row){
    const [checkedFields, setCheckedFields] = useState<Field[]>(fields);
    const [errorMsg, setErrorMsg] = useState('');
    const [pendingAction, setPendingAction] = useState("");
    const [showModal, setShowModal] = useState(false);

    const toggleField = (label: string) => {
        setCheckedFields((prev) =>
        prev.map((f) => f.label === label ? {...f, checked: !f.checked}: f)
        );
    };



    async function changePermission(status: "approve"|"reject") {
        const checkedIds = checkedFields.filter((f) => f.checked).map((f)=> f.id);
        try {
            await Promise.all(
                checkedIds.map((permissionid) =>
                    FetchApi(`/api/Permission/${permissionid}/${status}`,{
                        method: "PATCH"
                    }
                ))
            );
            onComplete(id);
            } catch (error) {
                setErrorMsg(`${error}`)
            }
    }


    return (
    <div className="flex justify-between items-center px-5 py-4 border-b border-gray-200 dark:border-gray-700">

        <div className="flex flex-col gap-2">

            <p className="text-sm text-gray-900 "> {company} </p>
            <p className="text-xs text-gray-500"> {date} </p>

            <div className="flex gap-3 flex-wrap">
                {checkedFields.map((field) => (
                    <label key={field.label}
                    className="flex items-center gap-2 border border-gray-300  rounded-md px-3 py-1 text-sm text-gray-700 cursor-pointer">
                        <input 
                        type="checkbox"
                        checked={field.checked}
                        onChange={() => toggleField(field.label)}
                        className="cursor-pointer"
                        
                        />
                        {field.label}
                    </label>
                ))}
            </div>
            <div className="flex items-center gap-1 text-xs text-gray-500 ">
                    <span>{reason}</span>
            </div>
        </div>

        <div className="flex gap-2">
            <button className="bg-green-500 hover:bg-green-600 text-white rounded-md px-4 py-2 text-base disabled:opacity-40 disabled:cursor-not-allowed cursor-pointer transition-colors"
            disabled={checkedFields.filter(f => f.checked).length === 0}
            onClick={() => {
                setShowModal(true);
                setPendingAction("approve");}}
            >✔</button>
            <button className="bg-red-500 hover:bg-red-600 text-white rounded-md px-4 py-2 text-base disabled:opacity-40 disabled:cursor-not-allowed cursor-pointer transition-colors"
            disabled={checkedFields.filter(f => f.checked).length === 0}
            onClick={() =>  {
                setShowModal(true);
                setPendingAction("reject")}}
            >✖</button>
             {errorMsg && <p className="text-sm text-red-500">{errorMsg}</p>}


             {showModal && (
                <ConfirmModal 
                    company={company}
                    status={pendingAction}
                    selectedFields={checkedFields
                        .filter((f) => f.checked)
                        .map((f) => f.label)}
                    onConfirm={() => {
                        if (pendingAction) changePermission(pendingAction as "approve" | "reject");
                        setShowModal(false);
                    }}
                    onCancel={() => {
                        setShowModal(false);
                        setPendingAction("");
                    }}
                />
             )}
        </div>

    </div>
    )
}