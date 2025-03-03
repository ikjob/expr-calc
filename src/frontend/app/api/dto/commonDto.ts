import { QueryListMetadata } from "../../models/metadata";

export type QueryResultMetadataDto = {
    pageNumber?: number | null;
    pageSize?: number | null;
    totalItemsCount?: number | null;
    timeOnServer?: string | null;
}


export type DataBodyDto<T> = {
    data: T;
    metadata?: QueryResultMetadataDto | null;
}


export function convertQueryListMetadataFromDtoToModel(dto?: QueryResultMetadataDto | null) : QueryListMetadata {
    if (!dto) {
        throw new Error("Query metadata was not provided");
    }
    else if (dto.pageNumber != null && dto.pageSize != null && dto.totalItemsCount != null && dto.timeOnServer != null) {
        return {
            pageNumber: dto.pageNumber,
            pageSize: dto.pageSize,
            totalItemsCount: dto.totalItemsCount,
            timeOnServer: Date.parse(dto.timeOnServer)
        }
    }
    else {
        throw new Error("Query metadata does not contain all required fields");
    }
}