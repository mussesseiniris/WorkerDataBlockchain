import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import CertificationPage from "../app/employer/certification/page";

// Mock next/navigation
jest.mock("next/navigation", () => ({
    useRouter: () => ({ push: jest.fn() })
}));

// Mock AuthContext
jest.mock("@/context/AuthContext", () => ({
    useAuth: () => ({
        token: "mock-token",
        role: "employer",
        isAuthReady: true
    })
}));

// Mock fetch
global.fetch = jest.fn();

const mockFetch = global.fetch as jest.Mock;

beforeEach(() => {
    mockFetch.mockClear();
});

describe("CertificationPage", () => {

    // Test 1: 显示 Not Submitted 状态
    it("renders Not Submitted status when no certification uploaded", async () => {
        mockFetch.mockResolvedValueOnce({
            ok: true,
            json: async () => ({ status: null, fileName: null, uploadedAt: null })
        });

        render(<CertificationPage />);

        await waitFor(() => {
            expect(screen.getByText("Not Submitted")).toBeInTheDocument();
        });
    });

    // Test 2: 显示 Pending 状态
    it("renders Pending Review status and hides upload section", async () => {
        mockFetch.mockResolvedValueOnce({
            ok: true,
            json: async () => ({
                status: "Pending",
                fileName: "cert.pdf",
                uploadedAt: "2026-06-04T10:00:00Z"
            })
        });

        render(<CertificationPage />);

        await waitFor(() => {
            expect(screen.getByText("Pending Review")).toBeInTheDocument();
            expect(screen.getByText(/cert.pdf/)).toBeInTheDocument();
            expect(screen.queryByText("Upload Document")).not.toBeInTheDocument()
            expect(screen.getByText(/Your document is under review/)).toBeInTheDocument();
        });
    });

    // Test 3: 显示 Approved 状态
    it("renders Approved status and hides upload section", async () => {
        mockFetch.mockResolvedValueOnce({
            ok: true,
            json: async () => ({
                status: "Approved",
                fileName: "cert.pdf",
                uploadedAt: "2026-06-04T10:00:00Z"
            })
        });

        render(<CertificationPage />);

        await waitFor(() => {
            expect(screen.getByText("Approved")).toBeInTheDocument();
            expect(screen.queryByText("Upload Document")).not.toBeInTheDocument();
            expect(screen.getByText(/Your certification has been approved/)).toBeInTheDocument();
        });
    });

    // Test 4: 显示 Rejected 状态，显示重新上传
    it("renders Rejected status and shows re-upload section", async () => {
        mockFetch.mockResolvedValueOnce({
            ok: true,
            json: async () => ({
                status: "Rejected",
                fileName: "old_cert.pdf",
                uploadedAt: "2026-06-04T10:00:00Z"
            })
        });

        render(<CertificationPage />);

        await waitFor(() => {
            expect(screen.getByText("Rejected")).toBeInTheDocument();
            expect(screen.getByText("Re-upload Document")).toBeInTheDocument();
        });
    });

    // Test 5: 上传按钮在没有选择文件时是 disabled
    it("disables upload button when no file selected", async () => {
        mockFetch.mockResolvedValueOnce({
            ok: true,
            json: async () => ({ status: null, fileName: null, uploadedAt: null })
        });

        render(<CertificationPage />);

        await waitFor(() => {
            const uploadButton = screen.getByText("Upload");
            expect(uploadButton).toBeDisabled();
        });
    });

    // Test 6: 选择文件后上传按钮变成可用
    it("enables upload button when file is selected", async () => {
        mockFetch.mockResolvedValueOnce({
            ok: true,
            json: async () => ({ status: null, fileName: null, uploadedAt: null })
        });

        render(<CertificationPage />);

        await waitFor(() => {
            expect(screen.getByText("Upload")).toBeInTheDocument();
        });

        const fileInput = screen.getByRole("button", { name: /upload/i });
        const input = document.querySelector('input[type="file"]') as HTMLInputElement;

        const file = new File(["content"], "cert.pdf", { type: "application/pdf" });
        fireEvent.change(input, { target: { files: [file] } });

        await waitFor(() => {
            expect(screen.getByText("Upload")).not.toBeDisabled();
        });
    });

    // Test 7: 上传成功后显示成功消息
    it("shows success message after successful upload", async () => {
        // 第一次调用：加载状态
        mockFetch.mockResolvedValueOnce({
            ok: true,
            json: async () => ({ status: null, fileName: null, uploadedAt: null })
        });

        // 第二次调用：上传成功
        mockFetch.mockResolvedValueOnce({
            ok: true,
            json: async () => ({
                status: "Pending",
                fileName: "cert.pdf",
                uploadedAt: "2026-06-04T10:00:00Z"
            })
        });

        // 第三次调用：刷新状态
        mockFetch.mockResolvedValueOnce({
            ok: true,
            json: async () => ({
                status: "Pending",
                fileName: "cert.pdf",
                uploadedAt: "2026-06-04T10:00:00Z"
            })
        });

        render(<CertificationPage />);

        await waitFor(() => {
            expect(screen.getByText("Upload")).toBeInTheDocument();
        });

        const input = document.querySelector('input[type="file"]') as HTMLInputElement;
        const file = new File(["content"], "cert.pdf", { type: "application/pdf" });
        fireEvent.change(input, { target: { files: [file] } });

        fireEvent.click(screen.getByText("Upload"));

        await waitFor(() => {
            expect(screen.getByText("Certification document uploaded successfully.")).toBeInTheDocument();
        });
    });

    // Test 8: 上传失败显示错误消息
    it("shows error message when upload fails", async () => {
        mockFetch.mockResolvedValueOnce({
            ok: true,
            json: async () => ({ status: null, fileName: null, uploadedAt: null })
        });

        mockFetch.mockResolvedValueOnce({
            ok: false,
            json: async () => ({ message: "Upload failed" })
        });

        render(<CertificationPage />);

        await waitFor(() => {
            expect(screen.getByText("Upload")).toBeInTheDocument();
        });

        const input = document.querySelector('input[type="file"]') as HTMLInputElement;
        const file = new File(["content"], "cert.pdf", { type: "application/pdf" });
        fireEvent.change(input, { target: { files: [file] } });

        fireEvent.click(screen.getByText("Upload"));

        await waitFor(() => {
            expect(screen.getByText("Upload failed")).toBeInTheDocument();
        });
    });
});