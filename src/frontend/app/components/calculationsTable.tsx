import { CheckCircle, TimesCircle, ExclamationCircle, Stop } from '@ricons/fa'
import CancelledIcon from './icons/cancelledIcon';
import PendingIcon from './icons/pendingIcon';
import { Calculation } from '../models/calculation';
import { Uuid } from '../common';

interface CalculationsTableProps {
    rows: Calculation[];
    onStop: (id: Uuid) => void;
}

const statusIconPerState = {
    "Pending": <PendingIcon className="text-base-content w-4" />,
    "InProgress": <span className="loading loading-spinner join-item text-primary w-4"/>,
    "Success": <CheckCircle className="text-success w-4" />,
    "Failed": <TimesCircle className="text-error w-4" />,
    "Cancelled": <CancelledIcon className="text-base-content w-4" />,
}

export default function CalculationsTable({ rows, onStop }: CalculationsTableProps) {  
    return (
        <table className="table border-1 border-base-200 table-fixed hover:table-hover">
            <thead className="info text-accent text-md bg-info/10">
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
                { rows.map((calculation, index) => (
                    <tr key={calculation.id} className={`hover:bg-base-200 ${index % 2 == 1 ? "bg-base-200/25" : ""}`}>
                        <td className="w-12">{statusIconPerState[calculation.status.state] ?? <></>}</td>
                        <td className="min-w-40 w-full text-left">{calculation.expression}</td>
                        <td className="w-40">3 <CalculationErrorInfoMarker /></td>
                        <td className="w-40">{calculation.createdBy}</td>
                        <td className="w-40">{calculation.createdAt.toLocaleString()}</td>
                        <td className="w-24"> {
                            (calculation.status.state == "Pending" || calculation.status.state == "InProgress") ?
                                <button className="btn btn-xs" onClick={() => onStop(calculation.id)}><Stop className="size-[1em]" />Stop</button> :
                                <></>
                        }</td>
                    </tr>
                ))}
            </tbody>
        </table>
    )


    /*
  return (
    <div className="overflow-x-auto my-4 hover:table-hover">
        <table className="table border-1 border-base-200 table-fixed">
            <thead className="info text-accent text-md bg-info/10">
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
                <tr className="hover:bg-base-200">
                    <td className="w-12"><CheckCircle className="text-success w-4" /></td>
                    <td className="min-w-40 w-full text-left">1 + 2</td>
                    <td className="w-40">3</td>
                    <td className="w-40">User1</td>
                    <td className="w-40">20.02.2025 13:00:01</td>
                    <td className="w-24"></td>
                </tr>
                <tr className="hover:bg-base-200 bg-base-200/25">
                    <td className="w-12"><TimesCircle className="text-error w-4" /></td>
                    <td className="min-w-40 w-full text-left">3 * 6 +</td>
                    <td className="w-40"><a>Syntax error</a><CalculationErrorInfoMarker /></td>
                    <td className="w-40">User2</td>
                    <td className="w-40">20.02.2025 13:00:01</td>
                    <td className="w-24"></td>
                </tr>
                <tr className="hover:bg-base-200">
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
  ) */
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