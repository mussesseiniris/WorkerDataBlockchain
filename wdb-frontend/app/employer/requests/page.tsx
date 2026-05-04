// Employer requests: submit new data access requests to worker's
'use client';
import { FetchApi } from '@/lib/api';
// import { Worker } from 'cluster';
import { use, useState } from 'react';

export default function Page() {
  // Type definitions
  type WorkerInfo = {
    id: string;
    desc: string;
    value: string;
  };

  type Worker = {
    id: string;
    name: string;
    email: string;
  };

  //from state
  const [email, setEmail] = useState('');
  const [reason, setReason] = useState('');
  // Worker data
  const [worker, setWorker] = useState<Worker | null>(null);
  const [workerInfos, setWorkerInfos] = useState<WorkerInfo[]>([]);

  // UI state
  const [isLoading, setIsLoading] = useState(false);
  const [errorMsg, setErrorMsg] = useState('');
  const [sentMsg, setSentMsg] = useState('');
  const [findWorker, setFindWorker] = useState(false);
  const [isOpen, setIsOpen] = useState(true);

  // Selected worker info items (stored as set of GUIDs)
  const [isSelected, setSelected] = useState<Set<string>>(new Set());

  // Search for a worker by email and fetch their available info
  async function handlesearch(email: string) {
    if (!email) {
      alert('Please enter an email');
      return;
    }
    setIsLoading(true);
    setErrorMsg('');
    try {
      var worker = await FetchApi(
        `/api/Employer/GetWorkerByEmail?email=${email}`,
      );

      if (!worker) {
        alert('There is no worker information for this email');
        return;
      }
      setWorker(worker);

      var workerInfos = await FetchApi(`/api/Employer?email=${email}`);
      setWorkerInfos(workerInfos);
      setFindWorker(true);
    } catch (error) {
      setErrorMsg('Worker not found');
    } finally {
      setIsLoading(false);
    }
  }
  // Toggle selection of a worker info item by its GUID(GUID is string)
  function toggle(id: string) {
    setSelected((prev) => {
      const next = new Set(prev);
      next.has(id) ? next.delete(id) : next.add(id);
      return next;
    });
  }
  // Submit access request for selected worker info items
  async function handleRequest() {
    if (!reason) {
      alert('please fill in the reason');
      return;
    }
    if (isSelected.size == 0) {
      alert('please select at least one item');
      return;
    }

    setSentMsg('Sending...');
    const token = localStorage.getItem('accessToken');
    if (!token) {
      setSentMsg('Please log in first');
      return;
    }
    try {
      var result = await FetchApi('/api/Employer/AccessRequests', {
        method: 'POST',
        headers: {
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({
          email: email,
          infoDesc: Array.from(isSelected),
          reason: reason,
        }),
      });
      setSentMsg('Request has been sent');
    } catch (error) {
      setSentMsg('Something went wrong, please try again');
    }
  }
  return (
    <>
      {/* create new access request */}
      {isOpen && (
        <div className="relative max-w-lg mx-auto mt-10 p-6 border border-gray-600 rounded-xl shadow-md">
          {/* cloese button */}
          <button
            className="absolute top-5 right-8 text-gray-600 text-xl"
            onClick={() => setIsOpen(false)}
          >
            x
          </button>
          <div className="flex items-center justify-center flex-col gap-2">
            <p className="text-left w-full text-gray-600">Create new request</p>

            {/* Step1: Search for worker by email  */}
            {!findWorker && (
              <div className=" w-full gap-2">
                <div className="relative border border-gray-300 rounded-xl px-4 pt-5 pb-2 w-full">
                  <label className="absolute top-2 left-4 text-xs text-gray-400">
                    Email
                  </label>

                  <input
                    type="text"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    className="w-full outline-none text-gray-800"
                    placeholder="Workeremail@gmail.com"
                  />
                </div>
                <button
                  onClick={() => handlesearch(email)}
                  disabled={isLoading}
                  className=" bg-[#49454F] px-6 py-2 rounded-lg text-white w-full my-2"
                >
                  {isLoading ? 'Searching' : 'Search'}
                </button>
                {errorMsg && (
                  <p className="text-[#49454F]-500 text-center">{errorMsg}</p>
                )}
              </div>
            )}
            {/* Step 2: Select info items and submit request */}

            {findWorker && (
              <div className="w-full flex flex-col ">
                <div className="text-gray-600 bg-gray-100 w-full rounded-lg my-4 p-4">
                  <p>Email: {worker?.email}</p>
                  <p>Name: {worker?.name}</p>
                </div>

                {workerInfos.length === 0 ? (
                  <>
                    <p className='mb-4'>This worker has no info items</p>
                    <button className='px-6 py-2 rounded-lg bg-[#49454F] text-white w-full' onClick={() => setFindWorker(false)}>Back</button>
                  </>
                ) : (
                  <>
                    <p className="text-gray-600">
                      Please choose the info you want to request
                    </p>
                    <div className="flex flex-col gap-2 ">
                      {workerInfos.map((w) => (
                        <div
                          key={w.id}
                          className="flex items-center justify-center border rounded-lg border-gray-300 w-full"
                        >
                          <input
                            type="checkbox"
                            className="border rounded-lg gap-2 m-2 accent-[#49454F]"
                            checked={isSelected.has(w.id)}
                            onChange={() => toggle(w.id)}
                          />
                          <p className="flex-1"> {w.desc}</p>
                        </div>
                      ))}
                    </div>

                    {/* Reason input and submit */}
                    <div className="flex flex-col items-center gap-4 rounded-lg my-4 w-full">
                      <div className="relative border border-gray-300 rounded-xl px-4 pt-5 pb-2 w-full">
                        <label className="absolute top-2 left-4 text-xs text-gray-400 ">
                          Reason
                        </label>
                        <input
                          type="text"
                          className="w-full outline-none text-gray-800"
                          value={reason}
                          onChange={(e) => setReason(e.target.value)}
                        />
                      </div>
                      {/* Click the button to submit a request */}
                      <button
                        onClick={() => handleRequest()}
                        className="px-6 py-2 rounded-lg bg-[#49454F] text-white w-full"
                      >
                        Submit
                      </button>
                      <p className="flex items-center">{sentMsg}</p>
                    </div>
                  </>
                )}
              </div>
            )}
          </div>
        </div>
      )}
      {/* find worker */}
    </>
  );
  {
    /* return总 */
  }
}
