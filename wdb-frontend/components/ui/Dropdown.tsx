'use client'

interface DropdownProps {
    label: string
    value: string
    onChange: (value: string) => void
    options: { label: string; value: string }[]
    required?: boolean
}

export default function Dropdown({ label, value, onChange, options, required }: DropdownProps) {
    return (
        <div className="flex flex-col gap-1">
            <label className="text-sm font-medium text-gray-700">
                {label} {required && <span className="text-red-500">*</span>}
            </label>
            <select
                value={value}
                onChange={(e) => onChange(e.target.value)}
                className="border border-gray-300 rounded-lg px-3 py-2 text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
                <option value="">Select an option</option>
                {options.map((option) => (
                    <option key={option.value} value={option.value}>
                        {option.label}
                    </option>
                ))}
            </select>
        </div>
    )
}