import { DateTime } from "../common";

export type QueryListMetadata = {
    pageNumber: number;
    pageSize: number;
    totalItemsCount: number;
    timeOnServer: DateTime;
}


export interface DataListWithMetadata<T> {
    data: T[];
    metadata: QueryListMetadata;
}