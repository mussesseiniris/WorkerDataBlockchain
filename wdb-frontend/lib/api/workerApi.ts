import { Step1Data } from "@/app/worker/profile/Step1PersonalInfo";

const BASE_URL = '/api/worker'

export async function getWorkerProfile() {
    const response = await fetch(`${BASE_URL}/profile`, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
        },
    })

    if (!response.ok) {
        throw new Error('Failed to fetch worker profile')
    }

    return response.json() as Promise<Step1Data>
}

export async function updateWorkerProfile(data: Step1Data) {
    const response = await fetch(`${BASE_URL}/profile`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
    })

    if (!response.ok) {
        throw new Error('Failed to update worker profile')
    }

    return response.json()
}