'use client'

import RadioGroup from '@/components/ui/RadioGroup'
import FileUpload from '@/components/ui/FileUpload'
import TextArea from '@/components/ui/TextArea'

interface Step2Data {
    workRights: string
    workRightsProof: File | null
    criminalRecordSignature: string
    certification: File | null
}

interface Step2Props {
    data: Step2Data
    onChange: (data: Step2Data) => void
}

const workRightsOptions = [
    { label: 'Citizen', value: 'citizen' },
    { label: 'Permanent Resident', value: 'pr' },
    { label: 'Work Visa', value: 'work_visa' },
    { label: 'I have the legal right to work in New Zealand', value: 'legal_right' },
    { label: 'I require sponsorship', value: 'need_sponsorship' },
]

export default function Step2WorkRights({ data, onChange }: Step2Props) {
    const update = (field: keyof Step2Data, value: string | File | null) => {
        onChange({ ...data, [field]: value })
    }

    return (
        <div className="flex flex-col gap-6">
            <h2 className="text-xl font-semibold text-gray-800">Work Rights & Compliance</h2>

            <RadioGroup
                label="Work Rights"
                value={data.workRights}
                onChange={(v) => update('workRights', v)}
                options={workRightsOptions}
                required
            />

            <FileUpload
                label="Work Rights Proof"
                onFileChange={(file) => update('workRightsProof', file)}
                required
            />

            <div className="flex flex-col gap-2">
                <p className="text-sm font-medium text-gray-700">Criminal Record Authorization</p>
                <div className="p-4 bg-gray-50 rounded-lg text-sm text-gray-600 mb-2">
                    I authorise the release of my criminal record report to the requesting 
                    employer for employment verification purposes, in accordance with 
                    New Zealand Ministry of Justice processes.
                </div>
                <TextArea
                    label="Digital Signature (please enter your full name as your electronic signature)"
                    value={data.criminalRecordSignature}
                    onChange={(v) => update('criminalRecordSignature', v)}
                    placeholder="Enter your full name to confirm authorisation"
                    required
                />
            </div>

            <FileUpload
                label="Certification"
                onFileChange={(file) => update('certification', file)}
            />
        </div>
    )
}
