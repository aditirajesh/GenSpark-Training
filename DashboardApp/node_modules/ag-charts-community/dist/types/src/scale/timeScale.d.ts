import type { TimeIntervalUnit } from 'ag-charts-types';
import { TimeInterval } from '../util/time';
import { ContinuousScale } from './continuousScale';
import type { ScaleFormatParams, ScaleTickParams, ScaleTickResult } from './scale';
export declare class TimeScale extends ContinuousScale<Date, TimeInterval | TimeIntervalUnit | number> {
    static is(value: unknown): value is TimeScale;
    readonly type = "time";
    constructor();
    toDomain(d: number): Date;
    convert(value: Date, options?: {
        clamp: boolean;
    }): number;
    invert(value: number): Date;
    niceDomain(ticks: ScaleTickParams<TimeInterval | number>, domain?: Date[]): Date[];
    /**
     * Returns uniformly-spaced dates that represent the scale's domain.
     */
    ticks(params: ScaleTickParams<TimeInterval | number>, domain?: Date[], visibleRange?: [number, number], extend?: boolean): ScaleTickResult<Date> | undefined;
    private _tickFormatter;
    /**
     * Returns a time format function suitable for displaying tick values.
     *
     * @param ticks Optional array of tick values for custom formatting.
     * @param domain Optional array representing the [min, max] values of the time axis.
     * @param specifier Optional format specifier string for custom date formatting (e.g., `%Y`, `%m`, `%d`).
     * @param formatOffset Optional number for applying an offset to the format (e.g., timezone shifts).
     * @returns A function that formats a `Date` object into a string based on the provided specifier or default format.
     */
    tickFormatter(params: ScaleFormatParams<Date>): (date: Date) => string;
    datumFormatter(params: ScaleFormatParams<Date>): (date: Date) => string;
}
export declare function getDateTicksForInterval({ start, stop, interval, availableRange, visibleRange, extend, }: {
    start: number;
    stop: number;
    interval: number | TimeInterval | TimeIntervalUnit;
    availableRange: number;
    visibleRange: [number, number] | undefined;
    extend: boolean;
}): Date[] | undefined;
