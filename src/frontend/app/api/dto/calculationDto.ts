import { Uuid } from "../../common";
import { Calculation, CalculationCreateRequest, CalculationStatus, createCancelledCalculationStatus, createFailedCalculationStatus, createInProgressCalculationStatus, createPedningCalculationStatus, createSuccessCalculationStatus } from "../../models/calculation";
import { CalculationFilters } from "../../models/calculationFilters";
import { PaginationParams } from "../../models/paginationParams";

export type CalculationDto = {
    id: Uuid;
    expression: string;
    createdBy: string;
    createdAt: Date;
    updatedAt: Date;
    status: CalculationStatusDto;
}

export type CalculationStateDto = "Pending" | "InProgress" | "Cancelled" | "Failed" | "Success";
export type CalculationErrorCodeDto = "UnexpectedError" | "BadExpressionSyntax" | "ArithmeticError";

export type CalculationStatusDto = {
    state: CalculationStateDto;
    calculationResult?: number | null;
    errorCode?: CalculationErrorCodeDto | null;
    errorDetails?: CalculationErrorDetailsDto | null;
    cancelledBy?: string | null;
}

export type CalculationErrorDetailsDto = {
    errorCode: string;
    offset?: number | null;
    length?: number | null;
}


export function convertCalculationFromDtoToModel(dto: CalculationDto) : Calculation {
    return {
        id: dto.id,
        expression: dto.expression,
        createdBy: dto.createdBy,
        createdAt: dto.createdAt,
        updatedAt: dto.updatedAt,
        status: convertCalculationStatusFromDtoToModel(dto.status) 
    }
}

export function convertCalculationStatusFromDtoToModel(dto: CalculationStatusDto) : CalculationStatus {
    switch (dto.state) {
        case "Pending":
            return createPedningCalculationStatus();
        case "InProgress":
            return createInProgressCalculationStatus();
        case "Success":
            if (!dto.calculationResult)
                throw new Error("CalculationResult not provided for success status");
            return createSuccessCalculationStatus(dto.calculationResult);
        case "Failed":
            if (!dto.errorCode || !dto.errorDetails)
                throw new Error("ErrorCode not provided for failed status");
            return createFailedCalculationStatus(dto.errorCode, dto.errorDetails);
        case "Cancelled":
            if (!dto.cancelledBy)
                throw new Error("CancelledBy not provided for failed status");
            return createCancelledCalculationStatus(dto.cancelledBy);
        default:
            throw new Error("Unknown state: " + dto.state);
    }
}


export type CalculationGetListQueryParamsDto = {
    id?: Uuid;
    createdBy?: string;

    createdAtMin?: Date;
    createdAtMax?: Date;

    updatedAtMin?: Date;
    updatedAtMax?: Date;

    state?: CalculationStateDto;
    expression?: string;

    pageNumber?: number;
    pageSize?: number;
}

export function convertCalculationFiltersAndPaginationParamsIntoQueryParams(params: CalculationFilters & PaginationParams) : CalculationGetListQueryParamsDto {
    return {
        id: params.id,
        createdBy: params.createdBy,
        createdAtMin: params.createdAtMin,
        createdAtMax: params.createdAtMax,
        updatedAtMin: params.updatedAtMin,
        updatedAtMax: params.updatedAtMax,
        state: params.state,
        expression: params.expression,

        pageNumber: params.pageNumber,
        pageSize: params.pageSize
    }
}


export type CalculationCreateDto = {
    expression: string;
    createdBy: string;
}

export function convertCalculationCreateFromModelToDto(model: CalculationCreateRequest) : CalculationCreateDto {
    return {
        expression: model.expression,
        createdBy: model.createdBy
    }
}