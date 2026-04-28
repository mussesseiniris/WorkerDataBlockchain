// Worker requests: view, approve, or reject employer data access requests
// import { FetchApi } from '@lib/api';
import { useState, ReactNode } from 'react';

interface TabProps {
    id: string;
    label: string;
    children?: ReactNode;
}

const tabs: TabProps[] = [
    {
        id: "Active request",
        label: "active request",
        children: 
        <div>
            <p>
                List of permission requests
            </p>  
        </div>
    },
    {
        id: "Active access",
        label: "active access"
    }
]



export default function Page() { 
    
   const [activeTab, setActiveTab] = useState<string>(tabs[0].id);
   const activeContent = tabs.find((t) => t.id == activeTab)?.children;
    
    
    return (
        <main>
            <div>
                {tabs.map(({id, label}) => 
                <button 
                key={id}
                aria-selected={activeTab === id}
                onClick={() => setActiveTab(id)}
                >
                </button>
                )}
            </div>
            <div>
                {activeContent}
            </div>
        </main>
        
    ) }
