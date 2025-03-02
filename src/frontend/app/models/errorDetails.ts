export type ErrorDetails = {
    type: string;
    status: number | null;
    title: string | null;
    detail: string | null;
}

export function isErrorDetails(rawError: unknown) : rawError is ErrorDetails {
    if (typeof rawError === "object") {
        const obj = rawError as object;
        return obj != null &&
            "type" in obj &&
            typeof obj.type === "string" &&
            "status" in obj &&
            "title" in obj &&
            "detail" in obj;
    }
    return false;
}