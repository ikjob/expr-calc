import { backendApi } from './backendApi'
import { Uuid } from '../common'
import type { CalculationDto } from './dto/calculationDto'
import { DataBodyDto } from './dto/commonDto'

const calculationsTagType = "calculations"

export const calculationsApi = backendApi.enhanceEndpoints({ addTagTypes: [calculationsTagType] }).injectEndpoints({
  endpoints: (builder) => ({
    getCalculationById: builder.query<CalculationDto, Uuid>({
      query: (id) => `api/v1/calculations/${id}`,
      transformResponse: (response: DataBodyDto<CalculationDto>) => response.data,
      providesTags: [calculationsTagType]
    }),
  }),
  overrideExisting: false
})


export const { useGetCalculationByIdQuery } = calculationsApi