import type { TimeIntervalUnit } from 'ag-charts-types';
import { TimeInterval } from '../util/time';
import { DiscreteTimeScale } from './discreteTimeScale';
import type { NormalizedDomain, ScaleFormatParams, ScaleTickParams, ScaleTickResult } from './scale';
export declare class UnitTimeScale extends DiscreteTimeScale {
    static readonly defaultTickCount = 12;
    static is(value: unknown): value is UnitTimeScale;
    readonly type = "unit-time";
    private _domain;
    set domain(domain: Date[]);
    get domain(): Date[];
    private _interval;
    get interval(): TimeInterval | TimeIntervalUnit | undefined;
    set interval(interval: TimeInterval | TimeIntervalUnit | undefined);
    private _bands;
    get bands(): readonly Date[];
    normalizeDomains(...domains: Date[][]): NormalizedDomain<Date>;
    convert(d: Date, options?: {
        interpolate?: boolean;
    }): number;
    private calculateBandRange;
    private calculateBands;
    ticks({ interval }: ScaleTickParams<TimeInterval | TimeIntervalUnit | number>, domain?: Date[], visibleRange?: [number, number], extend?: boolean): ScaleTickResult<Date> | undefined;
    findIndex(value: Date): number | undefined;
    datumFormatter(params: ScaleFormatParams<Date>): (date: Date) => string;
}
