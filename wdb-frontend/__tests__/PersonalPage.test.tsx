import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import BasicProfileCard from "../app/worker/profile/components/BasicProfileCard";
import type { WorkerInfoItem } from "../app/worker/profile/type";

// prepare mock data
const mockData: WorkerInfoItem[] = [
    { workerId: "11111111-1111-1111-1111-111111111111", desc: "firstname", value: "John" },
    { workerId: "11111111-1111-1111-1111-111111111111", desc: "lastname", value: "Doe" },
    { workerId: "11111111-1111-1111-1111-111111111111", desc: "phonenumber", value: "021111111" },
    { workerId: "11111111-1111-1111-1111-111111111111", desc: "email", value: "john@example.com" },
    { workerId: "11111111-1111-1111-1111-111111111111", desc: "country", value: "New Zealand" },
    { workerId: "11111111-1111-1111-1111-111111111111", desc: "city", value: "Wellington" },
    { workerId: "11111111-1111-1111-1111-111111111111", desc: "street", value: "123 Main St" },
    { workerId: "11111111-1111-1111-1111-111111111111", desc: "postcode", value: "6011" },
    { workerId: "11111111-1111-1111-1111-111111111111", desc: "gender", value: "male" },
];

// prepare mock onSave function
const mockOnSave = jest.fn().mockResolvedValue(undefined);

// reset mock before each test
beforeEach(() => {
    mockOnSave.mockClear();
});

describe("BasicProfileCard", () => {

    // Test 1: check default display state
    it("renders basic information in display mode by default", () => {
        render(<BasicProfileCard data={mockData} onSave={mockOnSave} />);

        // check title and edit button exist
        expect(screen.getByText("Basic Information")).toBeInTheDocument();
        expect(screen.getByText("Edit")).toBeInTheDocument();

        // check data is displayed correctly
        expect(screen.getByText("John")).toBeInTheDocument();
        expect(screen.getByText("Doe")).toBeInTheDocument();
        expect(screen.getByText("021111111")).toBeInTheDocument();
        expect(screen.getByText("john@example.com")).toBeInTheDocument();
        expect(screen.getByText("New Zealand")).toBeInTheDocument();
        expect(screen.getByText("Wellington")).toBeInTheDocument();
    });

    // Test 2: check edit mode after clicking Edit button
    it("switches to edit mode when Edit button is clicked", () => {
        render(<BasicProfileCard data={mockData} onSave={mockOnSave} />);

        // click Edit button
        fireEvent.click(screen.getByText("Edit"));

        // Edit button should change to Done
        expect(screen.getByText("Done")).toBeInTheDocument();
        expect(screen.queryByText("Edit")).not.toBeInTheDocument();

        // input fields should appear with correct values
        expect(screen.getByDisplayValue("John")).toBeInTheDocument();
        expect(screen.getByDisplayValue("Doe")).toBeInTheDocument();
        expect(screen.getByDisplayValue("021111111")).toBeInTheDocument();
        expect(screen.getByDisplayValue("john@example.com")).toBeInTheDocument();
    });

    // Test 3: check onSave is called with correct params when Done is clicked
    it("calls onSave with correct values when Done is clicked", async () => {
        render(<BasicProfileCard data={mockData} onSave={mockOnSave} />);

        // switch to edit mode
        fireEvent.click(screen.getByText("Edit"));

        // click Done button
        fireEvent.click(screen.getByText("Done"));

        // wait for all async onSave calls to finish
        await waitFor(() => {
            expect(mockOnSave).toHaveBeenCalledWith("firstname", "John");
            expect(mockOnSave).toHaveBeenCalledWith("lastname", "Doe");
            expect(mockOnSave).toHaveBeenCalledWith("phonenumber", "021111111");
            expect(mockOnSave).toHaveBeenCalledWith("email", "john@example.com");
            expect(mockOnSave).toHaveBeenCalledWith("country", "New Zealand");
            expect(mockOnSave).toHaveBeenCalledWith("city", "Wellington");
            expect(mockOnSave).toHaveBeenCalledWith("street", "123 Main St");
            expect(mockOnSave).toHaveBeenCalledWith("postcode", "6011");
            expect(mockOnSave).toHaveBeenCalledWith("gender", "male");
        });

        // should return to display mode after saving
        expect(screen.getByText("Edit")).toBeInTheDocument();
        expect(screen.queryByText("Done")).not.toBeInTheDocument();
    });

    // Test 4: check input value can be changed
    it("updates input value when user types", () => {
        render(<BasicProfileCard data={mockData} onSave={mockOnSave} />);

        // switch to edit mode
        fireEvent.click(screen.getByText("Edit"));

        // find first name input and change value
        const firstNameInput = screen.getByDisplayValue("John");
        fireEvent.change(firstNameInput, { target: { value: "Jane" } });

        // check value has been updated
        expect(screen.getByDisplayValue("Jane")).toBeInTheDocument();
    });

    // Test 5: check empty data
    it("renders empty fields when no data is provided", () => {
        render(<BasicProfileCard data={[]} onSave={mockOnSave} />);

        // should still show the card title
        expect(screen.getByText("Basic Information")).toBeInTheDocument();

        // switch to edit mode
        fireEvent.click(screen.getByText("Edit"));

        // input fields should be empty
        const inputs = screen.getAllByRole("textbox");
        inputs.forEach(input => {
            expect(input).toHaveValue("");
        });
    });
});