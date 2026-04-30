import { render, screen } from "@testing-library/react";
import WorkerDashboardView from "./WorkerDashboardView";
import type { WorkerDashboardResponse } from "@/lib/workerDashboardApi";

const mockData: WorkerDashboardResponse = {
  worker: {
    id: "11111111-1111-1111-1111-111111111111",
    name: "user",
    email: "user@example.com",
    verified: true,
  },
  latestRequests: [
    {
      requestId: "55555555-5555-5555-5555-555555555551",
      employerId: "22222222-2222-2222-2222-222222222222",
      employerName: "First Step Solutions",
      createdAt: "2026-04-15T10:00:00Z",
      reason: "Site onboarding",
    },
  ],
  blockchainRecords: [],
};

describe("WorkerDashboardView", () => {
  it("renders worker basic information", () => {
    render(<WorkerDashboardView data={mockData} />);

    expect(screen.getByText("Worker Dashboard")).toBeInTheDocument();
    expect(screen.getByText("user")).toBeInTheDocument();
    expect(screen.getByText("user@example.com")).toBeInTheDocument();
    expect(screen.getByText("Verified")).toBeInTheDocument();
  });

  it("renders latest requests", () => {
    render(<WorkerDashboardView data={mockData} />);

    expect(screen.getByText("Latest Requests")).toBeInTheDocument();
    expect(screen.getByText("First Step Solutions")).toBeInTheDocument();
    expect(screen.getByText("Site onboarding")).toBeInTheDocument();
  });

  it("renders empty blockchain placeholder", () => {
    render(<WorkerDashboardView data={mockData} />);

    expect(
      screen.getByText("No blockchain records available yet.")
    ).toBeInTheDocument();
  });
});
