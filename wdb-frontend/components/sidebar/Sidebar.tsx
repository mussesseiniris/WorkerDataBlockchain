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
    {label : 'Dashboard', icon : LayoutDashboard , href: '/worker/dashboard'},
    {label : 'Request Access', icon: KeyRound, href: '/worker/request-access'},
    {label : 'Access List', icon:  List , href : '/worker/access-list'},
    {label : 'Data Exchange', icon: ArrowLeftRight, href: '/worker/data-exchange'},
    {label : 'Profile', icon :User, href: '/worker/profile'},
]

export default function Sidebar() {
    const [collapsed, setCollapsed] = useState(false)

    return(<div className={`
            flex flex-col h-screen bg-gray-900 text-white
            transition-all duration-300
            ${collapsed ? 'w-16' : 'w-56'}
        `}>
        {/* 收缩按钮 */}
            <div className="flex justify-end p-3">
                <button
                    onClick={() => setCollapsed(!collapsed)}
                    className="p-1 rounded hover:bg-gray-700"
                >
                    {collapsed ? <ChevronRight size={20} /> : <ChevronLeft size={20} />}
                </button>
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


