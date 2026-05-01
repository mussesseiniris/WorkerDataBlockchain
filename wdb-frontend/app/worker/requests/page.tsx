"use client";
// Worker requests: view, approve, or reject employer data access requests
import { FetchApi } from '../../../lib/api';
import { useState, ReactNode } from 'react';
import RequestRow, { Row, Field } from "../../components/RequestRow"
import ActiveAccessTab from "./ActiveAccessTab";


interface TabProps {
    id: string;
    label: string;
    children?: ReactNode;
}

type Permission = {
    id: string;
    info_id: string;
    request_id: string;
    status: string;
}

type Request = {
    id: string;
    created_at: string;
    employer_id: string;
    worker_id: string;
    reason: string;
}

type WorkerInfo = {
    id: string;
    desc: string;
}


const tabs: TabProps[] = [
    {
        id: "active-request",
        label: "Active Request",
        children:
            <div>
            //List of RequestRows
            </div>
    },
    {
        id: "active-access",
        label: "Active Access",
        children:
            <div>
                <ActiveAccessTab
                    permission={[
                        {
                            id: "1",
                            company: "Acme Corp",
                            date: "2025-01-15",
                            reason: "Employment verification",
                            workerInfo: [
                                { id: "f1", label: "Name" },
                                { id: "f2", label: "Email" },
                                { id: "f3", label: "Work History" },
                            ],
                        },
                        {
                            id: "2",
                            company: "Globex Inc",
                            date: "2025-03-02",
                            reason: "Selling your data to other people",
                            workerInfo: [
                                { id: "f4", label: "Name" },
                                { id: "f5", label: "Phone" },
                            ],
                        },
                    ]}
                    onRevoke={(permissionId, workerInfoId) => console.log("Revoke", permissionId, workerInfoId)}
                />



            </div>
    }
]



export default function Page() {

    const [activeTab, setActiveTab] = useState<string>(tabs[0].id);
    const activeContent = tabs.find((t) => t.id == activeTab)?.children;

    const [permissions, setPermissions] = useState<Permission[]>([]);
    const [request, setRequests] = useState<Request[]>([]);
    const [workerInfo, setWorkerInfo] = useState<WorkerInfo[]>([]);

    const [isLoading, setIsLoading] = useState(false);
    const [errorMsg, setErrorMsg] = useState('');

    async function getRows(workerid: string) {
        setIsLoading(true);
        setErrorMsg('');
        try {
            var requests = await FetchApi(
                `/api/Worker/${workerid}/requests`,
            )
            if (!requests) {
                alert("There are no current requests");
                return;
            }
            setRequests(requests);


            var permissions = await FetchApi(
                `/api/Worker/${workerid}/permissions`,
            );
            setPermissions(permissions);


            var workerinfo = await FetchApi(
                `/api/Worker/${workerid}/info`,
            )
            setWorkerInfo(workerinfo);


        } catch (error) {
            setErrorMsg(`${error}`)
        } finally {
            setIsLoading(false);
        }
    }

    return (
        <main className="p-8">
            <div>
                <h1 className="text-2xl font-semibold mb-6 text-gray-900">Data Access</h1>


            </div>
            <div className="flex border-b border-gray-200">
                {tabs.map(({ id, label }) =>
                    <button
                        key={id}
                        onClick={() => setActiveTab(id)}
                        className={`
                        px-5 py-2.5 text-sm font-medium border-b-2 -mb-px transition-colors cursor-pointer 
                        ${activeTab === id ? "border-gray-900 text-gray-900" :
                                "border-transparent text-gray-500 hover:text-gray-700 "}`}
                    >
                        {label}
                    </button>
                )}
            </div>
            <div
                className="mt-6">
                {activeContent}
            </div>
        </main>

    )
}