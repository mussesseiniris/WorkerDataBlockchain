"use client";
// Worker requests: view, approve, or reject employer data access requests
import { FetchApi } from '../../../lib/api';
import { useState, ReactNode, useEffect } from 'react';
import RequestRow, { Row, Field } from "../../components/RequestRow"
import RequestRowTab from './ActiveRequestTab';
import ActiveAccessTab from "./ActiveAccessTab";


interface TabProps {
    id: string;
    label: string;
    children?: ReactNode;
}




export default function Page() {

    const [activeTab, setActiveTab] = useState<string>("active-request");
    const [rows, setRows] = useState<Row[]>([]);
    const [isLoading, setIsLoading] = useState(false);
    const [errorMsg, setErrorMsg] = useState('');
    const [activeAccessRefresh, setActiveAccessRefresh] = useState(0);

    const workerId = "019d9918-c72f-7e3e-baa0-9de869651370";

    useEffect(() => {
        getRows(workerId);
    }, []);

    async function getRows(workerid: string) {
        setIsLoading(true);
        setErrorMsg('');
        try {
            var rows = await FetchApi(
                `/api/Worker/${workerid}/rows`,
            )
            if (!rows) {
                alert("There are no current requests");
                return;
            }
            setRows(rows);
        } catch (error) {
            setErrorMsg(`${error}`)
        } finally {
            setIsLoading(false);
        }
    }

    const tabs: TabProps[] = [
        {
            id: "active-request",
            label: "Active Request",
            children:
                <div>
                    {isLoading && <p className="text-sm text-gray-500">Loading...</p>}
                    {errorMsg && <p className="text-sm text-red-500">{errorMsg}</p>}
                    {!isLoading && <RequestRowTab requests={rows} />}
                </div>
        },
        {
            id: "active-access",
            label: "Active Access",
            children:
                <div>
                    <ActiveAccessTab
                        workerId={workerId}
                        refreshTrigger={activeAccessRefresh}
                        onRevoke={async (_requestId, workerInfoIds) => {
                            try {
                                await Promise.all(
                                    workerInfoIds.map((id) => FetchApi(`/api/permission/${id}/reject`))
                                );
                                setActiveAccessRefresh((n) => n + 1);
                                getRows(workerId);
                            } catch (error) {
                                alert(`Revoke failed: ${error}`);
                            }
                        }}
                    />

                </div>
        }
    ];

    const activeContent = tabs.find((t) => t.id === activeTab)?.children;


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
                                "border-transparent text-gray-500 hover:text-gray-700"}`}
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