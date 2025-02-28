import { CheckCircle, TimesCircle, Stop, ExclamationCircle } from '@ricons/fa'

export default function CalculationsTable() {  
  return (
      <div className="overflow-x-auto my-4 hover:table-hover">
        <table className="table border-1 border-base-200 table-fixed">
          <thead className="info text-accent text-md bg-sky-100/50">
            <tr className="">
              <th className="w-12"></th>
              <th className="min-w-40 w-full text-left">Expression</th>
              <th className="w-40">Result</th>
              <th className="w-40">Submitted By</th>
              <th className="w-40">Submitted At</th>
              <th className="w-24"></th>
            </tr>
          </thead>
          <tbody>
            <tr className="hover:bg-gray-200">
              <td className="w-12"><CheckCircle className="text-success w-4" /></td>
              <td className="min-w-40 w-full text-left">1 + 2</td>
              <td className="w-40">3</td>
              <td className="w-40">User1</td>
              <td className="w-40">20.02.2025 13:00:01</td>
              <td className="w-24"></td>
            </tr>
            <tr className="hover:bg-gray-200 bg-gray-200/25">
              <td className="w-12"><TimesCircle className="text-error w-4" /></td>
              <td className="min-w-40 w-full text-left">3 * 6 +</td>
              <td className="w-40"><a>Syntax error</a><CalculationErrorInfoMarker /></td>
              <td className="w-40">User2</td>
              <td className="w-40">20.02.2025 13:00:01</td>
              <td className="w-24"></td>
            </tr>
            <tr className="hover:bg-gray-200">
              <td className="w-12"><span className="loading loading-spinner join-item text-primary w-4"/></td>
              <td className="min-w-40 w-full text-left">8 / 2</td>
              <td className="w-40">-</td>
              <td className="w-40">User3</td>
              <td className="w-40">20.02.2025 13:00:01</td>
              <td className="w-24"><button className="btn btn-xs"><Stop className="size-[1em]" />Stop</button></td>
            </tr>
          </tbody>
        </table>
      </div>
  )
}


function CalculationErrorInfoMarker() {  
    return (
        <div className="dropdown dropdown-end">
        <div tabIndex={0} role="button" className="btn btn-circle btn-ghost btn-xs text-error">
            <ExclamationCircle className="h-4 w-4 stroke-current" />
        </div>
        <div
            tabIndex={0}
            className="card card-sm dropdown-content bg-base-100 rounded-box z-1 w-64 shadow-sm">
            <div tabIndex={0} className="card-body">
            <h2 className="card-title">Expression unbalanced</h2>
            <p>3 * 6 <a className="text-red-500">+</a></p>
            </div>
        </div>
        </div>
    )
}