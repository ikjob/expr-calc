import { CheckCircle, TimesCircle, Stop } from '@ricons/fa'

export default function CalculationsPage() {  
  return (
    <div className="mt-2 mx-4">
      <div className="join flex my-4">
        <input type="text" className="input input-bordered input-accent join-item flex-auto" placeholder="Expression" />
        <button className="btn btn-accent join-item rounded-r-full flex-none">Submit</button>
      </div>

      <details className="collapse collapse-arrow bg-base-200 border-base-300 rounded-md my-4 flex">
        <summary className="collapse-title">
          <span className="align-middle">Filters:</span>
          <div className="badge badge-lg mx-4">User: abc</div>
        </summary>
        <div className="collapse-content">
        <label className="label cursor-pointer">
          <input type="checkbox" className="checkbox" />
          <span className="label-text">Current user</span>
        </label>
        </div>
      </details>

      
      <div className="overflow-x-auto my-4">
        <table className="table border-1 border-base-200">
          {/* head */}
          <thead className="text-info text-accent text-md">
            <tr className="flex">
              <th className="flex-none w-8">#</th>
              <th className="flex-8">Expression</th>
              <th className="flex-1">Result</th>
              <th className="flex-none w-32">Submitted By</th>
              <th className="flex-none w-32">Submitted At</th>
              <th className="flex-none w-24"></th>
            </tr>
          </thead>
          <tbody>
            {/* row 1 */}
            <tr className="flex">
              <td className="flex-none w-8">1</td>
              <td className="flex-8">1 + 2</td>
              <td className="flex-1">3</td>
              <td className="flex-none w-32">User1</td>
              <td className="flex-none w-32">20.02.2025 13:00:01</td>
              <td className="flex-none w-24"><CheckCircle className="text-success w-4" /></td>
            </tr>
            {/* row 2 */}
            <tr className="flex">
              <td className="flex-none w-8">2</td>
              <td className="flex-8">3 * 6</td>
              <td className="flex-1">18</td>
              <td className="flex-none w-32">User2</td>
              <td className="flex-none w-32">20.02.2025 13:00:01</td>
              <td className="flex-none w-24"><TimesCircle className="text-error w-4" /></td>
            </tr>
            {/* row 3 */}
            <tr className="flex">
              <td className="flex-none w-8">3</td>
              <td className="flex-8">8 / 2</td>
              <td className="flex-1">4</td>
              <td className="flex-none w-32">User3</td>
              <td className="flex-none w-32">20.02.2025 13:00:01</td>
              <td className="flex-none w-24"><div className="join"><span className="loading loading-spinner join-item text-primary w-4"/><Stop className="joinItem w-4 ml-2" /></div></td>
            </tr>
          </tbody>
        </table>
      </div>


      <div className="join">
        <input
          className="join-item btn btn-square"
          type="radio"
          name="options"
          aria-label="1"
          defaultChecked />
        <input className="join-item btn btn-square" type="radio" name="options" aria-label="2" />
        <input className="join-item btn btn-square" type="radio" name="options" aria-label="3" />
        <input className="join-item btn btn-square btn-disabled text-normal" type="radio" name="options" aria-label="..." />
        <input className="join-item btn btn-square" type="radio" name="options" aria-label="5" />
        <input className="join-item btn btn-square" type="radio" name="options" aria-label="6" />
      </div>
    </div>
  )
}