import { getWorkerDashboard } from "@/lib/workerDashboardApi";
import WorkerDashboardView from "./WorkerDashboardView";

export default async function WorkerDashboardPage() {
  const workerId = "11111111-1111-1111-1111-111111111111";
  const data = await getWorkerDashboard(workerId);

  return <WorkerDashboardView data={data} />;
}
