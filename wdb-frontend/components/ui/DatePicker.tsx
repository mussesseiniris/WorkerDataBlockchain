'use client'

interface DatePickerProps {
    label: string
    value: Date | null
    onChange: (date: Date | null) => void
    required?: boolean
}

export default function DatePicker({ label, value, onChange, required }: DatePickerProps) {
    return (
        <div className="flex flex-col gap-1">
            <label className="text-sm font-medium text-gray-700">
                {label} {required && <span className="text-red-500">*</span>}
            </label>
            <input
                type="date"
                value={value ? value.toISOString().split('T')[0] : ''}
                onChange={(e) => onChange(e.target.value ? new Date(e.target.value) : null)}
                className="border border-gray-300 rounded-lg px-3 py-2 text-sm text-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
        </div>
    )
}
