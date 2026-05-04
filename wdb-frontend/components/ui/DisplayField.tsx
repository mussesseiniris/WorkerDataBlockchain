


export const DisplayField = ({ label, value }: { label: string; value: string }) => (
    <div className="flex flex-col gap-2">
        <p className="text-sm font-medium text-gray-700">{label}</p>
        <p className="text-lg text-gray-900">{value}</p>
    </div>
)