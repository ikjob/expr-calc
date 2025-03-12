import React, { useEffect, useRef, useState } from 'react'
import { useCancelCalculationMutation, useCreateCalculationMutation, useGetCalculationsQuery } from '../api/calculations'
import CalculationsTable from '../components/calculationsTable'
import Pagination from '../components/pagination'
import { CalculationFilters } from '../models/calculationFilters'
import { PaginationParams } from '../models/paginationParams'
import { Uuid } from '../common'
import { useSelector } from 'react-redux'
import { selectActiveUserName } from '../redux/stores/userStore'
import AlertBar, { AlertBarSeverity, AlertContent, AlertItem } from '../components/alertBar'
import { ErrorDetails, isErrorDetails } from '../models/errorDetails'
import { CalculationsFilter } from '../components/calculationsFilter'

const PAGE_SIZE = 10;
const POLLING_INTERVAL_MS = 1500;

export default function CalculationsPage() {  
    const activeUser = useSelector(selectActiveUserName);
    const [filters, setFilters] = useState({} as CalculationFilters);
    const [pagination, setPagination] = useState({ pageNumber: 1, pageSize: PAGE_SIZE } as PaginationParams);
    const { data: calculationRows, isLoading: calcLoading, error: calcError, startedTimeStamp: calcTimestamp } = useGetCalculationsQuery({ filters: filters, pagination: pagination }, { pollingInterval: POLLING_INTERVAL_MS, skipPollingIfUnfocused: true });
    const [createCalculationApi, {error: createError, startedTimeStamp: createTimestamp}] = useCreateCalculationMutation();
    const [cancelCalculationApi, {error: cancelError, startedTimeStamp: cancelTimestamp}] = useCancelCalculationMutation();
    //const { data: calculationsDelta } = useGetCalculationUpdatesQuery({ fromTime:  filters: filters })

    const alertingErrors : AlertItem[] = [];
    if (calcError) {
        alertingErrors.push(convertErrorDataToAlertItem("Fetch", calcError, calcTimestamp));
    }
    if (createError) {
        alertingErrors.push(convertErrorDataToAlertItem("Submit", createError, createTimestamp));
    }
    if (cancelError) {
        alertingErrors.push(convertErrorDataToAlertItem("Cancel", cancelError, cancelTimestamp));
    }

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

            <AlertBar items={alertingErrors} />
        </div>
    )
}

function convertErrorDataToAlertItem(operation: "Fetch" | "Submit" | "Cancel", error: ErrorDetails | unknown, key?: React.Key) : AlertItem {
    let severity: AlertBarSeverity = "error";
    const content: AlertContent = {
        title: operation + " error",
        detail: "Internal error"
    };

    if (isErrorDetails(error)) {
        content.detail = error.detail ?? error.title;
        if (error.type == "overflow" && operation == "Submit") {
            severity = "warn";
            content.title = operation + " warning";
            content.detail = "Server overloaded. Make a new attempt later";
        }
        else if (error.type == "conflict" && operation == "Cancel") {
            severity = "warn";
            content.title = operation + " warning";
            content.detail = "Calculation finished or canceled by other user";
        }
    }

    return {
        key: key,
        severity: severity,
        value: content
    }
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
            if (currentInput.current.length <= 25000) {
                props.onSubmitExpr(currentInput.current);
                setInputErrorStatus({ error: "", counter: inputErrorStatus.counter });
            }
            else {
                setInputErrorStatus({ error: "The expression is too long. The maximum allowed length is 25000 symbols", counter: inputErrorStatus.counter + 1 });
            }
        } 
        else {
            setInputErrorStatus({ error: "Expression cannot be empty", counter: inputErrorStatus.counter + 1 });
        }
    }

    return (
        <div className="mt-4 mb-0">
            <div className="join flex">
                <input type="text" className="input input-bordered input-accent join-item flex-auto" 
                placeholder="Expression" maxLength={25001} 
                onChange={e => { currentInput.current = e.target.value; }}
                onKeyDown={(e) => { if (e.key == "Enter") { submitExpression(); } }} />
                <button className="btn btn-accent join-item rounded-r-full flex-none" onClick={submitExpression}>Submit</button>
            </div>
            <span className="validator-hint text-error ml-1 mt-1 mb-0">
                {inputErrorStatus.error}
            </span>
        </div>
    )
}
