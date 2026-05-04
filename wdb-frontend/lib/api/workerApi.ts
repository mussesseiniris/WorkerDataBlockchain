import { WorkerInfoItem } from "@/app/worker/profile/type";

const BASE_URL = `${process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5258'}/api/worker/info`



// a method that get worker repority from http in order to translate data to frontend
// parament is worker id and had statement by type.ts
export async function getWorkerProfile(token: string): Promise<WorkerInfoItem[]> {
    const response = await fetch(BASE_URL, {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
    });

    if (!response.ok) {
        throw new Error('Failed to fetch worker profile')
    }

    return response.json() as Promise<WorkerInfoItem[]> // tell program json's struction is same with interface:WorkerInfoItem
}


//when user add more info and click save, this method will pass the data to http and back to endfront
export async function addWorkerProfile(token: string, desc: string, value: string) {
    const response = await fetch(BASE_URL, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ desc, value }),
    });

    if (!response.ok) {
        throw new Error('Failed to add worker info')
    }
    return response.json();
}




// when user had edit this method can update or cover the newest dat to http then pass to endfront
export async function updateWorkerProfile(token: string, desc: string, value: string) {
    const response = await fetch(BASE_URL, {
        method: 'PUT',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ desc, value }),
    })

    if (!response.ok) {
        throw new Error('Failed to update worker profile')
    }

    return response.json()
}