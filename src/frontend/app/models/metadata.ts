export type QueryListMetadata = {
    pageNumber: number;
    pageSize: number;
    totalItemsCount: number;
    timeOnServer: Date;
}


export interface DataListWithMetadata<T> {
    data: T[];
    metadata: QueryListMetadata;
}