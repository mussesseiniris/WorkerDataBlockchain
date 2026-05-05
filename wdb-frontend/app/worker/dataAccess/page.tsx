"use client";
// Worker requests: view, approve, or reject employer data access requests
import { FetchApi } from '../../../lib/api';
import { useState, ReactNode, useEffect } from 'react';
import { Row } from "../../components/RequestRow"
import ActiveRequestTab from './ActiveRequestTab';

interface TabProps {
    id: string;
    label: string;
    children?: ReactNode;
}


export default function Page() {

    const [activeTab, setActiveTab] = useState<string>("active-request");
    const tabs: TabProps[] = [
        {
            id: "active-request",
            label: "Active Request",
            children:
                <div>
                    <ActiveRequestTab />
                </div>
        },
        {
            id: "active-access",
            label: "Active Access",
            children:
                <div>
                    <p>
                        List of permission approved
                    </p>
                </div>
        }
    ];

    const activeContent = tabs.find((t) => t.id === activeTab)?.children;

    return (
        <main className="p-8">
            <div>
                <h1 className="text-2xl font-semibold mb-6">Data Access</h1>
            </div>
            <div className="flex border-b border-gray-200 dark:border-gray-700">
                {tabs.map(({ id, label }) =>
                    <button
                        key={id}
                        onClick={() => setActiveTab(id)}
                        className={`
                        px-5 py-2.5 text-sm font-medium border-b-2 -mb-px transition-colors cursor-pointer 
                        ${activeTab === id ? "border-gray-900 text-gray-900 dark:border-white dark:text-white" :
                                "border-transparent text-gray-500 hover:text-gray-700 dark:hover:text-gray-300"}`
                        }
                    >
                        {label}
                    </button>
                )}
            </div>
            <div className="mt-6">
                {activeContent}
            </div>
        </main>

    )
}
