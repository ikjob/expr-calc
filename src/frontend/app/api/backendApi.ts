import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react'

function getBaseUrl() : string {
    const base = import.meta.env.VITE_BACKEND_URL;
    if (base && base.endsWith("/")) {
        return base + "api/v1";
    } 
    else if (base) {
        return base + "/api/v1";
    }
    else {
        return "/api/v1";
    }
}

export const backendApi = createApi({
    baseQuery: fetchBaseQuery({ baseUrl: getBaseUrl() }),
    endpoints: () => ({}),
})
