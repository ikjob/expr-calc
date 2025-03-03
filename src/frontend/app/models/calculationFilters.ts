import { DateTime, Uuid } from "../common";
import { CalculationState } from "./calculation";

export type CalculationFilters = {
    id?: Uuid;
    createdBy?: string;

    createdAtMin?: DateTime;
    createdAtMax?: DateTime;

    updatedAtMin?: DateTime;
    updatedAtMax?: DateTime;

    state?: CalculationState;
    expression?: string;
}