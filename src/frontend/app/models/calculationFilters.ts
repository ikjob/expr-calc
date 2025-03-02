import { Uuid } from "../common";
import { CalculationState } from "./calculation";

export type CalculationFilters = {
    id?: Uuid;
    createdBy?: string;

    createdAtMin?: Date;
    createdAtMax?: Date;

    updatedAtMin?: Date;
    updatedAtMax?: Date;

    state?: CalculationState;
    expression?: string;
}