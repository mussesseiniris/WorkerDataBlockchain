'use client'

import { useState } from "react";
import SidebaItem from "./SidebaItem";


import {
    LayoutDashboard,
    KeyRound,
    List,
    ArrowLeftRight,
    User,
    ChevronLeft,
    ChevronRight
} from 'lucide-react'


const navItems = [
    { label: 'Dashboard', icon: LayoutDashboard, href: '/worker/dashboard' },// Manage user data sharing and view access activity
    { label: 'Data Access', icon: KeyRound, href: '/worker/request-access' }, //Review employer requests, approve data sharing, and revoke active permissions
    { label: 'Audit Log', icon: ArrowLeftRight, href: '/worker/audit-log' }, //Track permission changes, access events, and blockchain records
    { label: 'Personal Data', icon: User, href: '/worker/profile' }, // Allow users to input their info
]

export default function Sidebar() {
    const [collapsed, setCollapsed] = useState(false)

    return (<div className={`
            flex flex-col h-screen bg-gray-900 text-white
            transition-all duration-300
            ${collapsed ? 'w-16' : 'w-56'}
        `}>

        <div className="flex flex-col items-center py-8 px-4 border-b border-gray-700">
            <div className="w-10 h-10 rounded-full bg-blue-500 flex items-center justify-center mb-3">
                <span className="text-white font-bold text-lg">F</span>
            </div>
            <span className="text-sm font-semibold text-white text-center">
                First Step Solution
            </span>
        </div>

        {/* 导航项 */}
        <nav className="flex flex-col gap-1 px-2">
            {navItems.map((item) => (
                <SidebaItem
                    key={item.href}
                    label={item.label}
                    icon={item.icon}
                    href={item.href}
                    collapsed={collapsed}
                />
            ))}
        </nav>
    </div>
    )
}


