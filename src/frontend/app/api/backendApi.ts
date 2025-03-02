import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react'


export const backendApi = createApi({
    baseQuery: fetchBaseQuery({ baseUrl: new URL("/api/v1", import.meta.env.VITE_BACKEND_URL).href }),
    endpoints: () => ({}),
})
