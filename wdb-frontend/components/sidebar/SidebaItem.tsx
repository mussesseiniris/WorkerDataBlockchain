'use client'

import Link from 'next/link'
import { usePathname } from 'next/navigation'
import { LucideIcon } from 'lucide-react'

interface SidebarItemProps {
    label: string
    icon: LucideIcon
    href: string
    collapsed: boolean
}

export default function SidebarItem({ label, icon: Icon, href, collapsed }: SidebarItemProps) {
    const pathname = usePathname()
    const isActive = pathname === href

    return (
        <Link
            href={href}
            className={`
                flex items-center gap-3 px-3 py-2 rounded-lg
                transition-colors duration-200
                ${isActive ? 'bg-blue-600 text-white' : 'text-gray-300 hover:bg-gray-700'}
            `}
        >
            <Icon size={20} />
            {!collapsed && <span className="text-sm font-medium">{label}</span>}
        </Link>
    )
}