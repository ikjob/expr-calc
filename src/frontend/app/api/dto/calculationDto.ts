import { Uuid } from "../../common";
import { Calculation, CalculationCreateRequest, CalculationStatus, createCancelledCalculationStatus, createFailedCalculationStatus, createInProgressCalculationStatus, createPedningCalculationStatus, createSuccessCalculationStatus } from "../../models/calculation";
import { CalculationFilters } from "../../models/calculationFilters";
import { PaginationParams } from "../../models/paginationParams";

export type CalculationDto = {
    id: Uuid;
    expression: string;
    createdBy: string;
    createdAt: string;
    updatedAt: string;
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
        createdAt: Date.parse(dto.createdAt),
        updatedAt: Date.parse(dto.updatedAt),
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
            if (dto.calculationResult == null)
                throw new Error("CalculationResult not provided for success status");
            return createSuccessCalculationStatus(dto.calculationResult);
        case "Failed":
            if (dto.errorCode == null || dto.errorDetails == null)
                throw new Error("ErrorCode not provided for failed status");
            return createFailedCalculationStatus(dto.errorCode, dto.errorDetails);
        case "Cancelled":
            if (dto.cancelledBy == null)
                throw new Error("CancelledBy not provided for failed status");
            return createCancelledCalculationStatus(dto.cancelledBy);
        default:
            throw new Error("Unknown state: " + dto.state);
    }
}


export type CalculationGetListQueryParamsDto = {
    id?: Uuid;
    createdBy?: string;

    createdAtMin?: string;
    createdAtMax?: string;

    updatedAtMin?: string;
    updatedAtMax?: string;

    state?: CalculationStateDto;
    expression?: string;

    pageNumber?: number;
    pageSize?: number;
}

export function convertCalculationFiltersAndPaginationParamsIntoQueryParams(filters?: CalculationFilters, pagination?: PaginationParams) : CalculationGetListQueryParamsDto {
    return {
        id: filters?.id,
        createdBy: filters?.createdBy,
        createdAtMin: filters?.createdAtMin ? new Date(filters.createdAtMin).toISOString() : undefined,
        createdAtMax: filters?.createdAtMax ? new Date(filters?.createdAtMax).toISOString() : undefined,
        updatedAtMin: filters?.updatedAtMin ? new Date(filters?.updatedAtMin).toISOString() : undefined,
        updatedAtMax: filters?.updatedAtMax ? new Date(filters?.updatedAtMax).toISOString() : undefined,
        state: filters?.state,
        expression: filters?.expression,

        pageNumber: pagination?.pageNumber,
        pageSize: pagination?.pageSize
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