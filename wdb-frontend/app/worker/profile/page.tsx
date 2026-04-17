'use client'
import { useState } from 'react'
import Step1PersonalInfo, { Step1Data } from './Step1PersonalInfo'


const initialData: Step1Data = {
    fullName: '',
    pronouns: '',
    email: '',
    phone: '',
    address: '',
    dateOfBirth: '',
    emergencyContactName: '',
    emergencyContactPhone: '',
}

export default function ProfilePage() {
     const [formData, setFormData] = useState<Step1Data>(initialData)
    return (
        <div className="p-6">
            <h1 className="text-2xl font-bold text-gray-800 mb-6">My Profile</h1>
            <Step1PersonalInfo 
                data={formData}
                onChange={setFormData}
            />
        </div>
    )
}

