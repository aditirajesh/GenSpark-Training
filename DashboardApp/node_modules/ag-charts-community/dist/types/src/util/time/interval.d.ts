import type { TimeIntervalUnit } from 'ag-charts-types';
/**
 * Converts the specified Date into a count of years,
 * days, hours etc. passed since some base date.
 */
type EncodeFn = (date: Date) => number;
/**
 * Converts the count of years, days, hours etc.
 * since a base date into another Date.
 */
type DecodeFn = (encoded: number) => Date;
/**
 * A function to be executed before the range calculation.
 * Returns a callback to be executed after the range calculation.
 */
type RangeFn = (start: Date, end: Date) => () => void;
interface RangeParams {
    extend?: boolean;
    visibleRange?: [number, number];
    limit?: number;
}
interface EveryParams {
    snapTo?: Date | number | 'start' | 'end';
}
/**
 * The interval methods don't mutate Date parameters.
 */
export declare class TimeInterval {
    readonly unit: TimeIntervalUnit;
    readonly milliseconds: number;
    readonly hierarchy: TimeInterval | undefined;
    protected readonly _encode: EncodeFn;
    protected readonly _decode: DecodeFn;
    readonly step: number;
    protected readonly _rangeCallback?: RangeFn | undefined;
    constructor(unit: TimeIntervalUnit, milliseconds: number, hierarchy: TimeInterval | undefined, _encode: EncodeFn, _decode: DecodeFn, step?: number, _rangeCallback?: RangeFn | undefined);
    static extent(start: Date, stop: Date, visibleRange?: [number, number]): Date[];
    private getOffset;
    /**
     * Returns a new date representing the latest interval boundary date before or equal to date.
     * For example, `day.floor(date)` typically returns 12:00 AM local time on the given date.
     * @param date
     */
    floor(date: Date | number): Date;
    /**
     * Returns a new date representing the earliest interval boundary date after or equal to date.
     * @param date
     */
    ceil(date: Date | number): Date;
    private rangeIndices;
    /**
     * Returns an array of dates representing every interval boundary after or equal to start (inclusive) and before stop (exclusive).
     * @param start Range start.
     * @param stop Range end.
     * @param extend If specified, the requested range will be extended to the closest "nice" values.
     */
    range(start: Date, stop: Date, params?: RangeParams): Date[];
    previous(date: Date): Date;
    next(date: Date): Date;
    rangeCount(start: Date, stop: Date, params?: RangeParams): number;
    /**
     * Returns a filtered view of this interval representing every step'th date.
     * It can be a number of minutes, hours, days etc.
     * Must be a positive integer.
     * @param step
     */
    every(step: number, options?: EveryParams): TimeInterval;
}
export {};
