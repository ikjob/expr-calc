import { connect, useSelector } from "react-redux";
import { CalculationFilters } from "../models/calculationFilters";
import { useEffect, useRef, useState } from "react";
import { selectActiveUserName } from "../redux/stores/userStore";
import { RootState } from "../redux/store";
import { CalculationState } from "../models/calculation";
import { User as UserIcon, Times as TimesIcon } from '@ricons/fa'

interface CalculationsFilterProps {
    filters: CalculationFilters;
    onChange: (filters: CalculationFilters) => void;
}

function UserFilter({ filters, onChange }: CalculationsFilterProps) {
    const activeUser = useSelector(selectActiveUserName);
    const [isActiveUserCheck, setIsActiveUserCheck] = useState(activeUser === filters.createdBy);
    const userInputRef = useRef<HTMLInputElement>(null);

    useEffect(() => {
        if (activeUser != filters.createdBy && isActiveUserCheck) {
            setIsActiveUserCheck(false);
        }
    }, [activeUser]);

    function toggleActiveUserFilter() {
        const newFilters = { ...filters };
        if (isActiveUserCheck) {
            setIsActiveUserCheck(false);
        } else {
            newFilters.createdBy = activeUser ?? undefined;
            if (userInputRef.current != null && newFilters.createdBy != null) {
                userInputRef.current.value = newFilters.createdBy;
            }
            setIsActiveUserCheck(true);
        }
        onChange(newFilters);
    }
    function onUserNameChanged(newName: string) {
        if (newName !== filters.createdBy) {
            const newFilters = { ...filters };
            newFilters.createdBy = newName.trim() ? newName : undefined;
            onChange(newFilters);
        }
        if (userInputRef.current != null) {
            userInputRef.current.value = newName;
        }
    }

    return (
        <span>
            <label className="input w-70 mr-3">
                <input type="text" className="" placeholder="User name" maxLength={32} disabled={isActiveUserCheck} 
                    ref={userInputRef}
                    defaultValue={filters.createdBy ?? ""}
                    onKeyDown={(e) => { if (e.key == "Enter") { onUserNameChanged((e.target as HTMLInputElement).value); } } }
                    onBlur={(e) => onUserNameChanged((e.target as HTMLInputElement).value) } />
                <button onClick={() => onUserNameChanged("")} disabled={isActiveUserCheck} >
                    <TimesIcon className="w-3 h-3"  />
                </button>
            </label>
            <label className="label cursor-pointer align-middle">
                <input type="checkbox" className="checkbox" 
                    checked={isActiveUserCheck}
                    onChange={toggleActiveUserFilter} />
                <span className="label-text">Current</span>
            </label>
        </span>
    )
}

const UserFilterContextBound = connect((state: RootState) => { return { activeUser: selectActiveUserName(state) } })(UserFilter)


