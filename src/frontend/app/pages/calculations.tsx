import CalculationsTable from '../components/calculationsTable/calculationsTable'
import Pagination from '../components/pagination'
import { useGetCalculationByIdQuery } from '../api/calculations'
import { isProblemDetailsError } from '../api/dto/problemDetailsDto'

export default function CalculationsPage() {  
  const { data, error, isLoading } = useGetCalculationByIdQuery("01954f5e-4ae1-7a7f-80ad-a5e482b483a1")

  return (
    <div className="mt-2 mx-4">
      <div className="join flex my-4">
        <input type="text" className="input input-bordered input-accent join-item flex-auto" placeholder="Expression" />
        <button className="btn btn-accent join-item rounded-r-full flex-none">Submit</button>
      </div>

      <div>
      {
        isLoading ? "loading" : JSON.stringify(data) ?? "null"
      }
      </div>
      <div>
        {
           isProblemDetailsError(error) ? (error.type ?? JSON.stringify(error)) : JSON.stringify(error)
        }
      </div>

      <CalculationsFilter />
    
      <CalculationsTable />

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