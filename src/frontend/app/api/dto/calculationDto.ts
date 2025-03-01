import { Uuid } from "../../common";

export type CalculationDto = {
    id: Uuid;
    expression: string;
    createdBy: string;
    createdAt: Date;
    updatedAt: Date;
    status: CalculationStatusDto;
}

export type CalculationStatusDto = {
    state: "Pending" | "InProgress" | "Cancelled" | "Failed" | "Success";
    calculationResult?: number | null;
    errorCode?: "UnexpectedError" | "BadExpressionSyntax" | "ArithmeticError" | null;
    errorDetails?: ErrorDetailsDto | null;
    cancelledBy?: string | null;
}

export type ErrorDetailsDto = {
    errorCode: string;
    offset?: number | null;
    length?: number | null;
}