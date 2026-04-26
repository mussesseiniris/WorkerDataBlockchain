// Employer requests: submit new data access requests to worker's
'use client';
import { FetchApi } from '@/lib/api';
// import { Worker } from 'cluster';
import { useState } from 'react';

export default function Page() {
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
  const [email, setEmail] = useState('');
  const [workerInfos, setWorkerInfos] = useState<WorkerInfo[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [errorMsg, setErrorMsg] = useState('');
  const [worker, setWorker] = useState<Worker | null>(null);

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
    } catch (error) {
      setErrorMsg('Worker not found');
    } finally {
      setIsLoading(false);
    }
  }
  return (
    <div>
      <div className="flex items-center justify-center flex-col gap-2">
        <p>Email:</p>
        <input
          type="text"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          className="border border-gray-400 p-1"
        />
        <button
          onClick={() => handlesearch(email)}
          disabled={isLoading}
          className="bg-blue-400 px-6 py-2 rounded-lg"
        >
          {isLoading ? 'Searching' : 'Search'}
        </button>
        {worker && <div>
            <p>Employer Email: {worker.email}</p>
            <p>Employer Name: {worker.name}</p></div>}
        {errorMsg && <p className="text-red-500">{errorMsg}</p>}
      </div>
      {workerInfos.map((w) => (
        <div key={w.id} className='flex flex-cool items-center justify-center'>
        <input type="checkbox" className='border' />
          <p>  {w.desc}</p>
        </div>
      ))}
    </div>
  );
}
