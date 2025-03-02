import { FetchBaseQueryError } from "@reduxjs/toolkit/query";
import { ErrorDetails } from "../../models/errorDetails";

export type ProblemDetailsDto = {
    status?: number | null;
    type?: string | null;
    title?: string | null;
    detail?: string | null;
}

export function convertRawClientErrorToErrorDetails(error: FetchBaseQueryError) : ErrorDetails {
    const result : ErrorDetails = { type: "", status: null, title: null, detail: null };
    let problemDetails: ProblemDetailsDto | null = null;

    if (error.status as number) {
        result.status = error.status as number;
        problemDetails = error.data as ProblemDetailsDto;
    } 
    else if (error.status as string) {
        result.type = error.status as string;
        if ("error" in error) {
            result.detail = error.error;
        }
        if ("originalStatus" in error) {
            result.status = error.originalStatus;
        }
        if ("data" in error) {
            problemDetails = error.data as ProblemDetailsDto;
        }
    }

    if (problemDetails) {
        result.status = result.status ?? problemDetails.status ?? null;
        result.type = result.type || problemDetails.type || (
            result.status ? "http_error_status" : ""
        );
        result.title = result.title || problemDetails.title || (
            knownTypesToTitleMap.get(result.type) ?? (
                result.status ? (httpStatusCodesMap.get(result.status) ?? null) : null
            )
        );
        result.detail = result.detail || problemDetails.detail || null;
    }

    result.type = result.type || "unknown";
    result.title = result.title || "Internal error";
    return result;
}


const knownTypesToTitleMap = new Map<string, string>([
    ["internal", "Internal server error"],
    ["not_found", "Entity not found"],
    ["overflow", "Server is overloaded with requests"],
    ["conflict", "Conflicting entity state detected"]
]);


const httpStatusCodesMap = new Map<number, string>([
    [200, "OK"],
    [201, "Created"],
    [202, "Accepted"],
    [203, "Non-Authoritative Information"],
    [204, "No Content"],
    [205, "Reset Content"],
    [206, "Partial Content"],
    [300, "Multiple Choices"],
    [301, "Moved Permanently"],
    [302, "Found"],
    [303, "See Other"],
    [304, "Not Modified"],
    [305, "Use Proxy"],
    [306, "Unused"],
    [307, "Temporary Redirect"],
    [400, "Bad Request"],
    [401, "Unauthorized"],
    [402, "Payment Required"],
    [403, "Forbidden"],
    [404, "Not Found"],
    [405, "Method Not Allowed"],
    [406, "Not Acceptable"],
    [407, "Proxy Authentication Required"],
    [408, "Request Timeout"],
    [409, "Conflict"],
    [410, "Gone"],
    [411, "Length Required"],
    [412, "Precondition Required"],
    [413, "Request Entry Too Large"],
    [414, "Request-URI Too Long"],
    [415, "Unsupported Media Type"],
    [416, "Requested Range Not Satisfiable"],
    [417, "Expectation Failed"],
    [418, "I\"m a teapot"],
    [429, "Too Many Requests"],
    [500, "Internal Server Error"],
    [501, "Not Implemented"],
    [502, "Bad Gateway"],
    [503, "Service Unavailable"],
    [504, "Gateway Timeout"],
    [505, "HTTP Version Not Supported"],
]);