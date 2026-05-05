"use client";

interface RevokeConfirmModalProps {
    company: string;
    selectedFields: string[];
    onConfirm: () => void;
    onCancel: () => void;
}

export default function RevokeConfirmModal({ company, selectedFields, onConfirm, onCancel }: RevokeConfirmModalProps) {
    return (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
            <div className="bg-white  rounded-2xl shadow-lg p-6 w-full max-w-sm mx-4">

                <h2 className="text-base font-semibold text-gray-900  mb-4">
                    Confirm to revoke the following access from <span className="text-gray-500">{company}</span>?
                </h2>

                <ul className="mb-6 flex flex-col gap-2">
                    {selectedFields.map((field) => (
                        <li
                            key={field}
                            className="text-sm text-gray-700 border border-gray-200  rounded-md px-3 py-1.5"
                        >
                            {field}
                        </li>
                    ))}
                </ul>

                <div className="flex gap-3 justify-end">
                    <button
                        onClick={onCancel}
                        className="px-4 py-2 text-sm rounded-lg border border-gray-300  text-gray-700  hover:bg-gray-100 transition-colors cursor-pointer"
                    >
                        Cancel
                    </button>
                    <button
                        onClick={onConfirm}
                        className="px-4 py-2 text-sm rounded-lg bg-red-500 hover:bg-red-600 text-white transition-colors cursor-pointer"
                    >
                        Revoke
                    </button>
                </div>

            </div>
        </div>
    );
}