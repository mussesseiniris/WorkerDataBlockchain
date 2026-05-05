'use client'

interface FileUploadProps {
    label: string
    onFileChange: (file: File) => void
    required?: boolean
}

export default function FileUpload({ label, onFileChange, required }: FileUploadProps) {
    return (
        <div className="flex flex-col gap-1">
            <label className="text-sm font-medium text-gray-700">
                {label} {required && <span className="text-red-500">*</span>}
            </label>
            <input
                type="file"
                onChange={(e) => {
                    const file = e.target.files?.[0]
                    if (file) {
                        onFileChange(file)
                    }
                }}
                className="border border-gray-300 rounded-lg px-3 py-2 text-sm text-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
        </div>
    )
}
