import CalculationsTable from '../components/calculationsTable/calculationsTable'
import Pagination from '../components/pagination'

export default function CalculationsPage() {  
  return (
    <div className="mt-2 mx-4">
      <div className="join flex my-4">
        <input type="text" className="input input-bordered input-accent join-item flex-auto" placeholder="Expression" />
        <button className="btn btn-accent join-item rounded-r-full flex-none">Submit</button>
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
      <input type="checkbox" className="checkbox" checked={true} />
      <span className="label-text">Current user</span>
    </label>
    </div>
  </details>
  )
}