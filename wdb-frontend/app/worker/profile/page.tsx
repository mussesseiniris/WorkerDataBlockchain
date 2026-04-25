'use client'

import TopBar from '@/components/ui/TopBar'
import UserInfoCard from './components/UserInfoCard'
import { useEffect, useState } from 'react'
import { WorkerInfoItem } from './type'
import BasicProfileCard from './components/BasicProfileCard'
import { getWorkerProfile, updateWorkerProfile } from '@/lib/api/workerApi'


const workerId = '1234'
const testName = 'John'

// define a shared component that display the function/ux will finish in next stage.
const PlaceholderCard = ({ title }: { title: string }) => (
    <div className="bg-white rounded-xl border border-gray-200 p-6">
        <h2 className="text-lg font-semibold text-gray-700">{title}</h2>
        <p className="text-sm text-gray-400 mt-2">Coming soon...</p>
    </div>
)

// ProfilePage is the parent component for the personal profile page.
// It fetches worker info from the API on mount and provides
// a save handler to child components.
// Note: workerId is hardcoded for testing. Will be replaced
// with real user context after login integration.
export default function ProfilePage() {
    const [allData, setAllData] = useState<WorkerInfoItem[]>([])

    useEffect(() => {
        const fetchData = async () => {
            const data = await getWorkerProfile(workerId)
            setAllData(Array.isArray(data) ? data : [data])
        }
        fetchData()
    }, [])

    const handleSave = async (desc: string, value: string) => {
        await updateWorkerInfo(workerId, desc, value)
        const updated = await getWorkerProfile(workerId)
        setAllData(Array.isArray(updated) ? updated : [updated])
    }

    return (
        <div className="flex flex-col h-screen bg-gray-50">
            <TopBar />
            <div className="flex-1 overflow-y-auto px-8 py-8 flex flex-col gap-6">
                <UserInfoCard data={allData} workerId={workerId} userName={testName} />
                <BasicProfileCard data={allData} onSave={async (handleSave) => {
                }} />
                <PlaceholderCard title="Health Considerations" />
                <PlaceholderCard title="Emergency Contact" />
                <PlaceholderCard title="Certifications" />
                <PlaceholderCard title="Work Restrictions" />
                <PlaceholderCard title="PPE Requirements" />
            </div>
        </div>
    )
}

function updateWorkerInfo(workerId: any, desc: string, value: string) {
    throw new Error('Function not implemented.')
}

function getWorkerInfo(workerId: any) {
    throw new Error('Function not implemented.')
}

