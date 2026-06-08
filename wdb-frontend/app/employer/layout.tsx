import Sidebar from "@/component/sidebar/Sidebar";
import TopBar from "@/component/ui/TopBar";

export default function EmployerLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="flex h-screen overflow-hidden bg-gray-50">
      <Sidebar role="employer" />
      <div className="flex flex-1 flex-col">
        <TopBar role="employer" />
        <main className="flex-1 overflow-y-auto p-6">
          {children}
        </main>
      </div>
    </div>
  );
}



