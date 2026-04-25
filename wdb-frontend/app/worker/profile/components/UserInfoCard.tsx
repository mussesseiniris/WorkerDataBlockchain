'use client'

import { User } from 'lucide-react'
import { WorkerInfoItem } from '../type'

interface UserInfoCardProps {
    data: WorkerInfoItem[]
    workerId: string
    userName: string
}

export default function UserInfoCard({ data, workerId, userName }: UserInfoCardProps) {
    const displayName = data.find(item => item.desc === 'fullName')?.value ?? userName

    return (
        <div className="bg-white rounded-xl border border-gray-200 p-6">

            {/* Header */}
            <h2 className="text-lg font-semibold text-gray-800 mb-6">My Profile</h2>

            {/* Avatar + Info */}
            <div className="flex items-center gap-6">

                {/* Avatar */}
                <div className="w-20 h-20 rounded-full bg-gray-200 flex items-center justify-center flex-shrink-0">
                    <User size={36} className="text-gray-400" />
                </div>

                {/* Info */}
                <div className="flex flex-col gap-3">
                    <p className="text-xl font-semibold text-gray-900">{displayName}</p>
                    <p className="text-sm text-gray-500">Worker ID: {workerId}</p>
                </div>

            </div>
        </div>
    )
}