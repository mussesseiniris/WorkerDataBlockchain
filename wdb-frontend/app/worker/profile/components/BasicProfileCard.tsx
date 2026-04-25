import { useEffect, useState } from "react"
import { WorkerInfoItem } from "../type"
import TextInput from "@/components/ui/TextInput"
import Dropdown from "@/components/ui/Dropdown"
import { DisplayField } from '@/components/ui/DisplayField'

// define what type of data should be inject
interface BasicProfileCardProps {
    data: WorkerInfoItem[]
    onSave: (desc: string, value: string) => Promise<void>
}

// main method : define the variable of this component and read danamic variable's value then save final value
// return{} define the ui logic 
export default function BasicProfileCard({ data, onSave }: BasicProfileCardProps) {
    // define the variable in order to allow avriavle could be danamic
    const [isEditing, setIsEdit] = useState(false)
    const [editFirstName, setEditFirstName] = useState('')
    const [editLastName, setEditLastName] = useState('')
    const [editPhone, setEditPhone] = useState('')
    const [editEmail, setEditEmail] = useState('')
    const [editCountry, setEditCountry] = useState('')
    const [editCity, setEditCity] = useState('')
    const [editStreet, setEditStreet] = useState('')
    const [editPostcode, setEditPostcode] = useState('')
    const [editGendar, setEditGendar] = useState('')

    const genderOptions = [
        { label: 'Male', value: 'male' },
        { label: 'Female', value: 'female' },
        { label: 'Non-binary', value: 'non-binary' },
        { label: 'Prefer not to say', value: 'prefer_not_to_say' },
    ]


    // read the danamic variable's value
    useEffect(() => {
        setEditFirstName(data.find(item => item.desc == 'firstname')?.value ?? '')
        setEditLastName(data.find(item => item.desc == 'lastname')?.value ?? '')
        setEditPhone(data.find(item => item.desc == 'phonenumber')?.value ?? '')
        setEditEmail(data.find(item => item.desc == 'email')?.value ?? '')
        setEditCountry(data.find(item => item.desc == 'country')?.value ?? '')
        setEditCity(data.find(item => item.desc == 'city')?.value ?? '')
        setEditStreet(data.find(item => item.desc == 'street')?.value ?? '')
        setEditPostcode(data.find(item => item.desc == 'post code')?.value ?? '')
        setEditGendar(data.find(item => item.desc == 'genda')?.value ?? '')
    }, [data])
    //  save final value
    const handleDone = async () => {

        await onSave('firstname', editFirstName)
        await onSave('lastname', editLastName)
        await onSave('phonenumber', editPhone)
        await onSave('email', editEmail)
        await onSave('country', editCountry)
        await onSave('city', editCity)
        await onSave('street', editStreet)
        await onSave('post code', editPostcode)
        await onSave('genda', editGendar)

        setIsEdit(false)
    }
    // define th ui logic
    return (
        <div className="bg-white rounded-xl border border-gray-200 p-6">
            <div className="flex justify-between items-start mb-6">
                <h2 className="text-lg font-semibold text-gray-800"> Basic Information</h2>
                <button
                    onClick={isEditing ? handleDone : () => setIsEdit(true)}
                    className="flex item-center gap text-sm text-blue-600 hover:text-blue-800 ">
                    {isEditing ? 'Done' : 'Edit'}
                </button>
            </div>
            {isEditing ? (
                <div className="flex flex-col gap-4">
                    <div className="grid grid-cols-2 gap-4">
                        <TextInput label={"First Name"} value={editFirstName}
                            onChange={(v) => setEditFirstName(v)}
                        />

                        <TextInput label={"Last Name"} value={editLastName}
                            onChange={(v) => setEditLastName(v)}
                        />

                        <TextInput label={"Phone"} value={editPhone}
                            onChange={(v) => setEditPhone(v)}
                        />
                        <TextInput label={"Email"} value={editEmail}
                            onChange={(v) => setEditEmail(v)}
                        />
                    </div>

                    <p className="text-sm font-medium text-gray-500 mt-4">Address</p>
                    <div className="grid grid-cols-2 gap-4 pl-4 border-l-2 border-gray-200">
                        <TextInput label={"Country"} value={editCountry} onChange={(v) => setEditCountry(v)} />
                        <TextInput label={"City"} value={editCity} onChange={(v) => setEditCity(v)} />
                        <TextInput label={"Street"} value={editStreet} onChange={(v) => setEditStreet(v)} />
                        <TextInput label={"Post Code"} value={editPostcode} onChange={(v) => setEditPostcode(v)} />
                    </div>

                    <Dropdown label={"Gendar"} value={editGendar} onChange={(v) => setEditGendar(v)} options={genderOptions} />
                </div>
            ) : (

                <div className="flex flex-col gap-4">
                    <div className="grid grid-cols-2 gap-4">
                        <DisplayField label="First Name" value={editFirstName} />
                        <DisplayField label="Last Name" value={editLastName} />
                    </div>
                    <div className="grid grid-cols-2 gap-4">
                        <DisplayField label="Phone" value={editPhone} />
                        <DisplayField label="Email" value={editEmail} />
                    </div>
                    <DisplayField label="Country" value={editCountry} />
                    <DisplayField label="City" value={editCity} />
                    <DisplayField label="Street" value={editStreet} />
                    <DisplayField label="Post Code" value={editPostcode} />
                    <DisplayField label="Gender" value={editGendar} />
                </div>

            )}
        </div>
    )
}

