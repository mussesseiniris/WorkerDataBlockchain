"use client";
// Worker requests: view, approve, or reject employer data access requests
// import { FetchApi } from '@lib/api';

import { useState, ReactNode } from 'react';
import styles from "./page.module.css";

interface TabProps {
    id: string;
    label: string;
    children?: ReactNode;
}

const tabs: TabProps[] = [
    {
        id: "active-request",
        label: "Active Request",
        children: 
        <div>
            <p>
                List of permission requests
            </p>  
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
]



export default function Page() { 
    
   const [activeTab, setActiveTab] = useState<string>(tabs[0].id);
   const activeContent = tabs.find((t) => t.id == activeTab)?.children;
    
    
    return (
        <main className={styles.main}>
            <div className={styles.heading}>
                <h1>Data Access</h1>
            </div>
            <div className={styles.tablist}>
                {tabs.map(({id, label}) => 
                <button 
                    key={id}
                    onClick={() => setActiveTab(id)}
                    className={`${styles.tabBtn} ${activeTab === id ? styles.tabBtnActive : ""}`}
                >
                    {label}
                </button>
                )}
            </div>
            <div 
            className={styles.tabContent}
            id={`panel-${activeTab}`}
            >
                {activeContent}
            </div>
        </main>
        
    ) }
