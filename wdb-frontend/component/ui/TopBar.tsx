'use client'

import { User, Bell, ShieldCheck, ChevronRight, LogOut, ArrowLeft } from 'lucide-react'
import { useState, useRef, useEffect } from 'react'
import { usePathname, useRouter } from 'next/navigation'
import { useAuth } from '@/context/AuthContext'
import { useNotificationRefresh } from '@/context/NotificationRefreshContext'
import Link from 'next/link'


const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5258'

interface NotificationFormat {
    id: string
    employerName: string
    notificationType: string
    workerInfoDesc: string
    notificationTime: string
}

const formatTime = (iso: string) =>
    new Date(iso).toLocaleString('en-US', {
        year: 'numeric', month: 'short', day: 'numeric',
        hour: '2-digit', minute: '2-digit', second: '2-digit',
        hour12: false
    })

export default function TopBar({ role, showBack: showBackProp }: { role: 'employer' | 'worker'; showBack?: boolean }) {
    console.log('TopBar rendered, role:', role)
    const [open, setOpen] = useState(false)
    const [messagesHovered, setMessagesHovered] = useState(false)
    const [notifications, setNotifications] = useState<NotificationFormat[]>([])
    const dropdownRef = useRef<HTMLDivElement>(null)
    const router = useRouter()
    const { userId, token, isAuthReady, logout } = useAuth()
    const { refreshKey } = useNotificationRefresh()
    const pathname = usePathname()
    const pathSegments = pathname.split('/').filter(Boolean)
    const mainPages = ['dashboard', 'dataAccess', 'requests', 'profile']
    const showBack = pathSegments.length >= 2 && !mainPages.includes(pathSegments[1])


    // load notifications
    useEffect(() => {
        if (!isAuthReady || !userId || !token) return
        const load = async () => {
            const res = await fetch(`${API_URL}/api/notification/employer/me?isRead=false`, {
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                }
            })
            if (!res.ok) return
            const response = await res.json()
            const data = Array.isArray(response) ? response : response.data ?? []
            console.log('notifications:', data)
            setNotifications(Array.isArray(response) ? response : response.data ?? [])
        }
        load()
    }, [isAuthReady, userId, token, refreshKey])

    // signify a notification as read
    async function markAsRead(notificationId: string) {
        if (!token) return
        const res = await fetch(`${API_URL}/api/notification/${notificationId}`, {
            method: 'PATCH',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            }
        })
        if (!res.ok) return
        setNotifications(prev => prev.filter(n => n.id !== notificationId))
    }

    // click outside to close dropdown
    useEffect(() => {
        function handleClickOutside(e: MouseEvent) {
            if (dropdownRef.current && !dropdownRef.current.contains(e.target as Node)) {
                setOpen(false)
                setMessagesHovered(false)
            }
        }
        document.addEventListener('mousedown', handleClickOutside)
        return () => document.removeEventListener('mousedown', handleClickOutside)
    }, [])

    return (
        <div className="flex justify-between items-center px-8 py-4 border-b border-gray-200 bg-white">
            {/* Back Button */}
            {showBack ? (
                <button
                    onClick={() => router.back()}
                    className="flex items-center gap-1.5 text-sm text-gray-500 hover:text-gray-800 transition-colors"
                >
                    <ArrowLeft size={16} />
                    <span>Back</span>
                </button>
            ) : (
                <div />
            )}

            <div className="relative" ref={dropdownRef}>

                {/* User Icon */}
                <button
                    onClick={() => setOpen(!open)}
                    className="relative w-10 h-10 rounded-full bg-gray-300 flex items-center justify-center hover:bg-gray-400 transition-colors"
                >
                    <User size={20} className="text-gray-600" />
                    {notifications.length > 0 && (
                        <span className="absolute -top-1 -right-1 w-4 h-4 bg-red-500 text-white text-[10px] font-bold rounded-full flex items-center justify-center">
                            {notifications.length}
                        </span>
                    )}
                </button>


                {/* Dropdown */}
                {open && (
                    <div className="absolute right-0 top-12 w-52 bg-white border border-gray-200 rounded-xl shadow-lg z-50">

                        {/* Messages */}
                        <div
                            className="relative"
                            onMouseEnter={() => setMessagesHovered(true)}
                            onMouseLeave={() => setMessagesHovered(false)}
                        >
                            <button className="flex items-center justify-between w-full px-4 py-3 text-sm text-gray-700 hover:bg-gray-50 rounded-t-xl">
                                <div className="flex items-center gap-2">
                                    <Bell size={16} />
                                    <span>Messages</span>
                                    {notifications.length > 0 && (
                                        <span className="w-4 h-4 bg-red-500 text-white text-[10px] font-bold rounded-full flex items-center justify-center">
                                            {notifications.length}
                                        </span>
                                    )}
                                </div>
                                <ChevronRight size={14} className="text-gray-400" />
                            </button>

                            {/* Second-level notification list */}
                            {messagesHovered && (
                                <div className="absolute right-full top-0 mr-2 w-80 bg-white border border-gray-200 rounded-xl shadow-lg z-50">
                                    <div className="px-4 py-3 border-b border-gray-100">
                                        <p className="text-sm font-semibold text-gray-800">Notifications</p>
                                        {notifications.length > 0 && (
                                            <p className="text-xs text-gray-400 mt-0.5">{notifications.length} unread</p>
                                        )}
                                    </div>

                                    <div className="max-h-72 overflow-y-auto divide-y divide-gray-50">
                                        {notifications.length === 0 ? (
                                            <p className="px-4 py-6 text-sm text-center text-gray-400">
                                                No unread notifications
                                            </p>
                                        ) : (
                                            notifications.map(n => (
                                                <div
                                                    key={n.id}
                                                    onClick={() => markAsRead(n.id)}
                                                    className="flex items-start gap-3 px-4 py-3 hover:bg-gray-50 cursor-pointer transition-colors"
                                                >
                                                    <span className="mt-1.5 w-2 h-2 flex-shrink-0 rounded-full bg-red-500" />
                                                    <div className="min-w-0">
                                                        <p className="text-sm font-medium text-gray-800 leading-snug">
                                                            [{n.notificationType}]
                                                            {n.workerInfoDesc ? ` — ${n.workerInfoDesc}` : ''}
                                                        </p>
                                                        <p className="text-xs text-gray-400 mt-0.5">
                                                            {formatTime(n.notificationTime)}
                                                        </p>
                                                    </div>
                                                </div>
                                            ))
                                        )}
                                    </div>

                                    <div className="border-t border-gray-100 px-4 py-2.5">
                                        <Link
                                            href="/notification/all"
                                            onClick={() => setOpen(false)}
                                            className="text-xs font-medium text-blue-500 hover:text-blue-600 transition-colors"
                                        >
                                            View all notifications →
                                        </Link>
                                    </div>
                                </div>
                            )}
                        </div>

                        {/* Certification — Only shown on employer side */}
                        {role === 'employer' && (
                            <div className="border-t border-gray-100">
                                <button
                                    onClick={() => {
                                        router.push('/employer/certification')
                                        setOpen(false)
                                    }}
                                    className="flex items-center gap-2 w-full px-4 py-3 text-sm text-gray-700 hover:bg-gray-50"
                                >
                                    <ShieldCheck size={16} />
                                    <span>Certification</span>
                                </button>
                            </div>
                        )}

                        {/* Log Out */}
                        <div className="border-t border-gray-100">
                            <button
                                onClick={() => {
                                    logout()
                                    router.push('/')
                                    setOpen(false)
                                }}
                                className="flex items-center gap-2 w-full px-4 py-3 text-sm text-red-600 hover:bg-red-50 rounded-b-xl"
                            >
                                <LogOut size={16} />
                                <span>Log Out</span>
                            </button>
                        </div>
                    </div>
                )}
            </div>
        </div>
    )
}