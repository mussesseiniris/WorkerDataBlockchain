'use client'

interface SignaturePadProps {
    label: string
    onSignatureChange: (signature: string) => void
    required?: boolean
}

export default function SignaturePad({ label, onSignatureChange, required }: SignaturePadProps) {
    return (
        <div className="flex flex-col gap-1">
            <label className="text-sm font-medium text-gray-700">
                {label} {required && <span className="text-red-500">*</span>}
            </label>
            <canvas
                width={500}
                height={200}
                className="border border-gray-300 rounded-lg px-3 py-2 text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
        </div>
    )
}
