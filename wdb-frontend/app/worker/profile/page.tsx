'use client'

import TopBar from '@/components/ui/TopBar'
import UserInfoCard from './components/UserInfoCard'
import { useEffect, useState } from 'react'
import { WorkerInfoItem } from './type'
import BasicProfileCard from './components/BasicProfileCard'
import { getWorkerProfile, updateWorkerProfile } from '@/lib/api/workerApi'
import { useRouter } from 'next/navigation'
import router from 'next/dist/shared/lib/router/router'



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
    const router = useRouter()
    const [allDate, setAllData] = useState<WorkerInfoItem[]>([])
    const [workerId, setWorkerId] = useState('')
    const [userName, setUserName] = useState('')
    const [token, setToken] = useState('')

    useEffect(() => {
        const storedToken = localStorage.getItem('accessToken')
        const id = localStorage.getItem('userId')
        const name = localStorage.getItem('userName')
        if (!storedToken || !id || !name) {
            router.push('/login')
            return
        }
        setToken(storedToken)

        setWorkerId(id ?? '')
        setUserName(name ?? '')

        const fetchData = async () => {
            try {
                const data = await getWorkerProfile(storedToken)
                setAllData(Array.isArray(data) ? data : [data])
            } catch (error) {
                console.error('Failed to fetch worker profile:', error)
            }
        }
        fetchData()
    }, [router])

    const handlesave = async (desc: string, value: string) => {
        try {
            if (!token) {
                router.push('/login')
                return
            }

            await updateWorkerProfile(token, desc, value)
            const updatedData = await getWorkerProfile(token)
            setAllData(Array.isArray(updatedData) ? updatedData : [updatedData])
        } catch (error) {
            console.error('Failed to update worker profile:', error)
        }
    }

    return (
        <div className="flex flex-col h-screen bg-gray-50">
            <TopBar />
            <div className="flex-1 overflow-y-auto px-8 py-8 flex flex-col gap-6">
                <UserInfoCard data={allDate} workerId={workerId} userName={userName} />
                <BasicProfileCard data={allDate} onSave={handlesave} />
                <PlaceholderCard title="Health Considerations" />
                <PlaceholderCard title="Emergency Contact" />
                <PlaceholderCard title="Certifications" />
                <PlaceholderCard title="Work Restrictions" />
                <PlaceholderCard title="PPE Requirements" />
            </div>
        </div>
    )
}

