'use client'

import { useState } from 'react'
import { Pencil, Check } from 'lucide-react'
import TextInput from '@/components/ui/TextInput'
import Dropdown from '@/components/ui/Dropdown'
import DatePicker from '../../../components/ui/DatePicker'

export interface Step1Data {
    fullName: string
    pronouns: string
    email: string
    phone: string
    address: string
    dateOfBirth: string
    emergencyContactName: string
    emergencyContactPhone: string
}

interface Step1Props {
    data: Step1Data
    onChange: (data: Step1Data) => void
}

const pronounOptions = [
    { label: 'He/Him', value: 'he/him' },
    { label: 'She/Her', value: 'she/her' },
    { label: 'They/Them', value: 'they/them' },
    { label: 'Prefer not to say', value: 'prefer_not_to_say' },
]

const DisplayField = ({ label, value }: { label: string; value: string }) => (
    <div className="flex flex-col gap-2">
        <p className="text-sm font-medium text-gray-700">{label}</p>
        <p className="text-lg text-gray-900">{value}</p>
    </div>
)

export default function Step1PersonalInfo({ data, onChange }: Step1Props) {
    const [isEditing, setIsEditing] = useState(false)

    const update = (field: keyof Step1Data, value: string) => {
        onChange({ ...data, [field]: value })
    }

        return (
        <div className="flex flex-col gap-6">

            {/* title and Edit button */}
            <div className="flex justify-between items-center">
                <h2 className="text-xl font-semibold text-gray-800">Personal Information</h2>
                <button
                    onClick={() => setIsEditing(!isEditing)}
                    className="flex items-center gap-1 text-sm text-blue-600 hover:text-blue-800"
                >
                    {isEditing ? (
                        <>
                            <Check size={16} />
                            <span>Done</span>
                        </>
                    ) : (
                        <>
                            <Pencil size={16} />
                            <span>Edit</span>
                        </>
                    )}
                </button>
            </div>

            {isEditing ? (
                // edit mode
                <>
                    <div className="grid grid-cols-2 gap-4">
                        <TextInput
                            label="Full Name"
                            value={data.fullName}
                            onChange={(v) => update('fullName', v)}
                            placeholder="John Doe"
                            required
                        />
                        <Dropdown
                            label="Preferred Pronouns"
                            value={data.pronouns}
                            onChange={(v) => update('pronouns', v)}
                            options={pronounOptions}
                        />
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <TextInput
                            label="Email"
                            value={data.email}
                            onChange={(v) => update('email', v)}
                            placeholder="john@example.com"
                            required
                        />
                        <TextInput
                            label="Phone Number"
                            value={data.phone}
                            onChange={(v) => update('phone', v)}
                            placeholder="+64 21 123 4567"
                        />
                    </div>

                    <TextInput
                        label="Address"
                        value={data.address}
                        onChange={(v) => update('address', v)}
                        placeholder="123 Main Street, Wellington"
                    />

                    <DatePicker
                        label="Date of Birth"
                        value={data.dateOfBirth ? new Date(data.dateOfBirth) : null}
                        onChange={(date) => update('dateOfBirth', date ? date.toISOString().split('T')[0] : '')}
                        required
                    />

                    <div className="flex flex-col gap-2">
                        <p className="text-sm font-medium text-gray-700">Emergency Contact</p>
                        <div className="grid grid-cols-2 gap-4 p-4 bg-gray-50 rounded-lg">
                            <TextInput
                                label="Name"
                                value={data.emergencyContactName}
                                onChange={(v) => update('emergencyContactName', v)}
                                placeholder="Jane Doe"
                            />
                            <TextInput
                                label="Phone"
                                value={data.emergencyContactPhone}
                                onChange={(v) => update('emergencyContactPhone', v)}
                                placeholder="+64 21 987 6543"
                            />
                        </div>
                    </div>
                </>
            ) : (
                // read only mode
                <>
                    <div className="grid grid-cols-2 gap-4">
                        <DisplayField label="Full Name" value={data.fullName} />
                        <DisplayField label="Preferred Pronouns" value={data.pronouns} />
                    </div>
                    <div className="grid grid-cols-2 gap-4">
                        <DisplayField label="Email" value={data.email} />
                        <DisplayField label="Phone Number" value={data.phone} />
                    </div>
                    <DisplayField label="Address" value={data.address} />
                    <DisplayField label="Date of Birth" value={data.dateOfBirth} />
                    <div className="flex flex-col gap-2">
                        <p className="text-sm font-medium text-gray-700">Emergency Contact</p>
                        <div className="grid grid-cols-2 gap-4 p-4 bg-gray-50 rounded-lg">
                            <DisplayField label="Name" value={data.emergencyContactName} />
                            <DisplayField label="Phone" value={data.emergencyContactPhone} />
                        </div>
                    </div>
                </>
            )}
        </div>
    )
}