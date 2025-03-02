import { QueryListMetadata } from "../../models/metadata";

export type QueryResultMetadataDto = {
    pageNumber?: number | null;
    pageSize?: number | null;
    totalItemsCount?: number | null;
    timeOnServer?: Date | null;
}


export type DataBodyDto<T> = {
    data: T;
    metadata?: QueryResultMetadataDto | null;
}


export function convertQueryListMetadataFromDtoToModel(dto?: QueryResultMetadataDto | null) : QueryListMetadata {
    if (!dto) {
        throw new Error("Query metadata was not provided");
    }
    else if (dto.pageNumber && dto.pageSize && dto.totalItemsCount && dto.timeOnServer) {
        return dto as QueryListMetadata;
    }
    else {
        throw new Error("Query metadata does not contain all required fields");
    }
}