export function CalculationsFilter({ filters, onChange }: CalculationsFilterProps) {
    const expressionInputRef = useRef<HTMLInputElement>(null);
    const dateFromInputRef = useRef<HTMLInputElement>(null);
    const dateToInputRef = useRef<HTMLInputElement>(null);
    const anyFilter = filters.createdBy || filters.state || filters.expression || filters.createdAtMin || filters.createdAtMax || false;

    function onSelectStateChange(newState: string) {
        if (newState != filters.state) {
            const newFilters = { ...filters };
            if (!newState) {
                newFilters.state = undefined;
            }
            else {
                newFilters.state = newState as CalculationState;
            }
            onChange(newFilters);
        }
    }

    function onExpressionChange(newExpr: string) {
        if (newExpr != filters.expression) {
            const newFilters = { ...filters };
            newFilters.expression = newExpr ? newExpr : undefined;
            onChange(newFilters);
            
            if (expressionInputRef.current != null) {
                expressionInputRef.current.value = newExpr;
            }
        }
    }

    function onDateFromChanged(newDateFrom: string) {
        const parsedDate = newDateFrom ? Date.parse(newDateFrom) : 0;
        if (parsedDate != filters.createdAtMin) {
            const newFilters = { ...filters };
            newFilters.createdAtMin = parsedDate ? parsedDate : undefined;
            onChange(newFilters);
            
            if (dateFromInputRef.current != null) {
                dateFromInputRef.current.value = newDateFrom;
            }
        }
    }
    function onDateToChanged(newDateTo: string) {
        const parsedDate = newDateTo ? Date.parse(newDateTo) : 0;
        if (parsedDate != filters.createdAtMax) {
            const newFilters = { ...filters };
            newFilters.createdAtMax = parsedDate ? parsedDate : undefined;
            onChange(newFilters);
            
            if (dateToInputRef.current != null) {
                dateToInputRef.current.value = newDateTo;
            }
        }
    }

    return (
        <details className="collapse collapse-arrow bg-base-200 border-base-300 rounded-md my-4">
            <summary className="collapse-title">
                <span className="align-middle">Filters:</span>
                { !anyFilter ? <div className="badge badge-lg mx-4">All</div> : <></>}
                { filters.expression ? <div className="badge badge-lg mx-4 max-w-120"><span className="text-info">Expression:</span><span className="truncate">{filters.expression.substring(0, Math.min(120, filters.expression.length))}</span></div> : <></>}
                { filters.createdBy ? <div className="badge badge-lg mx-4"><span className="text-info">Submitted by:</span> <UserIcon className="w-4 h-[1em] inline relative text-base-content" /> {filters.createdBy}</div> : <></>}
                { filters.createdAtMin ? <div className="badge badge-lg mx-4"><span className="text-info">Submitted after:</span>{new Date(filters.createdAtMin).toLocaleString()}</div> : <></>}
                { filters.createdAtMax ? <div className="badge badge-lg mx-4"><span className="text-info">Submitted before:</span>{new Date(filters.createdAtMax).toLocaleString()}</div> : <></>}
                { filters.state ? <div className="badge badge-lg mx-4"><span className="text-info">State:</span> {filters.state}</div> : <></>}
            </summary>
            <div className="collapse-content mt-2">
                <div className="mt-2">
                    <span className="mr-5 align-middle">Expression: </span>
                    <label className="input w-150 mr-3">
                        <input type="text" className="" placeholder="Expression substring" maxLength={25000}
                            ref={expressionInputRef}
                            defaultValue={filters.expression ?? ""}
                            onKeyDown={(e) => { if (e.key == "Enter") { onExpressionChange((e.target as HTMLInputElement).value); } } }
                            onBlur={(e) => onExpressionChange((e.target as HTMLInputElement).value) } />
                        <button onClick={() => onExpressionChange("")}>
                            <TimesIcon className="w-3 h-3"  />
                        </button>
                    </label>
                </div>
                <div className="mt-2">
                    <span className="mr-16 align-middle">User: </span>
                    <UserFilterContextBound filters={filters} onChange={onChange} />
                </div>
                <div className="mt-2">
                    <span className="mr-1 align-middle">Submitted at: </span>
                    <label className="input w-70">
                        <input type="datetime-local" className=""
                            ref={dateFromInputRef}
                            defaultValue={filters.createdAtMin ? new Date(filters.createdAtMin).toLocaleString() : undefined}
                            onKeyDown={(e) => { if (e.key == "Enter") { onDateFromChanged((e.target as HTMLInputElement).value); } } }
                            onBlur={(e) => onDateFromChanged((e.target as HTMLInputElement).value) } />
                        <button onClick={() => onDateFromChanged("")}>
                            <TimesIcon className="w-3 h-3"  />
                        </button>
                    </label>
                    <span className="mx-3 align-middle">to</span>
                    <label className="input w-70 mr-3">
                        <input type="datetime-local" className=""
                            ref={dateToInputRef}
                            defaultValue={filters.createdAtMin ? new Date(filters.createdAtMin).toLocaleString() : undefined}
                            onKeyDown={(e) => { if (e.key == "Enter") { onDateToChanged((e.target as HTMLInputElement).value); } } }
                            onBlur={(e) => onDateToChanged((e.target as HTMLInputElement).value) } />
                        <button onClick={() => onDateToChanged("")}>
                            <TimesIcon className="w-3 h-3"  />
                        </button>
                    </label>
                </div>
                <div className="mt-2">
                    <span className="mr-15 align-middle">State: </span>
                    <select defaultValue="Pick a status" className="select select-sm w-70" onChange={(e) => onSelectStateChange(e.target.value)}>
                        <option value="">---</option>
                        <option value="Pending">Pending</option>
                        <option value="InProgress">In Progress</option>
                        <option value="Success">Success</option>
                        <option value="Failed">Failed</option>
                        <option value="Cancelled">Cancelled</option>
                    </select>
                </div>
            </div>
        </details>
    )
}