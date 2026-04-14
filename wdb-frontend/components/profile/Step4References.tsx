'use client'

import TextInput from '@/components/ui/TextInput'
import TextArea from '@/components/ui/TextArea'

interface Reference {
    name: string
    company: string
    phone: string
    email: string
}

interface Step4Data {
    healthConsiderations: string
    references: Reference[]
}

interface Step4Props {
    data: Step4Data
    onChange: (data: Step4Data) => void
}

const emptyReference: Reference = {
    name: '',
    company: '',
    phone: '',
    email: '',
}

export default function Step4References({ data, onChange }: Step4Props) {
    const updateReference = (index: number, field: keyof Reference, value: string) => {
        const updated = data.references.map((ref, i) =>
            i === index ? { ...ref, [field]: value } : ref
        )
        onChange({ ...data, references: updated })
    }

    const addReference = () => {
        onChange({ ...data, references: [...data.references, { ...emptyReference }] })
    }

    const removeReference = (index: number) => {
        onChange({ ...data, references: data.references.filter((_, i) => i !== index) })
    }

    return (
        <div className="flex flex-col gap-6">
            <h2 className="text-xl font-semibold text-gray-800">Health & References</h2>

            <TextArea
                label="Health Considerations"
                value={data.healthConsiderations}
                onChange={(v) => onChange({ ...data, healthConsiderations: v })}
                placeholder="Please list any health considerations relevant to your work (e.g. latex allergy)"
            />

            <div className="flex flex-col gap-4">
                <p className="text-sm font-medium text-gray-700">References</p>

                {data.references.map((ref, index) => (
                    <div key={index} className="flex flex-col gap-3 p-4 bg-gray-50 rounded-lg">
                        <div className="flex justify-between items-center">
                            <p className="text-sm font-medium text-gray-600">
                                Reference {index + 1}
                            </p>
                            {index > 0 && (
                                <button
                                    onClick={() => removeReference(index)}
                                    className="text-xs text-red-500 hover:text-red-700"
                                >
                                    Remove
                                </button>
                            )}
                        </div>
                        <div className="grid grid-cols-2 gap-3">
                            <TextInput
                                label="Full Name"
                                value={ref.name}
                                onChange={(v) => updateReference(index, 'name', v)}
                                placeholder="Jane Doe"
                            />
                            <TextInput
                                label="Company"
                                value={ref.company}
                                onChange={(v) => updateReference(index, 'company', v)}
                                placeholder="Company Name"
                            />
                            <TextInput
                                label="Phone"
                                value={ref.phone}
                                onChange={(v) => updateReference(index, 'phone', v)}
                                placeholder="+64 21 123 4567"
                            />
                            <TextInput
                                label="Email"
                                value={ref.email}
                                onChange={(v) => updateReference(index, 'email', v)}
                                placeholder="jane@example.com"
                            />
                        </div>
                    </div>
                ))}

                <button
                    onClick={addReference}
                    className="flex items-center gap-2 text-sm text-blue-600 hover:text-blue-800 font-medium"
                >
                    + Add Another Reference
                </button>
            </div>
        </div>
    )
}