import { DateTime, Uuid } from "../common";
import { UserLogin } from "./user";

export type Calculation = {
    id: Uuid;
    expression: string;
    createdBy: UserLogin;
    createdAt: DateTime;
    updatedAt: DateTime;
    status: CalculationStatus;
}

export type CalculationState = "Pending" | "InProgress" | "Cancelled" | "Failed" | "Success";
export type CalculationErrorCode = "UnexpectedError" | "BadExpressionSyntax" | "ArithmeticError";

export type CalculationStatus = {
    state: CalculationState;

    calculationResult?: number | null;

    errorCode?: CalculationErrorCode | null;
    errorDetails?: CalculationErrorDetails | null;

    cancelledBy?: UserLogin | null;
}

export interface CalculationErrorDetails {
    errorCode: string;
    offset?: number | null;
    length?: number | null;
}


export interface CalculationCreateRequest {
    expression: string;
    createdBy: UserLogin;
}

export function createPedningCalculationStatus() : CalculationStatus {
    return {
        state: "Pending",
    }
}
export function createInProgressCalculationStatus() : CalculationStatus {
    return {
        state: "InProgress",
    }
}
export function createSuccessCalculationStatus(calculationResult: number) : CalculationStatus {
    return {
        state: "Success",
        calculationResult: calculationResult
    }
}
export function createFailedCalculationStatus(errorCode: CalculationErrorCode, errorDetails: CalculationErrorDetails) : CalculationStatus {
    return {
        state: "Failed",
        errorCode: errorCode,
        errorDetails: errorDetails
    }
}
export function createCancelledCalculationStatus(cancelledBy: UserLogin) : CalculationStatus {
    return {
        state: "Cancelled",
        cancelledBy: cancelledBy
    }
}