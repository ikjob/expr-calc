export type ProblemDetailsDto = {
    type?: string | null;
    status?: number | null;
    title?: string | null;
    detail? : string | null;
}

/* eslint-disable  @typescript-eslint/no-explicit-any */
export function isProblemDetailsError(error: unknown): error is ProblemDetailsDto {
    return (
        typeof error === "object" &&
        error != null &&
        "status" in error &&
        typeof (error as any).status === "number"
      )
}