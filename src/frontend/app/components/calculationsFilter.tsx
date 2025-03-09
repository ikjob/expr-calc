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
                    value={isActiveUserCheck ? (filters.createdBy ?? "") : undefined} 
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
    const anyFilter = filters.createdBy || filters.state || filters.expression || false;

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

    return (
        <details className="collapse collapse-arrow bg-base-200 border-base-300 rounded-md my-4">
            <summary className="collapse-title">
                <span className="align-middle">Filters:</span>
                { !anyFilter ? <div className="badge badge-lg mx-4">All</div> : <></>}
                { filters.expression ? <div className="badge badge-lg mx-4 max-w-120"><span className="text-info">Expression:</span><span className="truncate">{filters.expression.substring(0, Math.min(120, filters.expression.length))}</span></div> : <></>}
                { filters.createdBy ? <div className="badge badge-lg mx-4"><span className="text-info">Submitted by:</span> <UserIcon className="w-4 h-[1em] inline relative text-base-content" /> {filters.createdBy}</div> : <></>}
                { filters.state ? <div className="badge badge-lg mx-4"><span className="text-info">State:</span> {filters.state}</div> : <></>}
            </summary>
            <div className="collapse-content mt-2">
                <div className="mt-2">
                    <span className="mr-1 align-middle">Expression: </span>
                    <label className="input w-100 mr-3">
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
                    <span className="mr-12 align-middle">User: </span>
                    <UserFilterContextBound filters={filters} onChange={onChange} />
                </div>
                <div className="mt-2">
                    <span className="mr-11 align-middle">State: </span>
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