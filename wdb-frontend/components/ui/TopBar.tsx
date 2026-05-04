'use client'

import { User } from 'lucide-react'

export default function TopBar() {
    return (
        <div className="flex justify-end items-center px-8 py-4 border-b border-gray-200 bg-white">
            <button
                onClick={() => console.log('go to settings')}
                className="w-10 h-10 rounded-full bg-gray-300 flex items-center justify-center hover:bg-gray-400 transition-colors"
            >
                <User size={20} className="text-gray-600" />
            </button>
        </div>
    )
}