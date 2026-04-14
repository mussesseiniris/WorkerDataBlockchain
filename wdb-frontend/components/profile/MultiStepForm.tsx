'use client'

import { useState } from 'react'
import Step1PersonalInfo from './Step1PersonalInfo'
import Step2WorkRights from './Step2WorkRights'
import Step3Financial from './Step3Financial'
import Step4References from './Step4References'

interface FormData {
    // Step 1
    fullName: string
    pronouns: string
    email: string
    phone: string
    address: string
    dateOfBirth: string
    emergencyContactName: string
    emergencyContactPhone: string
    // Step 2
    workRights: string
    workRightsProof: File | null
    criminalRecordSignature: string
    certification: File | null
    // Step 3
    irdNumber: string
    bankAccount: string
    // Step 4
    healthConsiderations: string
    references: {
        name: string
        company: string
        phone: string
        email: string
    }[]
}

const initialData: FormData = {
    fullName: '',
    pronouns: '',
    email: '',
    phone: '',
    address: '',
    dateOfBirth: '',
    emergencyContactName: '',
    emergencyContactPhone: '',
    workRights: '',
    workRightsProof: null,
    criminalRecordSignature: '',
    certification: null,
    irdNumber: '',
    bankAccount: '',
    healthConsiderations: '',
    references: [{ name: '', company: '', phone: '', email: '' }],
}

const steps = [
    'Personal Info',
    'Work Rights',
    'Financial',
    'References',
]

export default function MultiStepForm() {
    const [currentStep, setCurrentStep] = useState(0)
    const [formData, setFormData] = useState<FormData>(initialData)

    const handleNext = () => {
        if (currentStep < steps.length - 1) {
            setCurrentStep(currentStep + 1)
        }
    }

    const handleBack = () => {
        if (currentStep > 0) {
            setCurrentStep(currentStep - 1)
        }
    }

    const handleSubmit = () => {
        console.log('Form submitted:', formData)
        // TODO: 提交到后端
    }

    return (
        <div className="max-w-2xl mx-auto">

            {/* Step Indicator */}
            <div className="flex items-center justify-between mb-8">
                {steps.map((step, index) => (
                    <div key={index} className="flex items-center">
                        <div className={`
                            flex items-center justify-center w-8 h-8 rounded-full text-sm font-medium
                            ${index <= currentStep
                                ? 'bg-blue-600 text-white'
                                : 'bg-gray-200 text-gray-500'}
                        `}>
                            {index + 1}
                        </div>
                        <span className={`
                            ml-2 text-sm hidden sm:block
                            ${index <= currentStep ? 'text-blue-600 font-medium' : 'text-gray-400'}
                        `}>
                            {step}
                        </span>
                        {index < steps.length - 1 && (
                            <div className={`
                                h-px w-12 mx-3
                                ${index < currentStep ? 'bg-blue-600' : 'bg-gray-200'}
                            `} />
                        )}
                    </div>
                ))}
            </div>

            {/* Form Content */}
            <div className="bg-white rounded-xl shadow p-6 mb-6">
                {currentStep === 0 && (
                    <Step1PersonalInfo
                        data={{
                            fullName: formData.fullName,
                            pronouns: formData.pronouns,
                            email: formData.email,
                            phone: formData.phone,
                            address: formData.address,
                            dateOfBirth: formData.dateOfBirth,
                            emergencyContactName: formData.emergencyContactName,
                            emergencyContactPhone: formData.emergencyContactPhone,
                        }}
                        onChange={(data) => setFormData({ ...formData, ...data })}
                    />
                )}
                {currentStep === 1 && (
                    <Step2WorkRights
                        data={{
                            workRights: formData.workRights,
                            workRightsProof: formData.workRightsProof,
                            criminalRecordSignature: formData.criminalRecordSignature,
                            certification: formData.certification,
                        }}
                        onChange={(data) => setFormData({ ...formData, ...data })}
                    />
                )}
                {currentStep === 2 && (
                    <Step3Financial
                        data={{
                            irdNumber: formData.irdNumber,
                            bankAccount: formData.bankAccount,
                        }}
                        onChange={(data) => setFormData({ ...formData, ...data })}
                    />
                )}
                {currentStep === 3 && (
                    <Step4References
                        data={{
                            healthConsiderations: formData.healthConsiderations,
                            references: formData.references,
                        }}
                        onChange={(data) => setFormData({ ...formData, ...data })}
                    />
                )}
            </div>

            {/* Navigation Buttons */}
            <div className="flex justify-between">
                <button
                    onClick={handleBack}
                    disabled={currentStep === 0}
                    className="px-6 py-2 text-sm font-medium rounded-lg bg-white border border-gray-300 text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                    Back
                </button>
                {currentStep < steps.length - 1 ? (
                    <button
                        onClick={handleNext}
                        className="px-6 py-2 text-sm font-medium rounded-lg bg-blue-600 text-white hover:bg-blue-700"
                    >
                        Next
                    </button>
                ) : (
                    <button
                        onClick={handleSubmit}
                        className="px-6 py-2 text-sm font-medium rounded-lg bg-green-600 text-white hover:bg-green-700"
                    >
                        Submit
                    </button>
                )}
            </div>
        </div>
    )
}

