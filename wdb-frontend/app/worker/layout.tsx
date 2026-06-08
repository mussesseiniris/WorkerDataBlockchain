import Sidebar from '@/component/sidebar/Sidebar';
import TopBar from '@/component/ui/TopBar';
import type { ReactNode } from 'react';

export default function WorkerLayout({
  children,
}: {
  children: ReactNode;
}) {
  return (
    <div className="flex h-screen overflow-hidden bg-gray-50">
      <Sidebar role="worker" />

      <div className="flex flex-1 flex-col overflow-hidden">
        <TopBar role="worker" />

        <main className="flex-1 overflow-y-auto p-6">
          {children}
        </main>
      </div>
    </div>
  );
}
