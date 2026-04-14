import MultiStepForm from '@/components/profile/MultiStepForm'

export default function ProfilePage() {
    return (
        <div className="p-6">
            <h1 className="text-2xl font-bold text-gray-800 mb-6">Complete Your Profile</h1>
            <MultiStepForm />
        </div>
    )
}

