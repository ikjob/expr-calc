import { useState } from 'react'
import { useCancelCalculationMutation, useGetCalculationsQuery } from '../api/calculations'
import CalculationsTable from '../components/calculationsTable'
import Pagination from '../components/pagination'
import { CalculationFilters } from '../models/calculationFilters'
import { PaginationParams } from '../models/paginationParams'
import { Uuid } from '../common'
import { useSelector } from 'react-redux'
import { selectActiveUserName } from '../redux/stores/userStore'

const PAGE_SIZE = 10;
const POLLING_INTERVAL_MS = 1500;

export default function CalculationsPage() {  
    const activeUser = useSelector(selectActiveUserName);
    const [filters] = useState({} as CalculationFilters);
    const [pagination, setPagination] = useState({ pageNumber: 1, pageSize: PAGE_SIZE } as PaginationParams);
    const { data: calculationRows, isLoading: calcLoading } = useGetCalculationsQuery({ filters: filters, pagination: pagination }, { pollingInterval: POLLING_INTERVAL_MS, skipPollingIfUnfocused: true });
    const [cancelCalculationApi] = useCancelCalculationMutation();
    //const { data: calculationsDelta } = useGetCalculationUpdatesQuery({ fromTime:  filters: filters })


    function cancelCalculation(id: Uuid) {
        if (activeUser) {
            cancelCalculationApi({ id : id, cancelledBy: activeUser });
        }
    }
    function changePage(pageNumber: number) {
        if (pageNumber > 0 && pageNumber != pagination.pageNumber) {
            setPagination({ pageNumber: pageNumber, pageSize: pagination.pageSize });
        }
    }

    return (
        <div className="mt-2 mx-4">
            <div className="join flex my-4">
                <input type="text" className="input input-bordered input-accent join-item flex-auto" placeholder="Expression" />
                <button className="btn btn-accent join-item rounded-r-full flex-none">Submit</button>
            </div>

            <CalculationsFilter />
          
            <div className="my-4 relative">
                <CalculationsTable rows={calculationRows?.data || []} onStop={cancelCalculation} />
                <div role="status" className={`absolute w-full h-full z-11 bg-base-300/50 top-0 left-0 place-content-center ${!calcLoading ? "hidden" : ""}`}>
                    <div className="text-center">
                        <span className="loading loading-ring loading-xl" />
                    </div>
                </div>
            </div>

            <Pagination 
                activePage={calculationRows?.metadata.pageNumber ?? 1} 
                pageSize={calculationRows?.metadata.pageSize ?? PAGE_SIZE} 
                totalItemsCount={calculationRows?.metadata.totalItemsCount ?? 0} 
                onPageChange={changePage}/>
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