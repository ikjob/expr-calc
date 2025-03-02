import CalculationsTable from '../components/calculationsTable'
import Pagination from '../components/pagination'
import { Calculation, createCancelledCalculationStatus, createFailedCalculationStatus, createInProgressCalculationStatus, createPedningCalculationStatus, createSuccessCalculationStatus } from '../models/calculation'

export default function CalculationsPage() {  
    const testRows : Calculation[] = [
        { id: "11", expression: "1+1", createdAt: new Date(), createdBy: "user", updatedAt: new Date(), status: createPedningCalculationStatus() },
        { id: "12", expression: "2+2", createdAt: new Date(), createdBy: "user", updatedAt: new Date(), status: createInProgressCalculationStatus() },
        { id: "13", expression: "1+3", createdAt: new Date(), createdBy: "user", updatedAt: new Date(), status: createSuccessCalculationStatus(100) },
        { id: "14", expression: "1+4", createdAt: new Date(), createdBy: "user", updatedAt: new Date(), status: createFailedCalculationStatus('ArithmeticError', { errorCode: 'Some code', offset: 1, length: 1}) },
        { id: "15", expression: "1+5", createdAt: new Date(), createdBy: "user", updatedAt: new Date(), status: createCancelledCalculationStatus("Admin") },
        { id: "16", expression: "1+6", createdAt: new Date(), createdBy: "user", updatedAt: new Date(), status: createInProgressCalculationStatus() },
        { id: "17", expression: "1+7", createdAt: new Date(), createdBy: "user", updatedAt: new Date(), status: createInProgressCalculationStatus() },
    ]

    return (
        <div className="mt-2 mx-4">
            <div className="join flex my-4">
                <input type="text" className="input input-bordered input-accent join-item flex-auto" placeholder="Expression" />
                <button className="btn btn-accent join-item rounded-r-full flex-none">Submit</button>
            </div>

            <CalculationsFilter />
          
            <CalculationsTable rows={testRows} onStop={(_id) => {}} />

            <Pagination />
        </div>
    )
}

function CalculationsFilter() {
    return (
        <details className="collapse collapse-arrow bg-base-200 border-base-300 rounded-md my-4 flex">
            <summary className="collapse-title">
                <span className="align-middle">Filters:</span>
                <div className="badge badge-lg mx-4">User: User1</div>
            </summary>
            <div className="collapse-content">
                <label className="label cursor-pointer">
                    <input type="checkbox" className="checkbox" defaultChecked={true} />
                    <span className="label-text">Current user</span>
                </label>
            </div>
        </details>
    )
}