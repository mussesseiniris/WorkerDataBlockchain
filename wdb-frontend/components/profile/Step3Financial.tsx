'use client'

import TextInput from '@/components/ui/TextInput'

interface Step3Data {
    irdNumber: string
    bankAccount: string
}

interface Step3Props {
    data: Step3Data
    onChange: (data: Step3Data) => void
}

export default function Step3Financial({ data, onChange }: Step3Props) {
    const update = (field: keyof Step3Data, value: string) => {
        onChange({ ...data, [field]: value })
    }

    const formatBankAccount = (value: string) => {
        // 格式化银行账号 XX-XXXX-XXXXXXX-XX
        const digits = value.replace(/\D/g, '')
        const parts = [
            digits.slice(0, 2),
            digits.slice(2, 6),
            digits.slice(6, 13),
            digits.slice(13, 15),
        ]
        return parts.filter(Boolean).join('-')
    }

    const formatIRD = (value: string) => {
        // 只允许 8-9 位数字
        return value.replace(/\D/g, '').slice(0, 9)
    }

    return (
        <div className="flex flex-col gap-6">
            <h2 className="text-xl font-semibold text-gray-800">Financial Information</h2>

            <TextInput
                label="IRD Number"
                value={data.irdNumber}
                onChange={(v) => update('irdNumber', formatIRD(v))}
                placeholder="123456789"
                required
            />
            <p className="text-xs text-gray-500 -mt-4">8-9 digits only</p>

            <TextInput
                label="Bank Account"
                value={data.bankAccount}
                onChange={(v) => update('bankAccount', formatBankAccount(v))}
                placeholder="XX-XXXX-XXXXXXX-XX"
                required
            />
        </div>
    )
}