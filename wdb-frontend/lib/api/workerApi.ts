import { WorkerInfoItem } from "@/app/worker/profile/type";

const BASE_URL = '/api/workerinfo'



// a method that get worker repority from http in order to translate data to frontend
// parament is worker id and had statement by type.ts
export async function getWorkerProfile(workerId: string) {
    const response = await fetch(`${BASE_URL}/${workerId}`, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
        },
    })

    if (!response.ok) {
        throw new Error('Failed to fetch worker profile')
    }

    return response.json() as Promise<WorkerInfoItem[]> // tell program json's struction is same with interface:WorkerInfoItem
}


//when user add more info and click save, this method will pass the data to http and back to endfront
export async function addWorkerProfile(workerId: string, desc: string, value: string) {
    const response = await fetch(`${BASE_URL}/${workerId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ desc, value }),
    })

    if (!response.ok) {
        throw new Error('Failed to add worker info')
    }

    return response.json()
}



// when user had edit this method can update or cover the newest dat to http then pass to endfront
export async function updateWorkerProfile(workerId: string, desc: string, value: string) {
    const response = await fetch(`${BASE_URL}/${workerId}`, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ desc, value }),
    })

    if (!response.ok) {
        throw new Error('Failed to update worker profile')
    }

    return response.json()
}