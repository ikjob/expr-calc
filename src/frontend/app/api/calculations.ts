import { backendApi } from './backendApi'
import { Uuid } from '../common'
import { CalculationDto, convertCalculationCreateFromModelToDto, convertCalculationFiltersAndPaginationParamsIntoQueryParams, convertCalculationFromDtoToModel } from './dto/calculationDto'
import { convertQueryListMetadataFromDtoToModel, DataBodyDto } from './dto/commonDto'
import { Calculation, CalculationCreateRequest } from '../models/calculation'
import { convertRawClientErrorToErrorDetails } from './dto/problemDetailsDto'
import { DataListWithMetadata } from '../models/metadata'
import { CalculationFilters } from '../models/calculationFilters'
import { PaginationParams } from '../models/paginationParams'
import { FetchArgs } from '@reduxjs/toolkit/query'
import { UserLogin } from '../models/user'

const calculationsTagType = "calculations"

export const calculationsApi = backendApi.enhanceEndpoints({ addTagTypes: [calculationsTagType] }).injectEndpoints({
    endpoints: (builder) => ({
        getCalculationById: builder.query<Calculation, Uuid>({
            query: (id) => `/calculations/${id}`,
            transformResponse: (response: DataBodyDto<CalculationDto>) => convertCalculationFromDtoToModel(response.data),
            transformErrorResponse: (baseQueryReturnValue, _meta, _arg) => convertRawClientErrorToErrorDetails(baseQueryReturnValue),
            providesTags: (_result, _error, id) => [{ type: calculationsTagType, id }],
        }),

        getCalculations: builder.query<DataListWithMetadata<Calculation>, CalculationFilters & PaginationParams | null>({
            query: (args) : FetchArgs => {
                return {
                    url: "/calculations",
                    params: args ? convertCalculationFiltersAndPaginationParamsIntoQueryParams(args) : undefined
                }
            },
            transformResponse: (response: DataBodyDto<CalculationDto[]>) => {
                return {
                    data: response.data.map(item => convertCalculationFromDtoToModel(item)),
                    metadata: convertQueryListMetadataFromDtoToModel(response.metadata)
                }
            },
            transformErrorResponse: (baseQueryReturnValue, _meta, _arg) => convertRawClientErrorToErrorDetails(baseQueryReturnValue),
            providesTags: [calculationsTagType],
        }),

        getCalculationUpdates: builder.query<DataListWithMetadata<Calculation>, { fromTime: Date, filters: CalculationFilters | null }>({
            query: (args) : FetchArgs => {
                const queryParams = (args.filters ? convertCalculationFiltersAndPaginationParamsIntoQueryParams(args.filters) : null) ?? { };
                queryParams.updatedAtMin = args.fromTime;
                queryParams.pageNumber = undefined;
                queryParams.pageSize = undefined;
                return {
                    url: "/calculations",
                    params: queryParams
                }
            },
            transformResponse: (response: DataBodyDto<CalculationDto[]>) => {
                return {
                    data: response.data.map(item => convertCalculationFromDtoToModel(item)),
                    metadata: convertQueryListMetadataFromDtoToModel(response.metadata)
                }
            },
            transformErrorResponse: (baseQueryReturnValue, _meta, _arg) => convertRawClientErrorToErrorDetails(baseQueryReturnValue),
            forceRefetch: () => true,
            providesTags: [calculationsTagType],
        }),

        createCalculation: builder.mutation<Calculation, CalculationCreateRequest>({
            query: (newCalc) => ({
                url: "/calculations",
                method: "POST",
                body: convertCalculationCreateFromModelToDto(newCalc),
            }),
            transformResponse: (response: CalculationDto) => convertCalculationFromDtoToModel(response),
            transformErrorResponse: (baseQueryReturnValue, _meta, _arg) => convertRawClientErrorToErrorDetails(baseQueryReturnValue),
            invalidatesTags: [calculationsTagType],
        }),

        cancelCalculation: builder.mutation<void, { id: Uuid; cancelledBy: UserLogin }>({
            query: ({ id, cancelledBy }) => ({
                url: `/calculations/${id}/status`,
                method: "PUT",
                body: { state: "Cancelled", cancelledBy: cancelledBy },
            }),
            transformErrorResponse: (baseQueryReturnValue, _meta, _arg) => convertRawClientErrorToErrorDetails(baseQueryReturnValue),
            invalidatesTags: (_result, _error, { id }) => [{ type: calculationsTagType, id }],
        }),
    }),
    overrideExisting: false
})


export const { 
    useGetCalculationByIdQuery, 
    useGetCalculationsQuery, 
    useGetCalculationUpdatesQuery, 
    useCreateCalculationMutation, 
    useCancelCalculationMutation } = calculationsApi