'use client'

interface RadioGroupProps {
    label: string
    value: string
    onChange: (value: string) => void
    options: { label: string; value: string }[]
    required?: boolean
}

export default function RadioGroup({ label, value, onChange, options, required }: RadioGroupProps) {
    return (
        <div className="flex flex-col gap-1">
            <label className="text-sm font-medium text-gray-700">
                {label} {required && <span className="text-red-500">*</span>}
            </label>
            {options.map((option) => (
                <div key={option.value} className="flex items-center">
                    <input
                        type="radio"
                        name={label}
                        value={option.value}
                        checked={value === option.value}
                        onChange={(e) => onChange(e.target.value)}
                        className="mr-2 text-blue-500 focus:ring-blue-500"
                    />
                    <label className="text-sm text-gray-700">{option.label}</label>
                </div>
            ))}
        </div>
    )
}
