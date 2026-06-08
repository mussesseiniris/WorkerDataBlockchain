import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { renderWithAuth } from "../utils/renderWithAuth";
import ProfilePage from "@/app/worker/profile/page";
import LoginPage from "@/app/login/page"
import EmployerDashboardPage from "@/app/employer/dashboard/page"
import EmployerRequestsPage from "@/app/employer/requests/RequestModal"

jest.mock('next/navigation', () => ({
    useRouter: () => ({ push: jest.fn() })
}));

describe("AuthContext integration test", () => {
    it("should render ProfilePage with auth context", () => {
        renderWithAuth(<ProfilePage />, {
            token: 'fake-token',
            role: 'worker',
            userId: '123',
            userName: 'TestUser'
        });
        expect(screen.getByText("TestUser")).toBeInTheDocument();
    });

    it("should render LoginPage with auth context", () => {
        renderWithAuth(<LoginPage />, {
            token: 'fake-token',
            role: 'worker',
            userId: '123',
            userName: 'TestUser'
        });
        expect(screen.getByText("Login")).toBeInTheDocument();
    });

    it("should render EmployerDashboardPage with auth context", () => {
        renderWithAuth(<EmployerDashboardPage />, {
            token: 'fake-token',
            role: 'employer',
            userId: '123',
            userName: 'TestUser'
        });
        expect(screen.getByText("Employer Dashboard")).toBeInTheDocument();
    });

    it("should render EmployerRequestsPage with auth context", () => {
        renderWithAuth(<EmployerRequestsPage onClose={function (): void {
            throw new Error("Function not implemented.");
        }} />, {
            token: 'fake-token',
            role: 'employer',
            userId: '123',
            userName: 'TestUser'
        });
        expect(screen.getByText("Create new request")).toBeInTheDocument();
    });
});

