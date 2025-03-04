import { 
    CheckCircle as CheckCircleIcon, 
    TimesCircle as TimesCircleIcon, 
    ExclamationCircle as ExclamationCircleIcon, 
    Stop as StopIcon, 
    User as UserIcon } from '@ricons/fa'
import CancelledIcon from './icons/cancelledIcon';
import PendingIcon from './icons/pendingIcon';
import { Calculation, CalculationErrorCode, CalculationState } from '../models/calculation';
import { Uuid } from '../common';
import { ReactNode } from 'react';

interface CalculationsTableProps {
    rows: Calculation[];
    onStop: (id: Uuid) => void;
}

const statusIconPerState = new Map<CalculationState, ReactNode>([
    [ "Pending", <PendingIcon className="text-base-content w-4" /> ],
    [ "InProgress", <span className="loading loading-spinner join-item text-primary w-4"/> ],
    [ "Success", <CheckCircleIcon className="text-success w-4" /> ],
    [ "Failed", <TimesCircleIcon className="text-error w-4" /> ],
    [ "Cancelled", <CancelledIcon className="text-base-content w-4" /> ],
])

const errorNameByErrorCode = new Map<CalculationErrorCode, string>([
    [ "UnexpectedError", "Error" ],
    [ "ArithmeticError", "Arithmetic error"],
    [ "BadExpressionSyntax", "Syntax error"]
])

function getErrorNameByErrorCode(code?: CalculationErrorCode | null) : string {
    if (!code) {
        return "Error";
    }
    return errorNameByErrorCode.get(code) ?? "Error";
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
                        <td className="w-12">{statusIconPerState.get(calculation.status.state) ?? <></>}</td>
                        <td className="min-w-40 w-full text-left line-clamp-1">{calculation.expression}</td>
                        <td className="w-40">{
                            (calculation.status.state == "Success" ? calculation.status.calculationResult :
                            (calculation.status.state == "Failed" ? <>{getErrorNameByErrorCode(calculation.status.errorCode)} <CalculationErrorInfoMarker calculation={calculation} className="relative top-[-0.15em]" /></> : 
                            (calculation.status.state == "Cancelled" ? <>Cancelled <CalculationCancelledInfoMarker calculation={calculation} className="relative top-[-0.15em]" /></> : 
                            <>-</>)))
                        }</td>
                        <td className="w-40 line-clamp-1">{calculation.createdBy}</td>
                        <td className="w-40">{new Date(calculation.createdAt).toLocaleString()}</td>
                        <td className="w-24"> {
                            (calculation.status.state == "Pending" || calculation.status.state == "InProgress") ?
                                <button className="btn btn-xs" onClick={() => onStop(calculation.id)}><StopIcon className="size-[1em]" />Stop</button> :
                                <></>
                        }</td>
                    </tr>
                ))}
            </tbody>
        </table>
    )
}

function CalculationCancelledInfoMarker(props: { calculation: Calculation, className?: string }) {  
    return (
        <div className={`dropdown dropdown-end ${props.className}`}>
            <div tabIndex={0} role="button" className="btn btn-circle btn-ghost btn-xs text-info">
                <ExclamationCircleIcon className="h-4 w-4 stroke-current" />
            </div>
            <div tabIndex={0} className="card card-sm dropdown-content bg-base-100 rounded-box z-1 w-64 shadow-sm border-base-content/25 border-1">
                <div tabIndex={0} className="card-body">
                    <span>
                        Cancelled by: <UserIcon className="w-4 h-[1em] inline relative top-[-0.15em] text-base-content" /> {props.calculation.status.cancelledBy}
                    </span>
                </div>
            </div>
        </div>
    )
}


function CalculationErrorContextView({text, offset, length }: { text: string, offset: number, length: number | null }) {
    const expectedOutLength = 36;

    const correctedOffset = offset < 0 ? 0 : (offset > text.length ? text.length : offset);
    const correctedLength = length ? ((length + correctedOffset) <= text.length ? length : text.length - correctedOffset) : 0;
    
    const beforeLength = Math.min(Math.max(6, (expectedOutLength - correctedLength) / 2), correctedOffset);
    let beforeText = text.substring(correctedOffset - beforeLength, correctedOffset);
    if (correctedOffset - beforeLength > 0) {
        beforeText = ".." + beforeText;
    }

    const highlightLength = Math.min(correctedLength, expectedOutLength - beforeLength);
    const highlightText = text.substring(correctedOffset, correctedOffset + highlightLength);

    const afterLength = Math.min(Math.max(6, (expectedOutLength - correctedLength) / 2), text.length - (correctedOffset + highlightLength));
    let afterText = text.substring(correctedOffset + highlightLength, correctedOffset + highlightLength + afterLength);
    if (correctedOffset + highlightLength + afterLength < text.length) {
        afterText = afterText + "..";
    }

    return (
        <span>{beforeText}<span className="bg-error/50">{highlightText.length > 0 ? highlightText : "\u00A0"}</span>{afterText}</span>
    )
}

function CalculationErrorInfoMarker(props: { calculation: Calculation, className?: string  }) {  
    let errorContext: ReactNode | null = null;
    if (props.calculation.status.errorDetails) {
        const details = props.calculation.status.errorDetails;
        if (details.offset != null) {
            errorContext = CalculationErrorContextView({ text: props.calculation.expression, offset: details.offset, length: details.length ?? null })
        }
    }


    return (
        <div className={`dropdown dropdown-end ${props.className}`}>
            <div tabIndex={0} role="button" className="btn btn-circle btn-ghost btn-xs text-error">
                <ExclamationCircleIcon className="h-4 w-4 stroke-current" />
            </div>
            <div tabIndex={0} className="card card-sm dropdown-content bg-base-100 rounded-box z-1 w-70 shadow-sm border-base-content/25 border-1">
                <div tabIndex={0} className="card-body">
                    <h2 className="card-title">{props.calculation.status.errorDetails?.errorCode ?? "Error"}</h2>
                    { errorContext ?? <></>}
                </div>
            </div>
        </div>
    )
}