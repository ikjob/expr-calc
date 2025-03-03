import { useEffect, useRef, useState } from 'react'
import { useCancelCalculationMutation, useCreateCalculationMutation, useGetCalculationsQuery } from '../api/calculations'
import CalculationsTable from '../components/calculationsTable'
import Pagination from '../components/pagination'
import { User as UserIcon } from '@ricons/fa'
import { CalculationFilters } from '../models/calculationFilters'
import { PaginationParams } from '../models/paginationParams'
import { Uuid } from '../common'
import { useSelector } from 'react-redux'
import { selectActiveUserName } from '../redux/stores/userStore'

const PAGE_SIZE = 10;
const POLLING_INTERVAL_MS = 1500;

export default function CalculationsPage() {  
    const activeUser = useSelector(selectActiveUserName);
    const [filters, setFilters] = useState({} as CalculationFilters);
    const [pagination, setPagination] = useState({ pageNumber: 1, pageSize: PAGE_SIZE } as PaginationParams);
    const { data: calculationRows, isLoading: calcLoading } = useGetCalculationsQuery({ filters: filters, pagination: pagination }, { pollingInterval: POLLING_INTERVAL_MS, skipPollingIfUnfocused: true });
    const [createCalculationApi] = useCreateCalculationMutation();
    const [cancelCalculationApi] = useCancelCalculationMutation();
    //const { data: calculationsDelta } = useGetCalculationUpdatesQuery({ fromTime:  filters: filters })


    function createCalculation(expression: string) {
        if (activeUser) {
            createCalculationApi({ expression: expression, createdBy: activeUser });
        }
    }
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
            <CalculationSubmit onSubmitExpr={createCalculation} />

            <CalculationsFilter filters={filters} onChange={(newFilters) => setFilters(newFilters)} />
          
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


interface CalculationSubmitProps {
    onSubmitExpr: (expression: string) => void;
}

function CalculationSubmit(props: CalculationSubmitProps) {
    const currentInput = useRef("");
    const [inputErrorStatus, setInputErrorStatus] = useState({error: "", counter: 0});

    useEffect(() => {
        if (inputErrorStatus.error) {
          const timeout = setTimeout(() => setInputErrorStatus({error: "", counter: inputErrorStatus.counter }), 5000);
          return () => {
            clearTimeout(timeout);
          };
        }
      }, [inputErrorStatus]);


    function submitExpression() {
        if (currentInput.current != null && currentInput.current != "" && currentInput.current.trim() != "") {
            props.onSubmitExpr(currentInput.current);
            setInputErrorStatus({ error: "", counter: inputErrorStatus.counter });
        } 
        else {
            setInputErrorStatus({ error: "Expression cannot be empty", counter: inputErrorStatus.counter + 1 });
        }
    }

    return (
        <div className="mt-4 mb-0">
            <div className="join flex">
                <input type="text" className="input input-bordered input-accent join-item flex-auto" placeholder="Expression" onChange={e => { currentInput.current = e.target.value; }} />
                <button className="btn btn-accent join-item rounded-r-full flex-none" onClick={submitExpression}>Submit</button>
            </div>
            <span className="validator-hint text-error ml-1 mt-1 mb-0">
                {inputErrorStatus.error}
            </span>
        </div>
    )
}



interface CalculationsFilterProps {
    filters: CalculationFilters;
    onChange: (filters: CalculationFilters) => void;
}

function CalculationsFilter({ filters, onChange }: CalculationsFilterProps) {
    const activeUser = useSelector(selectActiveUserName);
    const anyFilter = filters.createdBy || false;

    function toggleActiveUserFilter() {
        const newFilters = { ...filters };
        if (newFilters.createdBy || activeUser === null) {
            newFilters.createdBy = undefined;
        } else {
            newFilters.createdBy = activeUser;
        }
        onChange(newFilters);
    }

    return (
        <details className="collapse collapse-arrow bg-base-200 border-base-300 rounded-md my-4 flex">
            <summary className="collapse-title">
                <span className="align-middle">Filters:</span>
                { !anyFilter ? <div className="badge badge-lg mx-4">All</div> : <></>}
                { filters.createdBy ? <div className="badge badge-lg mx-4"><span className="text-info">Submitted by:</span> <UserIcon className="w-4 h-[1em] inline relative text-base-content" /> {filters.createdBy}</div> : <></>}
            </summary>
            <div className="collapse-content">
                <label className="label cursor-pointer">
                    <input type="checkbox" className="checkbox" 
                        checked={filters.createdBy == activeUser}
                        onChange={toggleActiveUserFilter} />
                    <span className="label-text">Current user</span>
                </label>
            </div>
        </details>
    )
}